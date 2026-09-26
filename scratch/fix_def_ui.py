import sys
import re

filepath = 'GMK360.Web/Views/Definition/Values.cshtml'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix order
if 'value="@(Model.Any() ? Model.Max(x => x.Order) + 1 : 1)"' not in content:
    content = re.sub(r'id="valOrd" class="form-control form-control-sm" value="1"', 'id="valOrd" class="form-control form-control-sm" value="@(Model.Any() ? Model.Max(x => x.Order) + 1 : 1)"', content)

# Hide Matrix Settings for B2BSectors in Add form
# Let's wrap the SubOptions and HasCount columns in an if block
add_form_suboptions = """<div class="col-md-3">
                        <div class="form-floating">
                            <input type="text" name="subOptions" id="valSub" class="form-control form-control-sm" placeholder="Virgülle ayırın" />
                            <label for="valSub">Alt Seçenekler (Virgülle, İsteğe Bağlı)</label>
                        </div>
                    </div>
                    <div class="col-md-2 d-flex flex-column justify-content-center">
                        <div class="form-check form-switch form-check-inline ms-2">
                            <input class="form-check-input" type="checkbox" id="valHasCount" name="hasCount" value="true">
                            <label class="form-check-label fw-bold" for="valHasCount">Adet Sorulsun mu?</label>
                        </div>
                        <div class="form-check form-switch form-check-inline ms-2 mt-2">
                            <input class="form-check-input" type="checkbox" id="valTxt" name="requiresTextInput" value="true">
                            <label class="form-check-label fw-bold" for="valTxt">Metin Girişi (Diğer)</label>
                        </div>
                    </div>"""

replacement_suboptions = """@if (category.SystemCode != "B2BSectors")
                    {
                        <div class="col-md-3">
                            <div class="form-floating">
                                <input type="text" name="subOptions" id="valSub" class="form-control form-control-sm" placeholder="Virgülle ayırın" />
                                <label for="valSub">Alt Seçenekler (Virgülle, İsteğe Bağlı)</label>
                            </div>
                        </div>
                        <div class="col-md-2 d-flex flex-column justify-content-center">
                            <div class="form-check form-switch form-check-inline ms-2">
                                <input class="form-check-input" type="checkbox" id="valHasCount" name="hasCount" value="true">
                                <label class="form-check-label fw-bold" for="valHasCount">Adet Sorulsun mu?</label>
                            </div>
                            <div class="form-check form-switch form-check-inline ms-2 mt-2">
                                <input class="form-check-input" type="checkbox" id="valTxt" name="requiresTextInput" value="true">
                                <label class="form-check-label fw-bold" for="valTxt">Metin Girişi (Diğer)</label>
                            </div>
                        </div>
                    }"""

if "category.SystemCode != \"B2BSectors\"" not in content:
    content = content.replace(add_form_suboptions, replacement_suboptions)

# Also hide Matris Ayarları column in table header and body
th_target = '<th class="px-4 py-3 border-0">Matris Ayarları</th>'
th_replacement = '''@if (category.SystemCode != "B2BSectors") {
                            <th class="px-4 py-3 border-0">Matris Ayarları</th>
                        }'''
content = content.replace(th_target, th_replacement)

td_target = '''<td class="px-4 py-3">
                                    @if(val.HasCount) { <span class="badge bg-info me-1">Adet Sorulur</span> }
                                    @if(!string.IsNullOrEmpty(val.SubOptions)) { <span class="badge bg-warning text-dark" title="@val.SubOptions">Alt Seçenekler Var</span> }
                                </td>'''
td_replacement = '''@if (category.SystemCode != "B2BSectors") {
                                <td class="px-4 py-3">
                                    @if(val.HasCount) { <span class="badge bg-info me-1">Adet Sorulur</span> }
                                    @if(!string.IsNullOrEmpty(val.SubOptions)) { <span class="badge bg-warning text-dark" title="@val.SubOptions">Alt Seçenekler Var</span> }
                                </td>
                                }'''
content = content.replace(td_target, td_replacement)

# Oh wait, the systemcode column. Let's hide that too for B2BSectors?
# "Sistem Kodu"
th_sys = '<th class="px-4 py-3 border-0">Sistem Kodu</th>'
th_sys_rep = '''@if (category.SystemCode != "B2BSectors") {
                            <th class="px-4 py-3 border-0">Sistem Kodu</th>
                        }'''
content = content.replace(th_sys, th_sys_rep)

td_sys = '''<td class="px-4 py-3">
                                    @if(!string.IsNullOrEmpty(val.SystemCode)) {
                                        <span class="badge bg-secondary bg-opacity-10 text-secondary border border-secondary border-opacity-25 px-2 py-1">@val.SystemCode</span>
                                    } else {
                                        <span class="text-muted">-</span>
                                    }
                                </td>'''
td_sys_rep = '''@if (category.SystemCode != "B2BSectors") {
                                <td class="px-4 py-3">
                                    @if(!string.IsNullOrEmpty(val.SystemCode)) {
                                        <span class="badge bg-secondary bg-opacity-10 text-secondary border border-secondary border-opacity-25 px-2 py-1">@val.SystemCode</span>
                                    } else {
                                        <span class="text-muted">-</span>
                                    }
                                </td>
                                }'''
content = content.replace(td_sys, td_sys_rep)


# Also in Add form for SystemCode:
syscode_add = """<div class="col-md-2">
                        <div class="form-floating">
                            <input type="text" name="systemCode" id="valSys" class="form-control form-control-sm" placeholder="Sistem Kodu" />
                            <label for="valSys">Sistem Kodu (İsteğe Bağlı)</label>
                        </div>
                    </div>"""
syscode_add_rep = """@if (category.SystemCode != "B2BSectors")
                    {
                        <div class="col-md-2">
                            <div class="form-floating">
                                <input type="text" name="systemCode" id="valSys" class="form-control form-control-sm" placeholder="Sistem Kodu" />
                                <label for="valSys">Sistem Kodu (İsteğe Bağlı)</label>
                            </div>
                        </div>
                    }
                    else
                    {
                        <input type="hidden" name="systemCode" value="" />
                    }"""
content = content.replace(syscode_add, syscode_add_rep)


with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
