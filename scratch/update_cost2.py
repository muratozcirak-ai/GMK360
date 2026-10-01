import io
import re

filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

cost_search = r'(@\(\(doc\.DocumentFee\.GetValueOrDefault\(\) \+ doc\.AdditionalCost\.GetValueOrDefault\(\)\)\.ToString\("N2"\)\)\s*.).*?(</td>)'

cost_injection = """@if(bestPrice > 0)
                                                {
                                                    <span class="text-success fs-6"><i class="bi bi-check-circle-fill"></i> @bestPrice.ToString("N2") ₺</span>
                                                }
                                                else
                                                {
                                                    \\1
                                                }
                                            \\2"""

content = re.sub(cost_search, cost_injection, content, flags=re.DOTALL)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated cost column.")
