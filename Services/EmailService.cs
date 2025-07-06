using System.Net;
using System.Net.Mail;
using System.Text;
using ShoppingPlate.Models;

namespace ShoppingPlate.Services;

public class EmailService : IEmailService
{
    private SmtpClient CreateSmtpClient()
    {
        var host = Environment.GetEnvironmentVariable("SMTP_HOST");
        var portStr = Environment.GetEnvironmentVariable("SMTP_PORT");
        var username = Environment.GetEnvironmentVariable("SMTP_USERNAME");
        var password = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

        int port = int.TryParse(portStr, out var parsedPort) ? parsedPort : 587;

        return new SmtpClient(host)
        {
            Port = port,
            EnableSsl = true,
            Credentials = new NetworkCredential(username, password)
        };
    }

    private string GetFromEmail()
    {
        var email = Environment.GetEnvironmentVariable("SMTP_FROM");
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("環境變數 SMTP_FROM 未設定");
        return email;
    }

    private MailMessage CreateMailMessage(string to, string subject, string body)
    {
        return new MailMessage
        {
            From = new MailAddress(GetFromEmail(), "ShoppingPlate 購物平台"),
            Subject = subject,
            Body = body,
            IsBodyHtml = false,
            To = { to }
        };
    }

    // 訂單確認通知
    public async Task SendOrderConfirmationAsync(Order order)
    {
        using var smtpClient = CreateSmtpClient();

        var subject = $"🧾 訂單確認 #{order.Id}";
        var body = new StringBuilder();
        body.AppendLine($"感謝您的訂購，{order.CustomerName}！");
        body.AppendLine($"訂單編號：{order.Id}");
        body.AppendLine($"下單時間：{order.OrderDate:yyyy-MM-dd HH:mm}");
        body.AppendLine($"總金額：${order.TotalAmount:N0}");
        body.AppendLine();
        body.AppendLine("📦 商品明細：");

        foreach (var item in order.OrderDetails)
        {
            body.AppendLine($"- {item.Product.Name} x {item.Quantity} @ ${item.UnitPrice:N0}");
        }

        var mail = CreateMailMessage(order.CustomerEmail, subject, body.ToString());

        try
        {
            await smtpClient.SendMailAsync(mail);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 無法寄送訂單確認信：{ex.Message}");
            throw;
        }
    }

    // 訂單取消通知
    public async Task SendOrderCancellationAsync(Order order, string cancelledBy)
    {
        using var smtpClient = CreateSmtpClient();

        var subject = $"❌ 訂單已取消通知 #{order.Id}";
        var body = new StringBuilder();
        string toEmail;

        if (cancelledBy == "Seller")
        {
            toEmail = order.CustomerEmail;
            body.AppendLine($"您好，{order.CustomerName}：");
            body.AppendLine($"很抱歉通知您，您的訂單（編號：{order.Id}）已由賣家於 {DateTime.Now:yyyy-MM-dd HH:mm} 取消。");
            body.AppendLine("若有任何疑問，請聯絡客服或賣家。");
        }
        else if (cancelledBy == "Customer")
        {
            toEmail = order.SellerEmail;
            body.AppendLine("您好，賣家：");
            body.AppendLine($"訂單（編號：{order.Id}）已由買家 {order.CustomerName} 於 {DateTime.Now:yyyy-MM-dd HH:mm} 取消。");
            body.AppendLine("請登入後台確認。");
        }
        else
        {
            throw new ArgumentException("cancelledBy 必須為 Seller 或 Customer");
        }

        var mail = CreateMailMessage(toEmail, subject, body.ToString());

        try
        {
            await smtpClient.SendMailAsync(mail);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"寄送取消通知失敗：{ex.Message}");
            throw;
        }
    }

    // 賣家申請審核結果通知
    public async Task SendSellerApplicationResultAsync(string toEmail, string userName, bool isApproved)
    {
        using var smtpClient = CreateSmtpClient();

        var subject = isApproved ? "✅ 賣家申請審核通過通知" : "❌ 賣家申請未通過通知";
        var body = new StringBuilder();

        body.AppendLine($"親愛的 {userName}，您好：");
        body.AppendLine();

        if (isApproved)
        {
            body.AppendLine("🎉 恭喜您，您的賣家申請已通過審核！");
            body.AppendLine("您現在可以登入賣家後台上架商品並管理訂單。");
        }
        else
        {
            body.AppendLine("很遺憾通知您，您的賣家申請未能通過審核。");
            body.AppendLine("若有任何疑問，請聯絡客服人員協助處理。");
        }

        body.AppendLine();
        body.AppendLine("感謝您使用 ShoppingPlate 購物平台！");

        var mail = CreateMailMessage(toEmail, subject, body.ToString());

        try
        {
            await smtpClient.SendMailAsync(mail);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"寄送賣家申請結果通知失敗：{ex.Message}");
            throw;
        }
    }
    public async Task SendTestEmailAsync(string toEmail, string subject, string body)
    {
        using var smtpClient = CreateSmtpClient();

        var mail = new MailMessage
        {
            From = new MailAddress(GetFromEmail(), "ShoppingPlate 測試寄信"),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        mail.To.Add(toEmail);

        try
        {
            await smtpClient.SendMailAsync(mail);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"寄送測試信失敗：{ex.Message}");
            throw;
        }
    }


}
