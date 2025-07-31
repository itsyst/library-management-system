public interface IEmailService
{
     Task<bool> EmailVerifier(string email);
     Task SendEmailAsync(string toEmail, string subject, string body);
}
 
