using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ShoppingPlate.Data;
using ShoppingPlate.Models;
using System.Security.Claims;

namespace ShoppingPlate.Services
{
    public class GoogleAuthProvider
    {
        private readonly ApplicationDbContext _context;

        public GoogleAuthProvider(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> HandleGoogleLogin(HttpContext httpContext)
        {
            var result = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded) return null;

            var claims = result.Principal?.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (email == null || googleId == null)
                return null;

            // 🔍 查詢是否已有帳號
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Provider == "Google" && u.ProviderKey == googleId);

            if (user == null)
            {
                user = new User
                {
                    Username = name ?? email,
                    Email = email,
                    Provider = "Google",
                    ProviderKey = googleId,
                    LoginRole = UserRole.Customer
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return user;
        }
    }
}
