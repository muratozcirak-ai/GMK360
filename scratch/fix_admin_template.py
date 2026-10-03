import codecs
import re

path = 'GMK360.Web/Views/SystemPhaseTemplate/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Update Table Headers
target_th = r'<th>Satınalma Türü</th>'
content = content.replace('<th>Satnalma Tr</th>', '')
content = content.replace('<th>Satınalma Türü</th>', '')

# Update Table Body
target_td = r'<td>\s*@if \(item\.IsQuoteRequired\)[\s\S]*?</td>'
content = re.sub(r'<td>\s*@if \(item\.IsQuoteRequired\).*?</td>', '', content, flags=re.DOTALL)

# Update Select Options
target_select = r'<select name="phaseCategory" class="form-select" required>.*?</select>'
replacement_select = '''<select name="phaseCategory" class="form-select" required>
                        <option value="2">Faz 1: Yıkım ve Zemin Hazırlığı</option>
                        <option value="3">Faz 2: Temel ve Altyapı</option>
                        <option value="4">Faz 3: Kaba Yapı (Betonarme ve Duvar)</option>
                        <option value="5">Faz 4: Çatı ve İzolasyon</option>
                        <option value="6">Faz 5: İnce İşler (Mimari)</option>
                        <option value="8">Faz 6: Mekanik ve Sıhhi Tesisat</option>
                        <option value="7">Faz 7: Elektrik ve Zayıf Akım</option>
                        <option value="9">Faz 8: Çevre Düzenleme ve Teslimat</option>
                    </select>'''
content = re.sub(target_select, replacement_select, content, flags=re.DOTALL)

# Remove IsQuoteRequired from modal
target_quote_modal = r'<div class="mb-3">\s*<label class="form-label fw-bold">Satınalma Türü</label>[\s\S]*?</div>'
content = re.sub(r'<div class="mb-3">\s*<label class="form-label fw-bold">Satnalma Tr</label>.*?</div>', '', content, flags=re.DOTALL)
content = re.sub(r'<div class="mb-3">\s*<label class="form-label fw-bold">Satınalma Türü</label>.*?</div>', '', content, flags=re.DOTALL)

# Phase Name formatter
target_phase_td = r'<td class="ps-4 fw-bold text-primary">Faz @\(\(int\)item\.PhaseCategory\)</td>'
replacement_phase_td = '''<td class="ps-4 fw-bold text-primary">
                                    @if ((int)item.PhaseCategory == 2) { <span>Faz 1: Yıkım</span> }
                                    else if ((int)item.PhaseCategory == 3) { <span>Faz 2: Temel</span> }
                                    else if ((int)item.PhaseCategory == 4) { <span>Faz 3: Kaba Yapı</span> }
                                    else if ((int)item.PhaseCategory == 5) { <span>Faz 4: Çatı</span> }
                                    else if ((int)item.PhaseCategory == 6) { <span>Faz 5: İnce İşler</span> }
                                    else if ((int)item.PhaseCategory == 8) { <span>Faz 6: Mekanik</span> }
                                    else if ((int)item.PhaseCategory == 7) { <span>Faz 7: Elektrik</span> }
                                    else if ((int)item.PhaseCategory == 9) { <span>Faz 8: Peyzaj</span> }
                                </td>'''
content = re.sub(target_phase_td, replacement_phase_td, content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)