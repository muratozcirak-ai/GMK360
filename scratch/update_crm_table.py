import re

filepath = r'GMK360.Web\Views\Admin\B2bList.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the table headers and rows to make it a CRM dashboard
new_table = """<thead class="bg-light">
                        <tr>
                            <th class="px-4 py-3 border-0 rounded-top-start">@(listType == "usta" ? "Usta Adı Soyadı" : "Firma Ünvanı")</th>
                            <th class="px-4 py-3 border-0">Kategoriler / Alanlar</th>
                            <th class="px-4 py-3 border-0">Kayıt Tarihi</th>
                            <th class="px-4 py-3 border-0">Hesap Durumu</th>
                            <th class="px-4 py-3 border-0">Kayıt Eden (Referans)</th>
                            <th class="px-4 py-3 border-0 text-end rounded-top-end">İşlemler (CRM)</th>
                        </tr>
                    </thead>
                    <tbody>
                        @if(!Model.Any()) {
                            <tr><td colspan="6" class="text-center py-5 text-muted">Bu kategoride henüz kayıt bulunamadı.</td></tr>
                        }
                        @foreach(var item in Model)
                        {
                            <tr>
                                <td class="px-4 py-3">
                                    <div class="fw-bold text-dark mb-1">@item.Name</div>
                                    <div class="small text-muted"><i class="ph-fill ph-identification-card me-1"></i>@(item.LegalStatus == GMK360.Core.Entities.B2b.LegalEntityType.Individual ? "Bireysel (Şahıs)" : "Tüzel Kişi (Şirket)")</div>
                                </td>
                                <td class="px-4 py-3">
                                    <div class="d-flex flex-wrap gap-1">
                                        @foreach(var cat in item.CompanyCategories)
                                        {
                                            <span class="badge bg-warning bg-opacity-10 text-dark border border-warning border-opacity-25 px-2 py-1">@cat.Category.Name</span>
                                        }
                                    </div>
                                </td>
                                <td class="px-4 py-3">
                                    <div class="fw-bold text-dark">@item.CreatedAt.ToString("dd.MM.yyyy")</div>
                                    <div class="small text-muted">@item.CreatedAt.ToString("HH:mm")</div>
                                </td>
                                <td class="px-4 py-3">
                                    @if(item.InvitationStatus == GMK360.Core.Entities.B2b.B2bInvitationStatus.Shadow)
                                    {
                                        <span class="badge bg-dark bg-opacity-10 text-dark border border-dark border-opacity-25"><i class="bi bi-moon-stars-fill text-dark me-1"></i> Gölge Kullanıcı</span>
                                    }
                                    else if(item.InvitationStatus == GMK360.Core.Entities.B2b.B2bInvitationStatus.Invited)
                                    {
                                        <span class="badge bg-info bg-opacity-10 text-info border border-info border-opacity-25"><i class="bi bi-envelope-paper-fill me-1"></i> Davet Edildi</span>
                                    }
                                    else if(item.InvitationStatus == GMK360.Core.Entities.B2b.B2bInvitationStatus.Active)
                                    {
                                        <span class="badge bg-success bg-opacity-10 text-success border border-success border-opacity-25"><i class="bi bi-check-circle-fill me-1"></i> Aktif Üye</span>
                                    }
                                    else if(item.InvitationStatus == GMK360.Core.Entities.B2b.B2bInvitationStatus.Pro)
                                    {
                                        <span class="badge bg-warning bg-opacity-10 text-warning border border-warning border-opacity-50"><i class="bi bi-star-fill text-warning me-1"></i> Pro Üye</span>
                                    }
                                </td>
                                <td class="px-4 py-3">
                                    @if(item.AddedByUserId != null)
                                    {
                                        <span class="badge bg-secondary bg-opacity-10 text-secondary border border-secondary border-opacity-25 px-2 py-1">
                                            <i class="bi bi-person-badge-fill me-1"></i> Ana Kullanıcı Eklemesi
                                        </span>
                                    }
                                    else if(item.AddedByAgency != null)
                                    {
                                        <span class="badge bg-secondary bg-opacity-10 text-secondary border border-secondary border-opacity-25 px-2 py-1">
                                            <i class="ph-fill ph-handshake me-1"></i> @item.AddedByAgency.CompanyName
                                        </span>
                                    }
                                    else
                                    {
                                        <span class="badge bg-light text-muted border px-2 py-1">Sistem Yöneticisi</span>
                                    }
                                </td>
                                <td class="px-4 py-3 text-end">
                                    @if(item.InvitationStatus == GMK360.Core.Entities.B2b.B2bInvitationStatus.Shadow)
                                    {
                                        <button class="btn btn-sm btn-primary fw-bold shadow-sm" onclick="alert('SMS/Mail Daveti Gönderilecek')">
                                            <i class="bi bi-send-fill me-1"></i> Davet Gönder
                                        </button>
                                    }
                                    else
                                    {
                                        <button class="btn btn-sm btn-light border text-muted">
                                            <i class="bi bi-three-dots"></i>
                                        </button>
                                    }
                                </td>
                            </tr>
                        }
                    </tbody>"""

# Find the indices to replace
start_idx = content.find('<thead class="bg-light">')
end_idx = content.find('</tbody>', start_idx) + len('</tbody>')

if start_idx != -1 and end_idx != -1:
    content = content[:start_idx] + new_table + content[end_idx:]
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print("CRM Table Successfully Updated.")
else:
    print("Could not find the table to replace.")
