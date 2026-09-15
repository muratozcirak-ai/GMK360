import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs'
with codecs.open(filepath, 'r', 'utf-8', errors='ignore') as f:
    content = f.read()

# 1. Fix Status mapping (was lost during revert)
old_code = r"project = new GMK360.Core.Entities.Construction.ConstructionProject { AgencyId = agencyId.Value, Status = GMK360.Core.Entities.Construction.ProjectStatus.Projelendirme_Teklif, CreatedAt = DateTime.UtcNow };"
new_code = r"project = new GMK360.Core.Entities.Construction.ConstructionProject { AgencyId = agencyId.Value, Status = (GMK360.Core.Entities.Construction.ProjectStatus)model.StatusId, CreatedAt = DateTime.UtcNow };"
content = content.replace(old_code, new_code)

insert_target = r"project.Name = model.Name;"
insert_code = r"project.Status = (GMK360.Core.Entities.Construction.ProjectStatus)model.StatusId;" + "\n                " + insert_target
content = content.replace(insert_target, insert_code, 1)

# 2. Fix Latitude/Longitude parsing
lat_old = 'project.Latitude = model.Latitude;'
lat_new = '''if (!string.IsNullOrEmpty(model.Latitude)) {
                    if (double.TryParse(model.Latitude.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lat)) {
                        project.Latitude = lat;
                    }
                }'''
lng_old = 'project.Longitude = model.Longitude;'
lng_new = '''if (!string.IsNullOrEmpty(model.Longitude)) {
                    if (double.TryParse(model.Longitude.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lng)) {
                        project.Longitude = lng;
                    }
                }'''
content = content.replace(lat_old, lat_new)
content = content.replace(lng_old, lng_new)

# 3. Add ViewData for Layout in Details(int? id)
details_find = '''ViewBag.Phase0Docs = await _context.ProjectLegalDocuments
                .Include(d => d.SystemTemplate)
                .Where(d => d.ConstructionProjectId == id)
                .ToListAsync();'''
details_replace = '''ViewBag.Phase0Docs = await _context.ProjectLegalDocuments
                .Include(d => d.SystemTemplate)
                .Where(d => d.ConstructionProjectId == id)
                .ToListAsync();
            
            ViewData["ProjectName"] = await _context.ConstructionProjects.Where(p => p.Id == id).Select(p => p.Name).FirstOrDefaultAsync();
            ViewData["ProjectId"] = id;'''
content = content.replace(details_find, details_replace, 1)

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
