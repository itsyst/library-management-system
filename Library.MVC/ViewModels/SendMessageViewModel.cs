using System.ComponentModel.DataAnnotations;

namespace Library.MVC.ViewModels;

public class SendMessageViewModel
{
    public required string RecipientId { get; set; }
    public required string RecipientName { get; set; }

    [Required(ErrorMessage = "Mottagarens e-postadress krävs.")]
    [EmailAddress(ErrorMessage = "Skriv en giltig e-postadress.")]
    public required string RecipientEmail { get; set; }

    [Required(ErrorMessage = "Ämne krävs.")] 
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Meddelandetext krävs.")]
    public required string Body { get; set; }
}
