import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\SeedController.cs'
with open(filepath, 'r', encoding='utf-8-sig') as f:
    content = f.read()

content = re.sub(
    r'_context\.Neighborhoods\.RemoveRange\(_context\.Neighborhoods\);\s*_context\.Districts\.RemoveRange\(_context\.Districts\);\s*_context\.Cities\.RemoveRange\(_context\.Cities\);\s*await _context\.SaveChangesAsync\(\);',
    r'await _context.Database.ExecuteSqlRawAsync("DELETE FROM Neighborhoods; DELETE FROM Districts; DELETE FROM Cities;"); _context.ChangeTracker.Clear();',
    content
)

with open(filepath, 'w', encoding='utf-8-sig') as f:
    f.write(content)
