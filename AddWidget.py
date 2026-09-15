import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Dashboard\Construction.cshtml'

with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

widget_code = '''
        <div class="col-12 col-xl-4 mb-4">
            <div class="card shadow-sm border-0 rounded-4 h-100 border-top border-danger border-4">
                <div class="card-header bg-white border-bottom-0 pt-4 px-4 d-flex justify-content-between align-items-center">
                    <h5 class="fw-bold mb-0 text-danger"><i class="bi bi-alarm me-2"></i> Yaklaşan Ödemeler</h5>
                    <a href="/SupplierCurrentAccount/Index" class="btn btn-sm btn-outline-danger rounded-pill">Finansa Git</a>
                </div>
                <div class="card-body p-4">
                    @if (ViewBag.UpcomingPayments != null && ((IEnumerable<GMK360.Core.Entities.Finance.SupplierPayment>)ViewBag.UpcomingPayments).Any())
                    {
                        <ul class="list-group list-group-flush">
                            @foreach (var payment in (IEnumerable<GMK360.Core.Entities.Finance.SupplierPayment>)ViewBag.UpcomingPayments)
                            {
                                var daysLeft = (payment.DueDate.Value.Date - DateTime.UtcNow.Date).Days;
                                var badgeClass = daysLeft <= 3 ? "bg-danger" : "bg-warning text-dark";
                                
                                <li class="list-group-item px-0 py-3 d-flex justify-content-between align-items-center">
                                    <div>
                                        <div class="fw-bold">@payment.SupplierCurrentAccount.PhonebookContact.Name</div>
                                        <div class="small text-muted">@payment.Method.ToString() - @payment.CheckNumber</div>
                                    </div>
                                    <div class="text-end">
                                        <div class="fw-bold text-danger">@payment.Amount.ToString("N2") ₺</div>
                                        <span class="badge @badgeClass rounded-pill">@(daysLeft < 0 ? Math.Abs(daysLeft) + " gün gecikti" : daysLeft + " gün kaldı")</span>
                                    </div>
                                </li>
                            }
                        </ul>
                    }
                    else
                    {
                        <div class="text-center py-4 text-muted">
                            <i class="bi bi-check-circle fs-1 text-success mb-2 d-block"></i>
                            <p class="mb-0">Yakın vadeli ödeme bulunmuyor.</p>
                        </div>
                    }
                </div>
            </div>
        </div>
'''

# Find the end of the first row and add a new col-12 col-xl-4
# Let's just insert it before the closing of the row div.
pattern = r'(</div>\s*</div>\s*</div>\s*</div>\s*</div>)'

def insert_widget(match):
    return '''            </div>
                </div>
            </div>
        </div>''' + widget_code + '''\n    </div>\n</div>'''

content = re.sub(r'<!-- END ROW -->', widget_code + '<!-- END ROW -->', content) # Trying to find a good anchor point

# If no END ROW, let's just append before last </div></div>
if 'Yaklaşan Ödemeler' not in content:
    content = content.replace('    </div>\r\n</div>\r\n\r\n@section Scripts', widget_code + '\r\n    </div>\r\n</div>\r\n\r\n@section Scripts')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
