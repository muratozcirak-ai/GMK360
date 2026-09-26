import sys

filepath = 'GMK360.Web/Views/ModuleDocumentRule/Index.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the inner tree render
tree_old = """                                                                        @if (!string.IsNullOrEmpty(item.PrerequisiteTemplateIds))
                                                                        {
                                                                            var prereqIds = item.PrerequisiteTemplateIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Where(x => int.TryParse(x, out _)).Select(int.Parse).ToList();
                                                                            var prereqNames = allTemplates?.Where(t => prereqIds.Contains(t.Id)).Select(t => t.Name).ToList();
                                                                            if(prereqNames != null && prereqNames.Any())
                                                                            {
                                                                                <div class="mt-1 ms-5 text-warning fw-bold" style="font-size: 0.85rem;">
                                                                                    <i class="ph ph-lock-key me-1"></i> Ön Koşullar: 
                                                                                    <span class="text-dark">@string.Join(", ", prereqNames)</span>
                                                                                </div>
                                                                            }
                                                                        }"""

tree_new = """                                                                        @if (item.Prerequisites != null && item.Prerequisites.Any())
                                                                        {
                                                                            <div class="mt-2 ms-4 border-start border-warning ps-3 py-1">
                                                                                <div class="text-warning fw-bold mb-2" style="font-size: 0.85rem;">
                                                                                    <i class="ph ph-lock-key me-1"></i> Bu evrakın ön koşulları (Aşağıdakiler alınmadan bu evrak kilitli kalır):
                                                                                </div>
                                                                                <ul class="list-unstyled mb-0">
                                                                                    @foreach(var pr in item.Prerequisites)
                                                                                    {
                                                                                        <li class="mb-1 d-flex align-items-center">
                                                                                            <i class="ph ph-arrow-elbow-down-right text-muted me-2"></i>
                                                                                            <span class="text-dark fw-bold me-auto">@(pr.PrerequisiteTemplate?.Name ?? "Bilinmiyor")</span>
                                                                                            
                                                                                            <form asp-action="DeletePrerequisite" method="post" class="d-inline ms-3" onsubmit="return confirm('Bu ön koşulu (bağımlılığı) silmek istediğinize emin misiniz?');">
                                                                                                <input type="hidden" name="id" value="@pr.Id" />
                                                                                                <button type="submit" class="btn btn-sm btn-light text-danger border-0 rounded-3 py-0 px-2" title="Sadece bu ön koşulu sil"><i class="ph ph-trash"></i></button>
                                                                                            </form>
                                                                                        </li>
                                                                                    }
                                                                                </ul>
                                                                            </div>
                                                                        }"""

content = content.replace(tree_old, tree_new)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
