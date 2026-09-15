import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_Layout.cshtml'
with codecs.open(filepath, 'r', 'utf-8', errors='ignore') as f:
    layout_content = f.read()

# Extract the user profile dropdown
start_marker = '@if (User.Identity.IsAuthenticated)'
end_marker = '</form>\r\n                            </li>\r\n                        </ul>\r\n                    </div>\r\n                }'

start_idx = layout_content.find(start_marker)
# Find the exact end
end_idx = layout_content.find(end_marker, start_idx) + len(end_marker)

if end_idx < len(end_marker): # fallback
    end_marker = '</form>\n                            </li>\n                        </ul>\n                    </div>\n                }'
    end_idx = layout_content.find(end_marker, start_idx) + len(end_marker)

user_profile_code = layout_content[start_idx:end_idx]

# If we couldn't extract it for some reason, use a fallback snippet
if start_idx == -1 or end_idx < len(end_marker):
    user_profile_code = '''@if (User.Identity.IsAuthenticated)
                {
                    <div class="dropdown">
                        <button class="btn btn-outline-navy px-4 rounded-pill dropdown-toggle d-flex align-items-center gap-2" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                            <i class="ph-fill ph-user-circle fs-5"></i>
                            <span class="d-none d-md-inline">Hesabım</span>
                        </button>
                        <ul class="dropdown-menu dropdown-menu-end shadow border-0 mt-2" style="border-radius: 12px; min-width: 200px;">
                            <li class="px-3 py-2 border-bottom mb-1">
                                <div class="fw-bold">@( (await UserManager.GetUserAsync(User))?.DisplayName ?? User.Identity.Name )</div>
                            </li>
                            <li>
                                <form class="form-inline" asp-area="Identity" asp-page="/Account/Logout" asp-route-returnUrl="@Url.Action("Index", "Home", new { area = "" })">
                                    <button type="submit" class="dropdown-item text-danger"><i class="ph ph-sign-out me-2"></i>Çıkış Yap</button>
                                </form>
                            </li>
                        </ul>
                    </div>
                }'''

project_layout_path = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ProjectLayout.cshtml'
with codecs.open(project_layout_path, 'r', 'utf-8') as f:
    proj_content = f.read()

proj_content = proj_content.replace('<partial name="_LoginPartial" />', user_profile_code)

with codecs.open(project_layout_path, 'w', 'utf-8') as f:
    f.write(proj_content)

