using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.ViewComponents;

public class AuthBannerViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _db;

    public AuthBannerViewComponent(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IViewComponentResult> InvokeAsync(bool isSmall = false)
    {
        var banners = await _db.AuthScreenBanners
            .Where(b => b.IsActive)
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync();

        if (isSmall)
        {
            return View("Small", banners);
        }

        return View(banners);
    }
}
