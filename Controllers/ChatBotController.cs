using Microsoft.AspNetCore.Mvc;
using ShoppingPlate.Services;

public class ChatBotController : Controller
{
    private readonly GeminiService _geminiService;

    public ChatBotController(GeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    // ✅ Gemini 小助手浮動視窗 - AJAX 呼叫端點
    [HttpPost]
    [Route("api/chatbot/ask")]
    public async Task<IActionResult> Ask([FromBody] ChatRequest req)
    {
        var message = req.Message.Trim();

        // 常見問題優先回應
        var defaultReply = GetDefaultReply(message);
        if (!string.IsNullOrEmpty(defaultReply))
        {
            return Json(new { reply = defaultReply });
        }

        // 呼叫 Gemini API
        string role = HttpContext.Session.GetInt32("LoginRole")?.ToString() ?? "訪客";
        string userName = HttpContext.Session.GetString("UserName") ?? "未登入";
        string path = HttpContext.Request.Path;
        string context = $"你是 豬手遮天購物網站的 AI 小助手。 網站使用 ASP.NET Core MVC 架構，具有購物車、商品瀏覽、訂單管理、退貨、金流功能。目前使用者為：{role}（{userName}），正在瀏覽：{path} 頁面。請根據這些資訊以中文回答問題。";

        var geminiResponse = await _geminiService.AskGemini(message);
        return Json(new { reply = geminiResponse });
    }

    // ✅ 常見問題回覆邏輯
    private string? GetDefaultReply(string message)
    {
        switch (message)
        {
            case "如何查詢訂單？":
                return "您可以點選 <a href='/Checkout/Query' class='chatbot-link'>訂單查詢頁面</a>，輸入訂單編號或登入帳號查看。";

            case "如何申請退貨？":
                return "請前往 <a href='/Checkout/Query' class='chatbot-link'>訂單查詢</a> 找到欲退貨項目並點選『申請退貨』，依指示操作即可。";

            case "我要修改密碼":
                return "請先登入後前往 <a href='/Account/Edit' class='chatbot-link'>帳號設定頁</a>，即可修改密碼。";

            default:
                return null;
        }
    }

    // ✅ 手動測試介面（可保留）
    [HttpGet]
    public IActionResult TestGemini()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> TestGemini(string userInput)
    {
        ViewData["UserInput"] = userInput;
        var response = await _geminiService.AskGemini(userInput);
        ViewBag.Response = response;
        return View();
    }

    // ✅ AJAX JSON API for chatbot 彈出視窗
    [HttpPost]
    public async Task<IActionResult> AskGeminiJson([FromBody] string userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput))
            return BadRequest("輸入不得為空");

        var response = await _geminiService.AskGemini(userInput);
        return Json(new { reply = response });
    }

    // ✅ 請求模型
    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
