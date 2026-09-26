import sys
filepath = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

start_marker = "<table class=\"table table-hover mb-0 align-middle table-bordered\">"
end_marker = "</table>"

start_idx = content.find(start_marker)
end_idx = content.find(end_marker, start_idx) + len(end_marker)

old_block = content[start_idx:end_idx]

new_block = """
                    @if (((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs) == null || !((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs).Any())
                    {
                        <div class="text-center text-muted py-4">Bu projeye henüz evrak/kriter eklenmemiş.</div>
                    }
                    else
                    {
                        var docs = (List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs;
                        var groupedDocs = docs.GroupBy(d => d.Stage ?? "Diğer Evraklar").ToList();
                        var globalRules = ViewBag.GlobalRules as List<GMK360.Core.Entities.ModuleDocumentRule> ?? new List<GMK360.Core.Entities.ModuleDocumentRule>();
                        var renderedDocIds = new HashSet<int>();
                        int stageIndex = 0;

                        <div class="accordion accordion-flush border-0" id="stagesAccordion">
                            @foreach(var group in groupedDocs)
                            {
                                stageIndex++;
                                <div class="accordion-item border mb-3 rounded shadow-sm">
                                    <h2 class="accordion-header" id="headingStage-@stageIndex">
                                        <button class="accordion-button @(stageIndex != 1 ? "collapsed" : "") fw-bold fs-6 text-primary bg-light" type="button" data-bs-toggle="collapse" data-bs-target="#collapseStage-@stageIndex" aria-expanded="@(stageIndex == 1 ? "true" : "false")" aria-controls="collapseStage-@stageIndex">
                                            <i class="bi bi-layers me-2"></i> @group.Key
                                            <span class="badge bg-secondary ms-auto rounded-pill me-3">@group.Count() Evrak</span>
                                        </button>
                                    </h2>
                                    <div id="collapseStage-@stageIndex" class="accordion-collapse collapse @(stageIndex == 1 ? "show" : "")" aria-labelledby="headingStage-@stageIndex" data-bs-parent="#stagesAccordion">
                                        <div class="accordion-body p-0">
                                            <div class="table-responsive">
                                                <table class="table table-hover mb-0 align-middle table-borderless table-striped">
                                                    <thead class="table-light border-bottom">
                                                        <tr>
                                                            <th class="ps-4">Evrak / Kriter Adı</th>
                                                            <th>Nereden Alınır</th>
                                                            <th>Tanıdık</th>
                                                            <th>Takipçi</th>
                                                            <th>Başlama T.</th>
                                                            <th>Bitiş T.</th>
                                                            <th>Durum</th>
                                                            <th class="text-end pe-4">İşlem</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                                        @foreach (var doc in group.OrderByDescending(x => x.IsCustom).ThenBy(x => x.DisplayOrder).ThenBy(x => x.Id))
                                                        {
                                                            if (renderedDocIds.Contains(doc.Id)) continue;
                                                            
                                                            var rule = globalRules.FirstOrDefault(r => r.SystemLegalDocumentTemplateId == doc.SystemTemplateId);
                                                            bool isOnlyPrereq = globalRules.Any(r => r.Prerequisites != null && r.Prerequisites.Any(p => p.PrerequisiteTemplateId == doc.SystemTemplateId)) && rule == null;
                                                            
                                                            if (isOnlyPrereq) continue; // Altında render edilecek
                                                            
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
                                                            
                                                            <!-- MAIN DOC ROW -->
                                                            <tr data-bs-toggle="@(isDependent ? "collapse" : "")" data-bs-target=".child-of-@doc.Id" style="@(isDependent ? "cursor: pointer;" : "")" class="border-bottom">
                                                                <td class="fw-bold ps-4">
                                                                    @if(isDependent) { <i class="bi bi-chevron-expand text-primary me-2 fs-5 align-middle"></i> } else { <span style="margin-left: 28px;"></span> }
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
                                                                    <a href="javascript:void(0)" class="text-primary text-decoration-none fw-semibold">
                                                                        <i class="bi bi-building"></i> @(doc.InstitutionContact ?? doc.SystemTemplate?.IssuedBy ?? "Belirtilmedi")
                                                                    </a>
                                                                </td>
                                                                <td>
                                                                    @if(!string.IsNullOrEmpty(doc.InstitutionContact)) { <span class="text-dark"><i class="bi bi-person-lines-fill text-muted"></i> @doc.InstitutionContact</span> } else { <span class="text-muted">-</span> }
                                                                </td>
                                                                <td>
                                                                    @if(string.IsNullOrEmpty(doc.AssignedUserId)) { <span class="text-danger"><i class="bi bi-person-x"></i> Yok</span> } else { <span><i class="bi bi-person-check text-primary"></i> @doc.AssignedUserId</span> }
                                                                </td>
                                                                <td>@(doc.StartDate.HasValue ? doc.StartDate.Value.ToString("dd.MM.yyyy") : "-")</td>
                                                                <td>@(doc.CompletedDate.HasValue ? doc.CompletedDate.Value.ToString("dd.MM.yyyy") : "-")</td>
                                                                <td>
                                                                    @if (doc.Status == "Bekliyor") { <span class="text-danger fw-bold">Yok</span> }
                                                                    else if (doc.Status == "İşlemde") { <span class="text-warning fw-bold">İşlemde</span> }
                                                                    else if (doc.Status == "Sorunlu") { <span class="text-secondary fw-bold">Sorunlu</span> }
                                                                    else { <span class="text-success fw-bold">Alındı</span> }
                                                                </td>
                                                                <td class="text-end pe-4">
                                                                    <a href="#" class="btn btn-sm btn-outline-primary py-0 px-3 rounded-pill text-decoration-none fw-bold" onclick="event.stopPropagation();"><i class="bi bi-pencil-square"></i> Yönet</a>
                                                                </td>
                                                            </tr>
                                                            renderedDocIds.Add(doc.Id);

                                                            <!-- CHILD DOCS (COLLAPSIBLE) -->
                                                            if (isDependent)
                                                            {
                                                                foreach (var pr in prereqs)
                                                                {
                                                                    var childDoc = docs.FirstOrDefault(d => d.SystemTemplateId == pr.PrerequisiteTemplateId);
                                                                    if (childDoc != null && !renderedDocIds.Contains(childDoc.Id))
                                                                    {
                                                                        <tr class="collapse child-of-@doc.Id bg-white" style="border-left: 4px solid #ffc107;">
                                                                            <td class="ps-5">
                                                                                <i class="bi bi-arrow-return-right text-muted me-2 ms-4"></i>
                                                                                <span class="text-secondary fw-semibold">@(childDoc.SystemTemplate?.Name ?? childDoc.DocumentName)</span>
                                                                            </td>
                                                                            <td>
                                                                                <a href="javascript:void(0)" class="text-secondary text-decoration-none">
                                                                                    <i class="bi bi-building text-muted"></i> @(childDoc.InstitutionContact ?? childDoc.SystemTemplate?.IssuedBy ?? "Belirtilmedi")
                                                                                </a>
                                                                            </td>
                                                                            <td>
                                                                                @if(!string.IsNullOrEmpty(childDoc.InstitutionContact)) { <span class="text-dark"><i class="bi bi-person-lines-fill text-muted"></i> @childDoc.InstitutionContact</span> } else { <span class="text-muted">-</span> }
                                                                            </td>
                                                                            <td>
                                                                                @if(string.IsNullOrEmpty(childDoc.AssignedUserId)) { <span class="text-danger"><i class="bi bi-person-x"></i> Yok</span> } else { <span><i class="bi bi-person-check text-primary"></i> @childDoc.AssignedUserId</span> }
                                                                            </td>
                                                                            <td>@(childDoc.StartDate.HasValue ? childDoc.StartDate.Value.ToString("dd.MM.yyyy") : "-")</td>
                                                                            <td>@(childDoc.CompletedDate.HasValue ? childDoc.CompletedDate.Value.ToString("dd.MM.yyyy") : "-")</td>
                                                                            <td>
                                                                                @if (childDoc.Status == "Bekliyor") { <span class="text-danger fw-bold">Yok</span> }
                                                                                else if (childDoc.Status == "İşlemde") { <span class="text-warning fw-bold">İşlemde</span> }
                                                                                else if (childDoc.Status == "Sorunlu") { <span class="text-secondary fw-bold">Sorunlu</span> }
                                                                                else { <span class="text-success fw-bold">Alındı</span> }
                                                                            </td>
                                                                            <td class="text-end pe-4">
                                                                                <a href="#" class="btn btn-sm btn-outline-secondary py-0 px-3 rounded-pill text-decoration-none fw-bold"><i class="bi bi-pencil-square"></i> Yönet</a>
                                                                            </td>
                                                                        </tr>
                                                                        renderedDocIds.Add(childDoc.Id);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    </tbody>
                                                </table>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            }
                        </div>
                    }
"""

content = content.replace(old_block, new_block)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
