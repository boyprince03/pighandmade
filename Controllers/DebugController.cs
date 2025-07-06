using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ShoppingPlate.Controllers
{
    public class DebugController : Controller
    {
        // ✅ 寫入 Session 測試值
        public IActionResult SetSession()
        {
            HttpContext.Session.SetString("DebugKey", "HELLO_SESSION");
            return Content("✅ Session 寫入成功: DebugKey = HELLO_SESSION");
        }

        // ✅ 讀取 Session 測試值
        public IActionResult ReadSession()
        {
            var value = HttpContext.Session.GetString("DebugKey") ?? "❌ Session 遺失！";
            return Content($"🔍 DebugKey = {value}");
        }

        // ✅ 顯示目前所有 cookie
        public IActionResult ShowCookies()
        {
            var cookies = HttpContext.Request.Cookies.Select(c => $"{c.Key} = {c.Value}");
            return Content(string.Join("\n", cookies));
        }

        // ✅ 顯示 OAuth 傳入資訊（追蹤 state）
        [Route("signin-google-debug")]
        public async Task<IActionResult> GoogleResponseDebug(string returnUrl = "/")
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return Content("❌ Google 認證失敗（state 驗證失敗？）\n" +
                    string.Join("\n", result.Failure?.Message ?? "Unknown Error"));
            }

            var output = new List<string>
            {
                "✅ Google 認證成功",
                "Claims:" + string.Join(" | ", result.Principal.Identities.First().Claims.Select(c => $"{c.Type} = {c.Value}"))
            };

            return Content(string.Join("\n", output));
        }
    }
}