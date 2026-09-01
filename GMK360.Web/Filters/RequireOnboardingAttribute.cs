using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using GMK360.Core.Entities.Identity;
using System.Threading.Tasks;

namespace GMK360.Web.Filters
{
    public class RequireOnboardingAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;
            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                var userManager = context.HttpContext.RequestServices.GetService(typeof(UserManager<ApplicationUser>)) as UserManager<ApplicationUser>;
                if (userManager != null)
                {
                    var appUser = await userManager.GetUserAsync(user);
                                        if (appUser != null && (!appUser.IsEDevletVerified || !appUser.PhoneNumberConfirmed))
                    {
                        // Eski/Mevcut kullanÄ±cÄ± (Legacy) kontrolÃ¼: EÄŸer kullanÄ±cÄ±nÄ±n sistemde mÃ¼lkÃ¼ veya yÃ¶nettiÄŸi bina varsa Onboarding'i es geÃ§.
                        var dbContext = context.HttpContext.RequestServices.GetService(typeof(GMK360.Data.Contexts.ApplicationDbContext)) as GMK360.Data.Contexts.ApplicationDbContext;
                        bool isLegacyUser = false;
                        if (dbContext != null)
                        {
                            isLegacyUser = dbContext.Properties.Any(p => p.UserId == appUser.Id) || 
                                           dbContext.BuildingManagers.Any(b => b.UserId == appUser.Id);
                        }

                        if (isLegacyUser)
                        {
                            // Eski kullanÄ±cÄ±, onu Onboarding'e zorlamÄ±yoruz, normal akÄ±ÅŸÄ±na devam ediyor.
                            await next();
                            return;
                        }

                        // EÄŸer kiÅŸi zaten Onboarding sayfasÄ±ndaysa loop'a girmemek iÃ§in kontrol edelim
                        var controller = context.RouteData.Values["controller"]?.ToString();
                        var action = context.RouteData.Values["action"]?.ToString();
                        
                        if (!(controller == "Account" && (action == "Onboarding" || action == "Logout")))
                        {
                            context.Result = new RedirectToActionResult("Onboarding", "Account", null);
                            return;
                        }
                    }
                }
            }
            await next();
        }
    }
}

