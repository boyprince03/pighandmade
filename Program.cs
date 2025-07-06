using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using ShoppingPlate.Data;
using ShoppingPlate.Services;
using DotNetEnv;
using Microsoft.Extensions.Options;
using Sprache;

var builder = WebApplication.CreateBuilder(args);

// 判斷是否為 Production 環境
var isProduction = builder.Environment.IsProduction();

// ✅ 資料庫設定（部署用 SQLite、開發用 SQL Server 或 SQLite 都可）
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (isProduction)
    {
        // 部署到 Ubuntu 用 SQLite
        options.UseSqlite("Data Source=shoppingplate.db");
    }
    else
    {
        // 開發模式建議用 SQLite（跨平台），或維持 SQL Server
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseSqlite("Data Source=shoppingplate.db"); // 推薦測試直接用 SQLite

        // 如果你堅持用 SQL Server，請改成下面這行
        // options.UseSqlServer(connectionString);
    }
});

// 載入 .env 檔案
DotNetEnv.Env.Load();

// 從環境變數取得設定
//var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
//var smtpPort = Environment.GetEnvironmentVariable("SMTP_PORT");
var smtpUser = Environment.GetEnvironmentVariable("SMTP_USERNAME");
var smtpPass = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
var smtpFrom = Environment.GetEnvironmentVariable("SMTP_FROM");
Console.WriteLine($"SMTP_USERNAME={Environment.GetEnvironmentVariable("SMTP_USERNAME")}");


builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.Secure = CookieSecurePolicy.Always;
});


// ✅ Session 設定
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();




builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddGoogle(options =>
{
    options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
    options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
    options.SaveTokens = true;
    options.Scope.Add("email");
    options.Scope.Add("profile");
    options.CallbackPath = "/signin-google";
});


//API
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<GoogleAuthProvider>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHttpClient<ChatGPTService>();
builder.Services.AddHttpClient<GeminiService>();



builder.Services.AddAuthorization();

// ✅ Railway 用 PORT 環境變數綁定 Port
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

//啟動APP，所有builder.Services 要在這之前
var app = builder.Build();

// ✅ 自動初始化資料庫（開發 SQLite 時也會執行）
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    DbInitializer.Initialize(db);
}

// ✅ 中介軟體設定
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCookiePolicy();    // ✅ 若你有設定 SameSiteMode.None，務必加入
app.UseSession();         // ✅ 必須先於 Authentication
app.UseRouting();
app.UseAuthentication();  // ✅ 認證
app.UseAuthorization();   // ✅ 授權

// ✅ 預設路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
