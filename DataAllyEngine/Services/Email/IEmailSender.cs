namespace DataAllyEngine.Services.Email;

public interface IEmailSender
{
	Task SendMail(string sender, string to, string subject, string content, bool isHtml = false);
	Task SendMailToMultipleRecipients(string sender, List<string> to, string subject, string content, bool isHtml = false);
}