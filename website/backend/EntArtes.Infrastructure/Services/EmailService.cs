using EntArtes.Core.Interfaces;

namespace EntArtes.Infrastructure.Services;

public class EmailService : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string body)
    {
        Console.WriteLine($"[EMAIL] To: {to}, Subject: {subject}");
        return Task.CompletedTask;
    }
}
