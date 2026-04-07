using ISMSponsor.Data;
using ISMSponsor.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ISMSponsor.Services;

/// <summary>
/// Service for detecting potential duplicate sponsors using multiple matching algorithms.
/// Does not auto-merge, only flags candidates for Admin review.
/// </summary>
public class DuplicateDetectionService
{
    private readonly AppDbContext _context;

    public DuplicateDetectionService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Scans all active sponsors for potential duplicates and creates candidate records.
    /// Returns the number of new candidates found.
    /// </summary>
    public async Task<int> ScanForDuplicatesAsync(string detectedByUserId)
    {
        var activeSponsors = await _context.Sponsors
            .Where(s => s.IsActive && !s.IsMerged)
            .Include(s => s.Addresses)
            .ToListAsync();

        var existingPairs = await _context.SponsorDuplicateCandidates
            .Where(c => c.Status == "Pending" || c.Status == "MergeScheduled")
            .Select(c => new { c.PrimarySponsorId, c.DuplicateSponsorId })
            .ToListAsync();

        var existingPairSet = existingPairs
            .Select(p => $"{p.PrimarySponsorId}|{p.DuplicateSponsorId}")
            .ToHashSet();

        var newCandidates = new List<SponsorDuplicateCandidate>();

        for (int i = 0; i < activeSponsors.Count; i++)
        {
            for (int j = i + 1; j < activeSponsors.Count; j++)
            {
                var sponsorA = activeSponsors[i];
                var sponsorB = activeSponsors[j];

                var pairKey1 = $"{sponsorA.SponsorId}|{sponsorB.SponsorId}";
                var pairKey2 = $"{sponsorB.SponsorId}|{sponsorA.SponsorId}";

                if (existingPairSet.Contains(pairKey1) || existingPairSet.Contains(pairKey2))
                    continue;

                var (score, reasons, explanation) = CalculateMatch(sponsorA, sponsorB);

                if (score >= 60.0m) // Threshold for flagging
                {
                    var candidate = new SponsorDuplicateCandidate
                    {
                        PrimarySponsorId = sponsorA.SponsorId,
                        DuplicateSponsorId = sponsorB.SponsorId,
                        MatchScore = score,
                        MatchReasons = string.Join(", ", reasons),
                        MatchExplanation = explanation,
                        Status = "Pending",
                        DetectedOn = DateTime.UtcNow,
                        DetectedByUserId = detectedByUserId
                    };

                    newCandidates.Add(candidate);
                }
            }
        }

        if (newCandidates.Any())
        {
            _context.SponsorDuplicateCandidates.AddRange(newCandidates);
            await _context.SaveChangesAsync();
        }

        return newCandidates.Count;
    }

    /// <summary>
    /// Checks if a specific sponsor is a potential duplicate of existing sponsors.
    /// Useful when creating or importing a new sponsor.
    /// </summary>
    public async Task<List<SponsorDuplicateCandidate>> CheckSponsorForDuplicatesAsync(
        string sponsorId, string detectedByUserId)
    {
        var targetSponsor = await _context.Sponsors
            .Include(s => s.Addresses)
            .FirstOrDefaultAsync(s => s.SponsorId == sponsorId);

        if (targetSponsor == null || targetSponsor.IsMerged)
            return new List<SponsorDuplicateCandidate>();

        var otherSponsors = await _context.Sponsors
            .Where(s => s.IsActive && !s.IsMerged && s.SponsorId != sponsorId)
            .Include(s => s.Addresses)
            .ToListAsync();

        var candidates = new List<SponsorDuplicateCandidate>();

        foreach (var other in otherSponsors)
        {
            var (score, reasons, explanation) = CalculateMatch(targetSponsor, other);

            if (score >= 60.0m)
            {
                candidates.Add(new SponsorDuplicateCandidate
                {
                    PrimarySponsorId = sponsorId,
                    DuplicateSponsorId = other.SponsorId,
                    MatchScore = score,
                    MatchReasons = string.Join(", ", reasons),
                    MatchExplanation = explanation,
                    Status = "Pending",
                    DetectedOn = DateTime.UtcNow,
                    DetectedByUserId = detectedByUserId
                });
            }
        }

        if (candidates.Any())
        {
            _context.SponsorDuplicateCandidates.AddRange(candidates);
            await _context.SaveChangesAsync();
        }

        return candidates;
    }

