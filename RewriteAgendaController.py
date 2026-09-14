import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\AgendaController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

import re

# Find the Create POST method
match = re.search(r'\[HttpPost\].*?public async Task<IActionResult> Create.*?return RedirectToAction.*?}', content, re.DOTALL)
if match:
    old_create = match.group(0)
    new_create = """[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AgendaRecord record, IFormFile imageFile, List<string> itemTopicTitle, List<string> itemPresentationText)
        {
            var agencyId = GetCurrentAgencyId();
            record.AgencyId = agencyId;
            record.CreatedAt = DateTime.Now;
            record.IsDeleted = false;
            
            // Ana toplantı resmi (Opsiyonel)
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "agenda");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                record.ImageUrl = "/uploads/agenda/" + uniqueFileName;
            }

            // Gündem Maddeleri (Slaytlar)
            record.Items = new List<AgendaItem>();
            if (itemTopicTitle != null && itemTopicTitle.Count > 0)
            {
                for (int i = 0; i < itemTopicTitle.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(itemTopicTitle[i])) continue;

                    var newItem = new AgendaItem
                    {
                        OrderNo = i + 1,
                        TopicTitle = itemTopicTitle[i],
                        PresentationText = itemPresentationText != null && itemPresentationText.Count > i ? itemPresentationText[i] : "",
                        LiveMeetingNotes = ""
                    };

                    // Her gündem maddesinin kendi resmi
                    var slideImage = Request.Form.Files["itemImage_" + i];
                    if (slideImage != null && slideImage.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "agenda");
                        Directory.CreateDirectory(uploadsFolder);
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + slideImage.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await slideImage.CopyToAsync(fileStream);
                        }
                        newItem.ImageUrl = "/uploads/agenda/" + uniqueFileName;
                    }

                    record.Items.Add(newItem);
                }
            }

            _context.AgendaRecords.Add(record);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { selectedDate = record.EventDate.ToString("yyyy-MM-dd") });
        }"""
    
    content = content.replace(old_create, new_create)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
