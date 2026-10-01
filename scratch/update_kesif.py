import io

filepath = r'GMK360.Web\Views\B2BPartnerPortal\Invite.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Add InfoMessage display
info_alert = """        @if (TempData["InfoMessage"] != null)
        {
            <div class="alert alert-info border-0 shadow-sm rounded-3 mb-4 d-flex align-items-center">
                <i class="bi bi-info-circle-fill fs-3 me-3"></i>
                <div>
                    <h6 class="fw-bold mb-0">@TempData["InfoMessage"]</h6>
                </div>
            </div>
        }"""
content = content.replace('<!-- Güvenlik & Hoşgeldin -->', info_alert + '\n\n        <!-- Güvenlik & Hoşgeldin -->')

# Add Kesif Talebi Button and Logic
buttons = """                                    <div class="input-group input-group-lg mb-3">
                                        <span class="input-group-text bg-light fw-bold text-muted">₺</span>
                                        <input type="number" step="0.01" name="offeredPrice" class="form-control fw-bold text-dark" placeholder="Götürü veya Birim fiyatınızı giriniz..." required>
                                        <button class="btn btn-primary px-4 fw-bold" type="submit"><i class="bi bi-send-fill me-2"></i> Teklifi İlet</button>
                                    </div>
                                    <div class="d-flex justify-content-between align-items-center">
                                        <div class="form-text"><i class="bi bi-clock-history"></i> Fiyatınız şifreli olarak satınalma departmanına iletilecektir.</div>
                                        <button type="button" class="btn btn-outline-secondary btn-sm" onclick="document.getElementById('inspectionForm').submit();">
                                            <i class="bi bi-calendar-event"></i> Sahayı Görmek İstiyorum (Keşif Talebi)
                                        </button>
                                    </div>
                                </form>
                                <form id="inspectionForm" method="post" action="/B2BPartnerPortal/RequestInspection" style="display:none;">
                                    @Html.AntiForgeryToken()
                                    <input type="hidden" name="inviteId" value="@Model.Id" />
                                </form>"""
                                
content = content.replace("""                                    <div class="input-group input-group-lg mb-3">
                                        <span class="input-group-text bg-light fw-bold text-muted">₺</span>
                                        <input type="number" step="0.01" name="offeredPrice" class="form-control fw-bold text-dark" placeholder="Götürü veya Birim fiyatınızı giriniz..." required>
                                        <button class="btn btn-primary px-4 fw-bold" type="submit"><i class="bi bi-send-fill me-2"></i> Teklifi İlet</button>
                                    </div>
                                    <div class="form-text"><i class="bi bi-clock-history"></i> Fiyatınız şifreli olarak satınalma departmanına iletilecektir.</div>
                                </form>""", buttons)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated Controller and View for Kesif Talebi")
