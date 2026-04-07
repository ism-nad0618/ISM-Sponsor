using ISMSponsor.Data;
using ISMSponsor.Models.Domain;
using ISMSponsor.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Services;

public class FeedbackService
{
    private readonly AppDbContext _context;

    public FeedbackService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FeedbackViewModel> GetFeedbackSummaryAsync()
    {
        var feedbackItems = await _context.Set<UserFeedback>()
            .OrderByDescending(f => f.SubmittedAt)
            .Take(50)
            .ToListAsync();

        var model = new FeedbackViewModel
        {
            RecentFeedback = feedbackItems.Select(f => new FeedbackItem
            {
                FeedbackId = f.FeedbackId,
                Category = f.Category,
                Severity = f.Severity,
                Module = f.Module,
                Title = f.Title,
                Description = f.Description,
                AffectedResource = f.AffectedResource,
                SubmittedBy = f.SubmittedBy,
                SubmittedAt = f.SubmittedAt,
                Status = f.Status,
                Resolution = f.Resolution
            }).ToList(),

            FeedbackByCategory = feedbackItems
                .GroupBy(f => f.Category)
                .ToDictionary(g => g.Key, g => g.Count()),

            FeedbackBySeverity = feedbackItems
                .GroupBy(f => f.Severity)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return model;
    }

    public async Task<int> SubmitFeedbackAsync(SubmitFeedbackViewModel model, string submittedBy)
    {
        var feedback = new UserFeedback
        {
            Category = model.Category,
            Severity = model.Severity,
            Module = model.Module,
            Title = model.Title,
            Description = model.Description,
            AffectedResource = model.AffectedResource,
            SubmittedBy = submittedBy,
            SubmittedAt = DateTime.UtcNow,
            Status = "Open"
        };

        _context.Set<UserFeedback>().Add(feedback);
        await _context.SaveChangesAsync();

        return feedback.FeedbackId;
    }

    public async Task UpdateFeedbackStatusAsync(int feedbackId, string status, string? resolution)
    {
        var feedback = await _context.Set<UserFeedback>().FindAsync(feedbackId);
        if (feedback != null)
        {
            feedback.Status = status;
            feedback.Resolution = resolution;
            await _context.SaveChangesAsync();
        }
    }
}
