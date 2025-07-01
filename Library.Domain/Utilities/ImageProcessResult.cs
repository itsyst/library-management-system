namespace Library.Domain.Utilities;

public class ImageProcessResult
{
    public bool Success { get; set; }
    public string? ImageBase64 { get; set; }
    public string? ErrorMessage { get; set; }
}
