using Library.MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

public class EmailController(IEmailService emailService, ILogger<EmailController> logger) : Controller
{
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<EmailController> _logger = logger;


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendToMember([FromForm] SendMessageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, error = "Ogiltiga fält: " + string.Join("; ", errors) });
        }

        if (string.IsNullOrWhiteSpace(model.RecipientEmail))
        {
            return BadRequest(new { success = false, error = "Mottagarens e-postadress saknas." });
        }

        // Validate disposable or fake email domains early
        var disposableDomains = new HashSet<string> { "mailinator.com", "yopmail.com", "tempmail.com" };
        var domain = model.RecipientEmail.Split('@').Last().ToLowerInvariant();

        if (disposableDomains.Contains(domain))
        {
            return Json(new { success = false, error = "E-postadresser från temporära domäner tillåts inte."});
        }

        // Verify if domain has MX records asynchronously
        var hasMx = await _emailService.EmailVerifier(model.RecipientEmail);
        if (!hasMx)
        {
            return Json(new { success = false, error = "E-postadressen är ogiltig (domänen kan inte ta emot e-post)." });
        }

        try
        {
            // Send the email
            await _emailService.SendEmailAsync(
                model.RecipientEmail,
                model.Subject,
                model.Body
            );

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid sändning av e-post!");
            // Ideally log the error here
            return StatusCode(500, new { success = false, error = "Ett fel uppstod vid sändning av e-post." });
        }
    }
}


  