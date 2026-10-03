import codecs
import re

path = 'GMK360.Web/Controllers/ConstructionProjectExpensesController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'public async Task<IActionResult> Index\(int\? projectId\)\s*\{'
replacement = '''public async Task<IActionResult> Index(int? projectId, int? expenseType, DateTime? startDate, DateTime? endDate)
        {
            ViewBag.SelectedType = expenseType;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");'''
content = re.sub(target, replacement, content)

target_query = r'if \(projectId\.HasValue\)\s*\{\s*query = query\.Where\(e => e\.ProjectId == projectId\.Value\);\s*\}'
replacement_query = '''if (projectId.HasValue)
            {
                query = query.Where(e => e.ProjectId == projectId.Value);
            }
            if (expenseType.HasValue)
            {
                query = query.Where(e => (int)e.ExpenseType == expenseType.Value);
            }
            if (startDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate <= endDate.Value);
            }'''
content = re.sub(target_query, replacement_query, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)