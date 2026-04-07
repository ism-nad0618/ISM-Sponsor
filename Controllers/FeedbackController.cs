using ISMSponsor.Services;
using ISMSponsor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISMSponsor.Controllers;

[Authorize]
public class FeedbackController : Controller
{
    private readonly FeedbackService _feedbackService;

    public FeedbackController(FeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpGet]
    public IActionResult Submit()
    {
        return View(new SubmitFeedbackViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitFeedbackViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.Identity?.Name ?? "Unknown";
        var feedbackId = await _feedbackService.SubmitFeedbackAsync(model, userId);

        TempData["SuccessMessage"] = $"Thank you for your feedback! Reference ID: {feedbackId}";
        return RedirectToAction(nameof(Submit));
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> List()
    {
        var model = await _feedbackService.GetFeedbackSummaryAsync();
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateStatus(int feedbackId, string status, string? resolution)
    {
        await _feedbackService.UpdateFeedbackStatusAsync(feedbackId, status, resolution);
        return RedirectToAction(nameof(List));
    }
}
