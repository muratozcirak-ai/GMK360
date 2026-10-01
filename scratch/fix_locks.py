import io

filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Inject isReadyToApply calculation
old_dep = "bool isDependent = prereqs != null && prereqs.Any();"
new_dep = """bool isDependent = prereqs != null && prereqs.Any();
                                        bool isReadyToApply = true;
                                        if (isDependent) {
                                            foreach (var pr in prereqs) {
                                                var cDoc = Model.FirstOrDefault(d => d.SystemTemplateId == pr.PrerequisiteTemplateId);
                                                if (cDoc == null || cDoc.Status != "Alındı") {
                                                    isReadyToApply = false; break;
                                                }
                                            }
                                        }"""
content = content.replace(old_dep, new_dep)


# 2. Update the "Bu evrakın ön koşulları var" text with the colored lock logic
old_text = """<br/><small class="text-muted ms-4" style="font-size: 0.75rem;"><i class="bi bi-lock-fill text-warning"></i> Bu evrakın ön koşulları var</small>"""
new_text = """<br/>
                                                    @if(isReadyToApply) {
                                                        <small class="text-success ms-4" style="font-size: 0.75rem;"><i class="bi bi-unlock-fill"></i> Ön koşullar sağlandı</small>
                                                    } else {
                                                        <small class="text-danger ms-4" style="font-size: 0.75rem;"><i class="bi bi-lock-fill"></i> Ön koşullar tamamlanmadı!</small>
                                                    }"""
content = content.replace(old_text, new_text)

# 3. Disable the Manage button for Main docs if not ready
old_btn = """<button class="btn btn-sm btn-outline-primary rounded-pill px-3 fw-bold shadow-sm" onclick="openManageModal(@doc.Id)"><i class="bi bi-pencil-square"></i> Yönet</button>"""
new_btn = """<button class="btn btn-sm btn-outline-primary rounded-pill px-3 fw-bold shadow-sm" onclick="openManageModal(@doc.Id)" @(isDependent && !isReadyToApply ? "disabled title='Önce alt ön koşul evrakları tamamlanmalıdır'" : "")>
                                                      @if(isDependent && !isReadyToApply) { <i class="bi bi-lock-fill text-danger"></i> } else { <i class="bi bi-pencil-square"></i> } Yönet
                                                  </button>"""
content = content.replace(old_btn, new_btn)


with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated Index.cshtml with lock logic")
