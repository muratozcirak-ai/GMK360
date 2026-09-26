import io
import re

filepath = r'GMK360.Web\Views\ConstructionProject\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

pattern = r'(<div id="collapseOldSummary".*?<div class="col-md-8">\s*)(<div class="mt-2 position-relative">)'

replacement = r"""\1@{
                            var eskiBloklar = Model.Blocks?.Where(b => b.IsExistingBuilding).ToList();
                            string yapimYiliText = "Belirtilmedi";
                            if (eskiBloklar != null && eskiBloklar.Any())
                            {
                                var yillar = eskiBloklar.Where(b => b.BuildingAge.HasValue).Select(b => b.BuildingAge.Value).Distinct().OrderBy(y => y).ToList();
                                if (yillar.Any())
                                {
                                    yapimYiliText = string.Join(", ", yillar);
                                }
                            }
                        }
                        <h6 class="fw-bold border-bottom pb-2 mb-3">Eski Bina Bilgileri</h6>
                        <div class="row mb-4">
                            <div class="col-sm-6">
                                <div class="text-muted small">Yapım Yılı</div>
                                <div class="fw-bold text-dark">@yapimYiliText</div>
                            </div>
                        </div>

                        \2"""

content = re.sub(pattern, replacement, content, flags=re.DOTALL)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Added Yapım Yılı to Eski Bina")
