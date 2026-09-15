import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\SeedController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

old_code = '''            // Clear old data to prevent conflicts (except Country)
            _context.Neighborhoods.RemoveRange(_context.Neighborhoods);
            _context.Districts.RemoveRange(_context.Districts);
            _context.Cities.RemoveRange(_context.Cities);
            await _context.SaveChangesAsync();'''

new_code = '''            // Clear old data to prevent conflicts (except Country)
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Neighborhoods");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Districts");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Cities");
            _context.ChangeTracker.Clear();'''

content = content.replace(old_code, new_code)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
