using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using ShoppingPlate.Services;

namespace ShoppingPlate.Controllers
{
    public class TestEmailController : Controller
    {
        private readonly IEmailService _emailService;

        public TestEmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string toEmail, string subject, string message)
        {
            try
            {
                await _emailService.SendTestEmailAsync(toEmail, subject, message);
                ViewBag.Result = "✅ 測試信已送出，請查看信箱";
            }
            catch (Exception ex)
            {
                ViewBag.Result = $"❌ 寄信失敗：{ex.Message}";
            }

            return View();
        }

        // 測試用：可直接訪問 localhost:/TestEmail/TestSmtpDirect
        [HttpGet]
        public async Task<IActionResult> TestSmtpDirect()
        {
            var appPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

            Console.WriteLine($"[DEBUG] App Password Length: {appPassword?.Length}");

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential("teamoneproject004@gmail.com", appPassword)
            };

            var mail = new MailMessage(
                "teamoneproject004@gmail.com",
                "boyprincemagician@gmail.com",  // ← 改成你自己 Gmail 或常用信箱
                "📧 SMTP 直接測試信件",
                "✅ 這是從 TestSmtpDirectAsync() 測試寄出的信件"
            );
            Console.WriteLine($"SMTP_USERNAME={Environment.GetEnvironmentVariable("SMTP_USERNAME")}");
            Console.WriteLine($"SMTP_PASSWORD={Environment.GetEnvironmentVariable("SMTP_PASSWORD")}");


            try
            {
                await client.SendMailAsync(mail);
                return Content("✅ 測試 SMTP 寄信成功！");
            }
            catch (Exception ex)
            {
                return Content($"❌ 測試 SMTP 寄信失敗：{ex.Message}");
            }
        }
    }
}
