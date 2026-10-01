import io
import re

filepath = r'GMK360.Web\Views\AdminCRM\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

injection = """
                        @if (ViewBag.B2BShadows != null)
                        {
                            var shadows = ViewBag.B2BShadows as IEnumerable<GMK360.Core.Entities.B2B.B2BNetworkContact>;
                            if(shadows != null)
                            {
                                foreach (var contact in shadows)
                                {
                                    <tr class="hover-lift" style="background-color: #f8f9fa;">
                                        <td class="ps-4 text-secondary">
                                            <i class="ph ph-minus"></i>
                                        </td>
                                        <td>
                                            <h6 class="mb-0 fw-bold text-secondary">@contact.CompanyName</h6>
                                            <small class="text-muted">Kişi: @(string.IsNullOrEmpty(contact.ContactPerson) ? "-" : contact.ContactPerson)</small>
                                            <br><small class="text-info"><i class="bi bi-box-arrow-in-right"></i> Davetle Geldi</small>
                                        </td>
                                        <td>
                                            <div class="mb-1"><i class="bi bi-envelope me-2 text-muted"></i>@(string.IsNullOrEmpty(contact.Email) ? "-" : contact.Email)</div>
                                            <div><i class="bi bi-telephone me-2 text-muted"></i>@(contact.PhoneNumber ?? "-")</div>
                                        </td>
                                        <td>
                                            <span class="badge bg-light text-dark border d-inline-block">Tedarikçi</span>
                                            <span class="badge bg-secondary rounded-pill ms-1" style="font-size:0.7rem;">
                                                <i class="bi bi-cloud me-1"></i>Gölge Hesap
                                            </span>
                                            <div class="mt-1 small fw-bold text-muted">
                                                <i class="bi bi-person-heart"></i> Davet Eden: <span class="text-dark">@(contact.OwnerAgency?.CompanyName ?? "Bilinmiyor")</span>
                                            </div>
                                        </td>
                                        <td>
                                            <span class="badge bg-warning bg-opacity-10 text-warning border border-warning rounded-pill px-3">
                                                Dönüşüm Bekliyor
                                            </span>
                                        </td>
                                        <td>
                                            <span class="text-muted small"><i class="bi bi-dash-circle"></i> Doğrulanmadı</span>
                                        </td>
                                        <td class="text-end pe-4">
                                            <button class="btn btn-sm btn-outline-secondary rounded-pill disabled">Detay Gör</button>
                                        </td>
                                    </tr>
                                }
                            }
                        }
                    </tbody>
"""

content = content.replace("                    </tbody>", injection)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Injected shadows in AdminCRM View")
