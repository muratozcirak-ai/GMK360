import io

filepath = r'GMK360.Web\Views\ProjectFinance\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

new_cards = """
    <!-- Orta Kısım: Genel Özet Tablosu -->
    <div class="card shadow-sm border-0 mb-4 rounded-4">
        <div class="card-header bg-white border-bottom p-3">
            <h5 class="mb-0 fw-bold text-dark"><i class="bi bi-table text-primary me-2"></i>Genel Özet</h5>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover table-bordered mb-0">
                    <thead class="table-light">
                        <tr>
                            <th class="fw-bold">Başlık (Proje Harcamaları)</th>
                            <th class="fw-bold text-end">Planlanan</th>
                            <th class="fw-bold text-end">Gerçekleşen</th>
                            <th class="fw-bold text-end">Fark</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var phase in (Dictionary<GMK360.Core.Entities.Construction.BudgetPhaseCategory, dynamic>)ViewBag.PhaseData)
                        {
                            string asamaAdi = "";
                            switch (phase.Key)
                            {
                                case GMK360.Core.Entities.Construction.BudgetPhaseCategory.ResmiEvraklarVeProsedurler: asamaAdi = "Resmi Evraklar ve Prosedürler"; break;
                                case GMK360.Core.Entities.Construction.BudgetPhaseCategory.YikimVeZeminHazirligi: asamaAdi = "Yıkım ve Zemin Hazırlığı"; break;
                                case GMK360.Core.Entities.Construction.BudgetPhaseCategory.TemelVeAltYapi: asamaAdi = "Temel ve Alt Yapı"; break;
                                case GMK360.Core.Entities.Construction.BudgetPhaseCategory.KabaInsaatKarkas: asamaAdi = "Kaba İnşaat (Karkas)"; break;
                                case GMK360.Core.Entities.Construction.BudgetPhaseCategory.CatiVeDisCephe: asamaAdi = "Çatı ve Dış Cephe"; break;
                                case GMK360.Core.Entities.Construction.BudgetPhaseCategory.InceIslerIcMekan: asamaAdi = "İnce İşler (İç Mekan)"; break;
                                case GMK360.Core.Entities.Construction.BudgetPhaseCategory.ElektrikVeZayifAkim: asamaAdi = "Elektrik ve Zayıf Akım"; break;
                                case GMK360.Core.Entities.Construction.BudgetPhaseCategory.MekanikTesisatVeMakine: asamaAdi = "Mekanik Tesisat ve Makine"; break;
                                case GMK360.Core.Entities.Construction.BudgetPhaseCategory.PeyzajVeTeslim: asamaAdi = "Peyzaj ve Teslim"; break;
                            }
                            
                            decimal diff = phase.Value.Diff;
                            string diffColorClass = diff > 0 ? "text-danger" : (diff < 0 ? "text-success" : "text-muted");

                            <tr>
                                <td class="fw-bold">@asamaAdi</td>
                                <td class="text-end">@(((decimal)phase.Value.Planned).ToString("N2")) ₺</td>
                                <td class="text-end">@(((decimal)phase.Value.Actual).ToString("N2")) ₺</td>
                                <td class="text-end fw-bold @diffColorClass">@((Math.Abs(diff)).ToString("N2")) ₺ @(diff > 0 ? "(Aşıldı)" : (diff < 0 ? "(Tasarruf)" : ""))</td>
                            </tr>
                        }
                    </tbody>
                    <tfoot class="table-light fw-bold">
                        <tr>
                            <td class="text-end text-uppercase">Genel Toplam:</td>
                            <td class="text-end fs-5 text-primary">@(((decimal)ViewBag.TotalPlanned).ToString("N2")) ₺</td>
                            <td class="text-end fs-5 text-dark">@(((decimal)ViewBag.TotalActual).ToString("N2")) ₺</td>
                            <td class="text-end fs-5 @((decimal)ViewBag.TotalDiff > 0 ? "text-danger" : "text-success")">@((Math.Abs((decimal)ViewBag.TotalDiff)).ToString("N2")) ₺</td>
                        </tr>
                    </tfoot>
                </table>
            </div>
        </div>
    </div>

    <!-- Alt Kısım: Detaylı Akordeon Yapısı -->
    <h5 class="fw-bold mb-3 mt-5"><i class="bi bi-list-nested text-warning me-2"></i>Aşama Detayları</h5>
    
    <div class="accordion mb-5 shadow-sm" id="financeAccordion">
        @{
            int index = 0;
            foreach (var phase in (Dictionary<GMK360.Core.Entities.Construction.BudgetPhaseCategory, dynamic>)ViewBag.PhaseData)
            {
                string asamaAdi = "";
                switch (phase.Key)
                {
                    case GMK360.Core.Entities.Construction.BudgetPhaseCategory.ResmiEvraklarVeProsedurler: asamaAdi = "Resmi Evraklar ve Prosedürler"; break;
                    case GMK360.Core.Entities.Construction.BudgetPhaseCategory.YikimVeZeminHazirligi: asamaAdi = "Yıkım ve Zemin Hazırlığı"; break;
                    case GMK360.Core.Entities.Construction.BudgetPhaseCategory.TemelVeAltYapi: asamaAdi = "Temel ve Alt Yapı"; break;
                    case GMK360.Core.Entities.Construction.BudgetPhaseCategory.KabaInsaatKarkas: asamaAdi = "Kaba İnşaat (Karkas)"; break;
                    case GMK360.Core.Entities.Construction.BudgetPhaseCategory.CatiVeDisCephe: asamaAdi = "Çatı ve Dış Cephe"; break;
                    case GMK360.Core.Entities.Construction.BudgetPhaseCategory.InceIslerIcMekan: asamaAdi = "İnce İşler (İç Mekan)"; break;
                    case GMK360.Core.Entities.Construction.BudgetPhaseCategory.ElektrikVeZayifAkim: asamaAdi = "Elektrik ve Zayıf Akım"; break;
                    case GMK360.Core.Entities.Construction.BudgetPhaseCategory.MekanikTesisatVeMakine: asamaAdi = "Mekanik Tesisat ve Makine"; break;
                    case GMK360.Core.Entities.Construction.BudgetPhaseCategory.PeyzajVeTeslim: asamaAdi = "Peyzaj ve Teslim"; break;
                }

                string accId = "collapse" + index;
                string headId = "heading" + index;
                index++;
                
                var items = (List<GMK360.Core.Entities.Construction.ConstructionBudgetItem>)phase.Value.Items;

                <div class="accordion-item border-0 border-bottom">
                    <h2 class="accordion-header" id="@headId">
                        <button class="accordion-button collapsed fw-bold bg-white text-dark" type="button" data-bs-toggle="collapse" data-bs-target="#@accId">
                            <i class="bi bi-building-gear me-2 text-muted"></i> @asamaAdi 
                            <span class="ms-auto badge bg-light text-dark border me-3">Toplam: @(((decimal)phase.Value.Actual).ToString("N2")) ₺</span>
                        </button>
                    </h2>
                    <div id="@accId" class="accordion-collapse collapse" data-bs-parent="#financeAccordion">
                        <div class="accordion-body p-0">
                            <div class="table-responsive">
                                <table class="table table-sm table-hover mb-0">
                                    <thead class="table-light">
                                        <tr>
                                            <th class="ps-4">Kalemler / Başlık</th>
                                            <th class="text-end">Planlanan</th>
                                            <th class="text-end">Gerçekleşen</th>
                                            <th class="text-end pe-4">Fark</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        @if (items.Count == 0)
                                        {
                                            <tr>
                                                <td class="ps-4 text-muted" colspan="4"><em>(Henüz bütçe veya harcama kalemi girilmedi)</em></td>
                                            </tr>
                                        }
                                        else
                                        {
                                            foreach (var item in items)
                                            {
                                                decimal iDiff = item.ActualTotalCost - item.PlannedTotalCost;
                                                string iColor = iDiff > 0 ? "text-danger" : (iDiff < 0 ? "text-success" : "text-muted");
                                                <tr>
                                                    <td class="ps-4">@item.ItemName <small class="text-muted d-block">@item.Quantity @item.Unit</small></td>
                                                    <td class="text-end">@item.PlannedTotalCost.ToString("N2") ₺</td>
                                                    <td class="text-end">@item.ActualTotalCost.ToString("N2") ₺</td>
                                                    <td class="text-end pe-4 fw-bold @iColor">@Math.Abs(iDiff).ToString("N2") ₺ @(iDiff > 0 ? "(Aşıldı)" : (iDiff < 0 ? "(Tasarruf)" : ""))</td>
                                                </tr>
                                            }
                                        }
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>
                </div>
            }
        }
    </div>
</div>
"""

start_idx = content.find('<!-- Orta Kısım: Genel Özet Tablosu -->')
if start_idx != -1:
    content = content[:start_idx] + new_cards
    with io.open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
