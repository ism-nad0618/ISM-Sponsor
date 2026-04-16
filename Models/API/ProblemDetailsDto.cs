using System;
using System.Collections.Generic;

namespace ISMSponsor.Models.API
{
    /// <summary>
    /// Standard error/problem response DTO for REST API responses.
    /// Follows RFC 7807 (Problem Details for HTTP APIs) structure.
    /// Used for 4xx/5xx error responses from all API endpoints.
    /// </summary>
    public class ProblemDetailsDto
    {
        /// <summary>
        /// HTTP status code (400, 404, 409, 422, 500, etc.).
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Machine-readable error type (e.g., "INVALID_COVERAGE_REQUEST", "SPONSOR_NOT_FOUND").
        /// </summary>
        public string ErrorType { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable error summary.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed error explanation (optional).
        /// </summary>
        public string? Detail { get; set; }

        /// <summary>
        /// Unique correlation ID for tracing this error across logs.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// RFC 3986 URI identifying the error type.
        /// Optional; used for linking to documentation.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// RFC 3986 URI identifying the specific problem instance.
        /// Optional; used for custom problem pages.
        /// </summary>
        public string? Instance { get; set; }

        /// <summary>
        /// Timestamp when the error occurred (UTC).
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Validation error details (for 422 Unprocessable Entity responses).
        /// Key = field name, Value = list of error messages for that field.
        /// </summary>
        public Dictionary<string, List<string>>? Errors { get; set; }

        /// <summary>
        /// Request ID for correlating with application logs.
        /// </summary>
        public string? RequestId { get; set; }
    }
}