    /// <summary>
    /// Calculates match score between two sponsors using multiple algorithms.
    /// Returns (score, reasons list, explanation text).
    /// </summary>
    private (decimal score, List<string> reasons, string explanation) CalculateMatch(
        Sponsor sponsorA, Sponsor sponsorB)
    {
        decimal score = 0.0m;
        var reasons = new List<string>();
        var explanationParts = new List<string>();

        // 1. Exact TIN match (very strong signal) - 40 points
        if (!string.IsNullOrWhiteSpace(sponsorA.Tin) &&
            !string.IsNullOrWhiteSpace(sponsorB.Tin) &&
            sponsorA.Tin.Equals(sponsorB.Tin, StringComparison.OrdinalIgnoreCase))
        {
            score += 40.0m;
            reasons.Add("Same TIN");
            explanationParts.Add($"Both have TIN: {sponsorA.Tin}");
        }

        // 2. External ID matches (strong signal) - 25 points each
        if (!string.IsNullOrWhiteSpace(sponsorA.PowerSchoolId) &&
            sponsorA.PowerSchoolId == sponsorB.PowerSchoolId)
        {
            score += 25.0m;
            reasons.Add("Same PowerSchool ID");
            explanationParts.Add($"PowerSchool ID: {sponsorA.PowerSchoolId}");
        }

        if (!string.IsNullOrWhiteSpace(sponsorA.NetSuiteId) &&
            sponsorA.NetSuiteId == sponsorB.NetSuiteId)
        {
            score += 25.0m;
            reasons.Add("Same NetSuite ID");
            explanationParts.Add($"NetSuite ID: {sponsorA.NetSuiteId}");
        }

        if (!string.IsNullOrWhiteSpace(sponsorA.StudentChargingPortalId) &&
            sponsorA.StudentChargingPortalId == sponsorB.StudentChargingPortalId)
        {
            score += 25.0m;
            reasons.Add("Same SCP ID");
            explanationParts.Add($"SCP ID: {sponsorA.StudentChargingPortalId}");
        }

        // 3. Sponsor name similarity (medium signal) - up to 30 points
        var nameSimilarity = CalculateNameSimilarity(sponsorA.SponsorName, sponsorB.SponsorName);
        if (nameSimilarity >= 0.85)
        {
            var namePoints = (decimal)(nameSimilarity * 30.0);
            score += namePoints;
            reasons.Add($"Similar name ({nameSimilarity:P0})");
            explanationParts.Add($"Names: '{sponsorA.SponsorName}' vs '{sponsorB.SponsorName}'");
        }

        // 4. Legal name similarity (if both have legal names) - up to 20 points
        if (!string.IsNullOrWhiteSpace(sponsorA.LegalName) &&
            !string.IsNullOrWhiteSpace(sponsorB.LegalName))
        {
            var legalSimilarity = CalculateNameSimilarity(sponsorA.LegalName, sponsorB.LegalName);
            if (legalSimilarity >= 0.85)
            {
                var legalPoints = (decimal)(legalSimilarity * 20.0);
                score += legalPoints;
                reasons.Add($"Similar legal name ({legalSimilarity:P0})");
                explanationParts.Add($"Legal: '{sponsorA.LegalName}' vs '{sponsorB.LegalName}'");
            }
        }

        // 5. Address similarity (weaker signal) - up to 15 points
        var addressA = sponsorA.Addresses?.FirstOrDefault(a => a.AddressType == "Billing") ??
                       sponsorA.Addresses?.FirstOrDefault();
        var addressB = sponsorB.Addresses?.FirstOrDefault(a => a.AddressType == "Billing") ??
                       sponsorB.Addresses?.FirstOrDefault();

        if (addressA != null && addressB != null)
        {
            var addressSimilarity = CalculateAddressSimilarity(addressA, addressB);
            if (addressSimilarity >= 0.8)
            {
                var addressPoints = (decimal)(addressSimilarity * 15.0);
                score += addressPoints;
                reasons.Add($"Similar address ({addressSimilarity:P0})");
                explanationParts.Add($"Address overlap detected");
            }
        }

        // Cap score at 100
        if (score > 100.0m)
            score = 100.0m;

        var explanation = reasons.Any()
            ? string.Join(". ", explanationParts) + "."
            : "No significant similarities found.";

        return (score, reasons, explanation);
    }

    /// <summary>
    /// Calculates string similarity using normalized Levenshtein distance.
    /// Returns value between 0.0 (completely different) and 1.0 (identical).
    /// </summary>
    private double CalculateNameSimilarity(string name1, string name2)
    {
        if (string.IsNullOrWhiteSpace(name1) || string.IsNullOrWhiteSpace(name2))
            return 0.0;

        // Normalize: lowercase, remove extra whitespace, remove common suffixes
        var norm1 = NormalizeName(name1);
        var norm2 = NormalizeName(name2);

        if (norm1 == norm2)
            return 1.0;

        var distance = LevenshteinDistance(norm1, norm2);
        var maxLength = Math.Max(norm1.Length, norm2.Length);

        if (maxLength == 0)
            return 0.0;

        return 1.0 - ((double)distance / maxLength);
    }

