import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ConstructionLayout.cshtml'

with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# Add injection
if 'UserManager<ApplicationUser>' not in content:
    content = content.replace('@using Microsoft.AspNetCore.Http', '@using Microsoft.AspNetCore.Identity\n@using GMK360.Core.Entities.Identity\n@inject UserManager<ApplicationUser> UserManager\n@using Microsoft.AspNetCore.Http')

# Add modal logic inside body
modal_html = '''
@{
    var appUser = await UserManager.GetUserAsync(User);
    bool needsMapConsent = appUser != null && !appUser.HasMapConsent;
}
@if(needsMapConsent)
{
    <div class="modal fade" id="mapConsentModal" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow-lg rounded-4">
                <div class="modal-body p-5 text-center">
                    <div class="mb-4">
                        <i class="bi bi-geo-alt-fill text-primary" style="font-size: 3rem;"></i>
                    </div>
                    <h4 class="fw-bold mb-3">Google Haritalar & Lojistik İzni</h4>
                    <p class="text-muted mb-4">
                        Şantiyelerimize lojistik erişimi kolaylaştırmak ve operasyonları hızlandırmak amacıyla, proje konumlarının ve firma bilgilerinizin Google Haritalar altyapısında işlenmesine / paylaşılmasına izin veriyor musunuz?
                    </p>
                    <div class="d-grid gap-2">
                        <button type="button" class="btn btn-primary rounded-pill py-2" onclick="acceptMapConsent()">
                            <i class="bi bi-check2-circle me-1"></i> Okudum, Onaylıyorum
                        </button>
                        <button type="button" class="btn btn-light rounded-pill text-muted" data-bs-dismiss="modal">
                            Daha Sonra
                        </button>
                    </div>
                    <div class="mt-3 small text-muted">
                        <i class="bi bi-shield-lock me-1"></i> Tüm verileriniz KVKK kapsamında korunmaktadır.
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script>
        document.addEventListener('DOMContentLoaded', function() {
            var consentModal = new bootstrap.Modal(document.getElementById('mapConsentModal'));
            consentModal.show();
        });

        function acceptMapConsent() {
            fetch('/Profile/AcceptMapConsent', {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name=""__RequestVerificationToken""]')?.value || ''
                }
            }).then(res => {
                var consentModal = bootstrap.Modal.getInstance(document.getElementById('mapConsentModal'));
                consentModal.hide();
                window.location.reload();
            });
        }
    </script>
}
'''

content = content.replace('<!-- Ana İçerik -->', modal_html + '\n        <!-- Ana İçerik -->')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
