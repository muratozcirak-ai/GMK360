import codecs
import re

path = 'GMK360.Web/Controllers/ProjectFinanceController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'// Bütçe Aşama Verileri\s*var budgetItems = project\.BudgetItems\?\.ToList\(\) \?\? new List<ConstructionBudgetItem>\(\);'
replacement = '''// Bütçe Aşama Verileri
            var budgetItems = project.BudgetItems?.ToList() ?? new List<ConstructionBudgetItem>();
            
            // Faz 0 Evraklarını Sanal Bütçe Kalemi Olarak Ekle
            var phase0Docs = await _context.ProjectLegalDocuments
                .Where(d => d.ConstructionProjectId == projectId)
                .ToListAsync();
            
            var phase0BudgetItems = phase0Docs.Select(d => new GMK360.Core.Entities.Construction.ConstructionBudgetItem
            {
                ItemName = d.DocumentName,
                PlannedUnitPrice = d.EstimatedCost ?? 0,
                Quantity = 1,
                ActualTotalCost = d.ActualCost ?? 0,
                PhaseCategory = GMK360.Core.Entities.Construction.BudgetPhaseCategory.ResmiEvraklarVeProsedurler
            }).ToList();
            
            budgetItems.AddRange(phase0BudgetItems);'''
content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)