    private string NormalizeName(string name)
    {
        // Remove common business suffixes
        var normalized = name.ToLowerInvariant().Trim();
        var suffixes = new[] { " llc", " inc", " corp", " corporation", " ltd", " limited", " co", " company" };

        foreach (var suffix in suffixes)
        {
            if (normalized.EndsWith(suffix))
                normalized = normalized.Substring(0, normalized.Length - suffix.Length).Trim();
        }

        // Remove punctuation and extra spaces
        var cleaned = new StringBuilder();
        foreach (var c in normalized)
        {
            if (char.IsLetterOrDigit(c) || c == ' ')
                cleaned.Append(c);
        }

        return System.Text.RegularExpressions.Regex.Replace(cleaned.ToString(), @"\s+", " ").Trim();
    }

    private int LevenshteinDistance(string s1, string s2)
    {
        var len1 = s1.Length;
        var len2 = s2.Length;
        var matrix = new int[len1 + 1, len2 + 1];

        for (int i = 0; i <= len1; i++)
            matrix[i, 0] = i;
        for (int j = 0; j <= len2; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= len1; i++)
        {
            for (int j = 1; j <= len2; j++)
            {
                var cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[len1, len2];
    }

    private double CalculateAddressSimilarity(SponsorAddress addr1, SponsorAddress addr2)
    {
        double score = 0.0;
        int checks = 0;

        // Compare street address (most important)
        if (!string.IsNullOrWhiteSpace(addr1.AddressLine1) &&
            !string.IsNullOrWhiteSpace(addr2.AddressLine1))
        {
            checks++;
            var streetSim = CalculateNameSimilarity(addr1.AddressLine1, addr2.AddressLine1);
            score += streetSim * 0.5; // Weight 50%
        }

        // Compare city
        if (!string.IsNullOrWhiteSpace(addr1.City) && !string.IsNullOrWhiteSpace(addr2.City))
        {
            checks++;
            if (addr1.City.Equals(addr2.City, StringComparison.OrdinalIgnoreCase))
                score += 0.25; // Weight 25%
        }

        // Compare state
        if (!string.IsNullOrWhiteSpace(addr1.StateProvince) &&
            !string.IsNullOrWhiteSpace(addr2.StateProvince))
        {
            checks++;
            if (addr1.StateProvince.Equals(addr2.StateProvince, StringComparison.OrdinalIgnoreCase))
                score += 0.15; // Weight 15%
        }

        // Compare postal code
        if (!string.IsNullOrWhiteSpace(addr1.PostalCode) &&
            !string.IsNullOrWhiteSpace(addr2.PostalCode))
        {
            checks++;
            var zip1 = addr1.PostalCode.Split('-')[0].Trim();
            var zip2 = addr2.PostalCode.Split('-')[0].Trim();
            if (zip1 == zip2)
                score += 0.10; // Weight 10%
        }

        return checks > 0 ? score : 0.0;
    }

    /// <summary>
    /// Gets all pending duplicate candidates for review.
    /// </summary>
    public async Task<List<SponsorDuplicateCandidate>> GetPendingCandidatesAsync()
    {
        return await _context.SponsorDuplicateCandidates
            .Include(c => c.PrimarySponsor)
            .Include(c => c.DuplicateSponsor)
            .Where(c => c.Status == "Pending")
            .OrderByDescending(c => c.MatchScore)
            .ToListAsync();
    }

    /// <summary>
    /// Marks a candidate as reviewed and not a duplicate.
    /// </summary>
    public async Task MarkAsNotDuplicateAsync(int candidateId, string reviewedByUserId, string notes)
    {
        var candidate = await _context.SponsorDuplicateCandidates.FindAsync(candidateId);
        if (candidate != null)
        {
            candidate.Status = "ReviewedNotDuplicate";
            candidate.ReviewedOn = DateTime.UtcNow;
            candidate.ReviewedByUserId = reviewedByUserId;
            candidate.ReviewNotes = notes;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Marks a candidate as scheduled for merge.
    /// </summary>
    public async Task ScheduleMergeAsync(int candidateId, string reviewedByUserId)
    {
        var candidate = await _context.SponsorDuplicateCandidates.FindAsync(candidateId);
        if (candidate != null)
        {
            candidate.Status = "MergeScheduled";
            candidate.ReviewedOn = DateTime.UtcNow;
            candidate.ReviewedByUserId = reviewedByUserId;
            await _context.SaveChangesAsync();
        }
    }
}
