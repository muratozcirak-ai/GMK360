import re

ctrl_file = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with open(ctrl_file, 'r', encoding='utf-8') as f:
    ctrl_content = f.read()

pattern = r'ViewBag\.Phase0Docs = await _context\.ProjectLegalDocuments[\s\S]*?ToListAsync\(\);'
new_code = '''var phase0Raw = await _context.ProjectLegalDocuments
                .Include(d => d.SystemTemplate)
                .Where(d => d.ConstructionProjectId == id)
                .OrderBy(d => d.DisplayOrder).ThenBy(d => d.Id)
                .ToListAsync();
            ViewBag.Phase0Docs = phase0Raw;
            ViewBag.GroupedPhase0Docs = phase0Raw.GroupBy(d => d.Stage ?? "Aşama Belirtilmemiş").ToList();
'''
ctrl_content = re.sub(pattern, new_code, ctrl_content)

with open(ctrl_file, 'w', encoding='utf-8-sig') as f:
    f.write(ctrl_content)
