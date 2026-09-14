import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\AgendaController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('return RedirectToAction(nameof(Index), new { selectedDate = record.EventDate.ToString("yyyy-MM-dd") });\r\n        });', 'return RedirectToAction(nameof(Index), new { selectedDate = record.EventDate.ToString("yyyy-MM-dd") });\r\n        }')
content = content.replace('return RedirectToAction(nameof(Index), new { selectedDate = record.EventDate.ToString("yyyy-MM-dd") });\n        });', 'return RedirectToAction(nameof(Index), new { selectedDate = record.EventDate.ToString("yyyy-MM-dd") });\n        }')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
