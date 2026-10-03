import codecs
import re

path = 'GMK360.Web/Views/PhaseTwo/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace action buttons with a simple Tutar Gir / Güncelle button
target_buttons = r'@if \(item\.QuoteStatus == GMK360\.Core\.Entities\.Construction\.BudgetQuoteStatus\.WaitingForPrice\).*?<!-- Action Buttons -->.*?</div>'
replacement_buttons = '''<!-- Action Buttons -->
                                                <div class="d-flex align-items-center">
                                                    @if (item.PlannedTotalCost == 0)
                                                    {
                                                        <button class="btn btn-sm btn-primary rounded-pill fw-bold px-3 me-2" onclick="openManageModal(@item.Id, '@item.ItemName', '0', '@item.Description', '1', 'Götürü')">
                                                            <i class="bi bi-currency-dollar"></i> Tutar Gir
                                                        </button>
                                                    }
                                                    else
                                                    {
                                                        <button class="btn btn-sm btn-outline-primary rounded-pill fw-bold px-3 me-2" onclick="openManageModal(@item.Id, '@item.ItemName', '@item.PlannedTotalCost.ToString(System.Globalization.CultureInfo.InvariantCulture)', '@item.Description', '@item.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture)', '@item.Unit')">
                                                            <i class="bi bi-pencil"></i> Düzenle
                                                        </button>
                                                    }

                                                    <form action="/PhaseTwo/DeleteItem/@item.Id" method="post" class="d-inline">
                                                        <button type="submit" class="btn btn-sm btn-outline-danger border-0"><i class="bi bi-trash"></i></button>
                                                    </form>
                                                </div>'''
content = re.sub(target_buttons, replacement_buttons, content, flags=re.DOTALL)

# Remove the strategy modal
target_strategy_modal = r'<!-- Strateji Modal -->.*?<!-- Yeni Kalem Ekle Modal -->'
replacement_strategy_modal = '''<!-- Yeni Kalem Ekle Modal -->'''
content = re.sub(target_strategy_modal, replacement_strategy_modal, content, flags=re.DOTALL)

# Remove openStrategyModal function
target_strategy_js = r'function openStrategyModal\(id, title\) \{.*?\}'
replacement_strategy_js = ''
content = re.sub(target_strategy_js, replacement_strategy_js, content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)