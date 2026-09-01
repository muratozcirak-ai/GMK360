using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using GMK360.Core.Entities.Identity;

namespace GMK360.Web.Filters
{
    public class OnboardingRequirementFilter : IAsyncActionFilter
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public OnboardingRequirementFilter(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;

            // Eğer kullanıcı giriş yapmamışsa filtreyi geç.
            if (user == null || !user.Identity.IsAuthenticated)
            {
                await next();
                return;
            }

            var controllerName = context.RouteData.Values["controller"]?.ToString();
            var actionName = context.RouteData.Values["action"]?.ToString();

            // Account controller (Login, Logout, Onboarding vs) ve herkese açık Home sayfalarında filtreyi devre dışı bırakıyoruz.
            if (controllerName == "Account" || controllerName == "Home" || controllerName == "Blog")
            {
                await next();
                return;
            }

            var appUser = await _userManager.GetUserAsync(user);
            if (appUser != null)
            {
                // Dijital Evim (Bireysel) kullanıcıları için bu filtreyi devreden çıkarıyoruz.
                // Kullanıcının doğrudan "SetupWizard" ekranına gidebilmesi ve karmaşık "Modül Seçimi" 
                // ekranına takılmaması için Bireysel kullanıcılarda telefon/e-devlet zorlamasını atlıyoruz.
                if (appUser.UserType == UserType.Individual)
                {
                    await next();
                    return;
                }

                // Kullanıcının Telefonu veya NVI onayı yoksa Onboarding'e zorla
                if (!appUser.PhoneNumberConfirmed || !appUser.IsEDevletVerified)
                {
                    if (!await _userManager.IsInRoleAsync(appUser, "Admin") && !await _userManager.IsInRoleAsync(appUser, "SuperAdmin"))
                    {
                        context.Result = new RedirectToActionResult("Onboarding", "Account", null);
                        return;
                    }
                }
            }

            await next();
        }
    }
}
