using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


public class HomeController : Controller
{
    private readonly ShoppingPlate.Data.ApplicationDbContext _context;

    public HomeController(ShoppingPlate.Data.ApplicationDbContext context)
    {
        _context = context;
    }
public async Task<IActionResult> Index(string? category)
{
    var products = _context.Products
        .Include(p => p.Category)
        .Include(p => p.Images)
        .Where(p => p.IsPublished)
        .AsQueryable();

    var categoryList = await _context.Categories.ToListAsync();
    ViewBag.Categories = categoryList;

    if (!string.IsNullOrEmpty(category))
    {
        if (categoryList.Any(c => c.Name == category))
        {
            products = products.Where(p => p.Category.Name == category);
        }
    }

    // 熱銷商品（用 ViewCount 熱門排序，最多五筆）
    var hotProducts = await _context.Products
        .Where(p => p.IsPublished)
        .OrderByDescending(p => p.ViewCount)
        .Take(5)
        .Include(p => p.Images)
        .ToListAsync();

    // 如果熱門商品都沒瀏覽（都為 0）或數量少於5，顯示最新商品
    if (hotProducts.Count == 0 || hotProducts.All(p => p.ViewCount == 0))
    {
        hotProducts = await _context.Products
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.Id) // Id 越大越新
            .Take(5)
            .Include(p => p.Images)
            .ToListAsync();
    }

    ViewBag.HotProducts = hotProducts;

    return View(await products.ToListAsync());
}


    public IActionResult Privacy()
    {
        return View();
    }


}
