﻿// Controllers/CartController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingPlate.Models;

public class CartController : Controller
{
    private readonly ShoppingPlate.Data.ApplicationDbContext _context;

    public CartController(ShoppingPlate.Data.ApplicationDbContext context)
    {
        _context = context;
    }

    //購物車商品數量更新
    [HttpGet]
    public IActionResult GetCartItemCount()
    {
        var sessionId = GetSessionId();

        var count = _context.CartItems
            .Where(c => c.SessionId == sessionId)
            .Count();

        return Json(new { count });
    }



    // 顯示購物車
    public async Task<IActionResult> Index()
    {
        var cart = await GetCartItems();
        return View(cart);
    }

    
    //AJAX cart數量更新
    [HttpPost]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        var sessionId = GetSessionId();

        var item = await _context.CartItems
            .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.ProductId == productId);

        if (item != null)
        {
            item.Quantity += quantity;
        }
        else
        {
            item = new CartItem
            {
                SessionId = sessionId,
                ProductId = productId,
                Quantity = quantity
            };
            _context.CartItems.Add(item);
        }

        await _context.SaveChangesAsync();
        return Ok(); // 給前端 AJAX 用
    }


    private string GetSessionId()
    {
        if (!HttpContext.Session.TryGetValue("SessionId", out var value))
        {
            value = Guid.NewGuid().ToByteArray();
            HttpContext.Session.Set("SessionId", value);
        }
        return BitConverter.ToString(value).Replace("-", "");
    }

    private async Task<List<CartItem>> GetCartItems()
    {
        var sessionId = GetSessionId();
        return await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.SessionId == sessionId)
            .ToListAsync();
    }
    //更新數量
    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
    {
        var item = await _context.CartItems.FindAsync(cartItemId);

        if (item != null && quantity > 0)
        {
            item.Quantity = quantity;
            await _context.SaveChangesAsync();
            
        }

        return RedirectToAction("Index");
    }
    //移除項目
    [HttpPost]
    public async Task<IActionResult> RemoveFromCart(int cartItemId)
    {
        var item = await _context.CartItems.FindAsync(cartItemId);

        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        return Ok();
    }
    [HttpPost]
    public async Task<IActionResult> ConfirmBeforeCheckout(List<CartItem> cartItems)
    {
        foreach (var item in cartItems)
        {
            var existing = await _context.CartItems.FindAsync(item.Id);
            if (existing != null && item.Quantity > 0)
            {
                existing.Quantity = item.Quantity;
            }
        }
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Checkout"); // ✅ 結帳頁刷新，紅點也會更新
    }



}
