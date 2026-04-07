using ISMSponsor.Constants;
using ISMSponsor.Models.API;
using ISMSponsor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ISMSponsor.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin,admissions,cashier")]
    public class CoverageController : ControllerBase
    {
        private readonly CoverageEvaluationService _evaluationService;
        private readonly ILogger<CoverageController> _logger;

        public CoverageController(CoverageEvaluationService evaluationService, ILogger<CoverageController> logger)
        {
            _evaluationService = evaluationService;
            _logger = logger;
        }

        /// <summary>
        /// Evaluate coverage for a charge line
        /// </summary>
        /// <param name="request">Evaluation request containing student, item, amount, and date information</param>
        /// <returns>Coverage decision with bill-to allocation and reason code</returns>
        [HttpPost("evaluate")]
        [ProducesResponseType(typeof(CoverageEvaluationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CoverageEvaluationResponse>> Evaluate([FromBody] CoverageEvaluationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
            var userDisplay = User.Identity?.Name ?? "Anonymous";
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "unknown";

            try
            {
                var response = await _evaluationService.EvaluateAsync(request, userId, userDisplay, userRole);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing coverage evaluation request");
                return StatusCode(500, new
                {
                    error = "An error occurred processing your request",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get all supported reason codes with descriptions
        /// </summary>
        /// <returns>List of reason codes with descriptions and categories</returns>
        [HttpGet("reasons")]
        [ProducesResponseType(typeof(List<ReasonCodeInfo>), StatusCodes.Status200OK)]
        public ActionResult<List<ReasonCodeInfo>> GetReasonCodes()
        {
            var reasonCodes = CoverageReasonCodes.GetAllReasonCodes();
            return Ok(reasonCodes);
        }
    }
}
