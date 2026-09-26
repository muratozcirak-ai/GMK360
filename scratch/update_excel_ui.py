import sys

filepath = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the table header and body
start_marker = "<table class=\"table table-hover mb-0 align-middle\">"
end_marker = "</table>"

start_idx = content.find(start_marker)
end_idx = content.find(end_marker, start_idx) + len(end_marker)

old_table = content[start_idx:end_idx]

new_table = """<table class="table table-hover mb-0 align-middle table-bordered">
                        <thead class="table-light">
                            <tr>
                                <th style="width: 120px;">Aşama</th>
                                <th>Evrak / Kriter Adı</th>
                                <th>Nerden Alınır</th>
                                <th>Tanıdık</th>
                                <th>Takipçi</th>
                                <th>Başlama T.</th>
                                <th>Bitiş T.</th>
                                <th>Durum</th>
                                <th class="text-end">İşlem</th>
                            </tr>
                        </thead>
                        <tbody>
                            @if (((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs) == null || !((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs).Any())
                            {
                                <tr>
                                    <td colspan="9" class="text-center text-muted py-4">Bu projeye henüz evrak/kriter eklenmemiş.</td>
                                </tr>
                            }
                            else
                            {
                                var docs = (List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs;
                                var groupedDocs = docs.GroupBy(d => d.Stage ?? "Diğer Evraklar").ToList();
                                var globalRules = ViewBag.GlobalRules as List<GMK360.Core.Entities.ModuleDocumentRule> ?? new List<GMK360.Core.Entities.ModuleDocumentRule>();
                                var renderedDocIds = new HashSet<int>();

                                foreach(var group in groupedDocs)
                                {
                                    bool isFirstInGroup = true;
                                    
                                    foreach (var doc in group.OrderByDescending(x => x.IsCustom).ThenBy(x => x.DisplayOrder).ThenBy(x => x.Id))
                                    {
                                        if (renderedDocIds.Contains(doc.Id)) continue;
                                        
                                        var rule = globalRules.FirstOrDefault(r => r.SystemLegalDocumentTemplateId == doc.SystemTemplateId);
                                        bool isOnlyPrereq = globalRules.Any(r => r.Prerequisites != null && r.Prerequisites.Any(p => p.PrerequisiteTemplateId == doc.SystemTemplateId)) && rule == null;
                                        
                                        if (isOnlyPrereq) continue; // Will be rendered under its parent
                                        
                                        var prereqs = rule?.Prerequisites;
                                        bool isDependent = prereqs != null && prereqs.Any();
                                        bool isReadyToApply = true;
                                        if (isDependent)
                                        {
                                            foreach(var pr in prereqs)
                                            {
                                                var cDoc = docs.FirstOrDefault(d => d.SystemTemplateId == pr.PrerequisiteTemplateId);
                                                if (cDoc == null || cDoc.Status != "Alındı")
                                                {
                                                    isReadyToApply = false; break;
                                                }
                                            }
                                        }
                                        
                                        <!-- MAIN DOCUMENT ROW -->
                                        <tr data-bs-toggle="collapse" data-bs-target=".child-of-@doc.Id" style="@(isDependent ? "cursor: pointer;" : "")">
                                            <td class="fw-bold bg-light text-center align-middle" style="border-right: 2px solid #dee2e6; border-bottom: 1px solid #fff;">
                                                @(isFirstInGroup ? group.Key : "")
                                            </td>
                                            <td class="fw-bold">
                                                @if(isDependent) { <i class="bi bi-chevron-down text-muted me-1" style="font-size:0.8rem;"></i> }
                                                @(doc.SystemTemplate?.Name ?? doc.DocumentName)
                                                @if(doc.IsCustom) { <span class="badge bg-info ms-1">Özel</span> }
                                                
                                                @if(isDependent) {
                                                    if(isReadyToApply) {
                                                        <span class="badge bg-success ms-2"><i class="bi bi-unlock-fill"></i> Başvurulabilir</span>
                                                    } else {
                                                        <span class="badge bg-danger ms-2"><i class="bi bi-lock-fill"></i> Bağımlı</span>
                                                    }
                                                }
                                            </td>
                                            <td>
                                                <a href="javascript:void(0)" class="text-primary fw-bold text-decoration-underline" title="Rehbere Ekle / İncele">
                                                    <i class="bi bi-building"></i> @(doc.InstitutionContact ?? doc.SystemTemplate?.IssuedBy ?? "Belirtilmedi")
                                                </a>
                                            </td>
                                            <td>
                                                @if(!string.IsNullOrEmpty(doc.InstitutionContact)) { <span class="text-dark"><i class="bi bi-person-lines-fill text-muted"></i> @doc.InstitutionContact</span> } else { <span class="text-muted">.</span> }
                                            </td>
                                            <td>
                                                @if(string.IsNullOrEmpty(doc.AssignedUserId)) { <span class="text-danger"><i class="bi bi-person-x"></i> Yok</span> } else { <span><i class="bi bi-person-check text-primary"></i> @doc.AssignedUserId</span> }
                                            </td>
                                            <td>@(doc.StartDate.HasValue ? doc.StartDate.Value.ToString("dd.MM.yyyy") : ".")</td>
                                            <td>@(doc.CompletedDate.HasValue ? doc.CompletedDate.Value.ToString("dd.MM.yyyy") : ".")</td>
                                            <td>
                                                @if (doc.Status == "Bekliyor") { <span class="text-danger fw-bold">Yok</span> }
                                                else if (doc.Status == "İşlemde") { <span class="text-warning fw-bold">İşlemde</span> }
                                                else if (doc.Status == "Sorunlu") { <span class="text-secondary fw-bold">Sorunlu</span> }
                                                else { <span class="text-success fw-bold">Alındı</span> }
                                            </td>
                                            <td class="text-end">
                                                <a href="#" class="btn btn-sm btn-outline-primary py-0 px-2 rounded text-decoration-none"><i class="bi bi-pencil-square"></i> Yönet</a>
                                            </td>
                                        </tr>
                                        renderedDocIds.Add(doc.Id);
                                        isFirstInGroup = false;

                                        <!-- CHILD DOCUMENTS ROW (COLLAPSIBLE) -->
                                        if (isDependent)
                                        {
                                            foreach (var pr in prereqs)
                                            {
                                                var childDoc = docs.FirstOrDefault(d => d.SystemTemplateId == pr.PrerequisiteTemplateId);
                                                if (childDoc != null && !renderedDocIds.Contains(childDoc.Id))
                                                {
                                                    <tr class="collapse child-of-@doc.Id" style="background-color: #f8f9fa;">
                                                        <td class="bg-light" style="border-right: 2px solid #dee2e6; border-bottom: 1px solid #fff;"></td>
                                                        <td class="ps-4">
                                                            <i class="bi bi-arrow-return-right text-muted me-2"></i>
                                                            <span class="text-dark">@(childDoc.SystemTemplate?.Name ?? childDoc.DocumentName)</span>
                                                        </td>
                                                        <td>
                                                            <a href="javascript:void(0)" class="text-primary text-decoration-underline">
                                                                <i class="bi bi-building text-muted"></i> @(childDoc.InstitutionContact ?? childDoc.SystemTemplate?.IssuedBy ?? "Belirtilmedi")
                                                            </a>
                                                        </td>
                                                        <td>
                                                            @if(!string.IsNullOrEmpty(childDoc.InstitutionContact)) { <span class="text-dark"><i class="bi bi-person-lines-fill text-muted"></i> @childDoc.InstitutionContact</span> } else { <span class="text-muted">.</span> }
                                                        </td>
                                                        <td>
                                                            @if(string.IsNullOrEmpty(childDoc.AssignedUserId)) { <span class="text-danger"><i class="bi bi-person-x"></i> Yok</span> } else { <span><i class="bi bi-person-check text-primary"></i> @childDoc.AssignedUserId</span> }
                                                        </td>
                                                        <td>@(childDoc.StartDate.HasValue ? childDoc.StartDate.Value.ToString("dd.MM.yyyy") : ".")</td>
                                                        <td>@(childDoc.CompletedDate.HasValue ? childDoc.CompletedDate.Value.ToString("dd.MM.yyyy") : ".")</td>
                                                        <td>
                                                            @if (childDoc.Status == "Bekliyor") { <span class="text-danger fw-bold">Yok</span> }
                                                            else if (childDoc.Status == "İşlemde") { <span class="text-warning fw-bold">İşlemde</span> }
                                                            else if (childDoc.Status == "Sorunlu") { <span class="text-secondary fw-bold">Sorunlu</span> }
                                                            else { <span class="text-success fw-bold">Alındı</span> }
                                                        </td>
                                                        <td class="text-end">
                                                            <a href="#" class="btn btn-sm btn-outline-primary py-0 px-2 rounded text-decoration-none"><i class="bi bi-pencil-square"></i> Yönet</a>
                                                        </td>
                                                    </tr>
                                                    renderedDocIds.Add(childDoc.Id);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        </tbody>
                    </table>"""

content = content.replace(old_table, new_table)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
