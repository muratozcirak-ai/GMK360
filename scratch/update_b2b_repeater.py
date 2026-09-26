import re

filepath = r'GMK360.Web\Views\Admin\B2bList.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the HTML block
old_block = """                    <div class="mb-3">
                        <label class="form-label fw-bold small">@(listType == "usta" ? "Ustalık Alanları" : "Sektörleri")</label>
                        <select class="form-select select2-search" multiple="multiple" name="categoryIds" style="width: 100%;" required>
                            @if(categories != null)
                            {
                                foreach(var cat in categories)
                                {
                                    <option value="@cat.Id">@cat.Name</option>
                                }
                            }
                        </select>
                    </div>"""

new_block = """                    <div class="mb-3">
                        <label class="form-label fw-bold small">@(listType == "usta" ? "Ustalık Alanları" : "Sektörleri")</label>
                        <div id="categoryRepeaterContainer">
                            <div class="category-row d-flex gap-2 mb-2">
                                <select class="form-select single-select2" name="categoryIds" style="width: 100%;" required>
                                    <option value="">Seçiniz...</option>
                                    @if(categories != null)
                                    {
                                        foreach(var cat in categories)
                                        {
                                            <option value="@cat.Id">@cat.Name</option>
                                        }
                                    }
                                </select>
                                <button type="button" class="btn btn-outline-danger remove-category-btn" style="display:none;"><i class="bi bi-trash"></i></button>
                            </div>
                        </div>
                        <button type="button" class="btn btn-sm btn-outline-primary mt-1" id="addCategoryBtn">
                            <i class="bi bi-plus-lg"></i> Yanına Ekle (Yeni Satır)
                        </button>
                    </div>"""

# Replace the specific block. (Dealing with encoding and exact whitespace can be tricky, so let's use regex or split)
# Alternatively, replace the <select... block directly.

content = re.sub(
    r'<div class="mb-3">\s*<label class="form-label fw-bold small">@\(listType == "usta" \? "Ustalk Alanlar" : "Sektrleri"\)</label>\s*<select class="form-select select2-search" multiple="multiple" name="categoryIds".*?</select>\s*</div>',
    new_block.replace("Ustalık Alanları", "Ustalk Alanlar").replace("Sektörleri", "Sektrleri").replace("Seçiniz...", "Seiniz..."),
    content,
    flags=re.DOTALL
)

# Insert the javascript for the repeater
js_block = """
        // --- YENİ EKLENEN: Tekrarlı Kategori (Branş) Seçici Mantığı ---
        function initCategoryRepeater() {
            // İlk select2'yi başlat
            $('.single-select2').select2({
                dropdownParent: $('#addB2bModal')
            });

            $('#addCategoryBtn').off('click').on('click', function() {
                var container = $('#categoryRepeaterContainer');
                var firstRow = container.find('.category-row').first();
                
                // Select2'yi geçici olarak destroy et (klonlamadan önce)
                firstRow.find('select').select2('destroy');
                
                var newRow = firstRow.clone();
                newRow.find('select').val(''); // Seçimi temizle
                newRow.find('.remove-category-btn').show(); // Silme butonunu göster
                
                container.append(newRow);
                
                // Hem eskisini hem yenisini tekrar select2 yap
                container.find('select.single-select2').select2({
                    dropdownParent: $('#addB2bModal')
                });
            });

            $(document).on('click', '.remove-category-btn', function() {
                $(this).closest('.category-row').remove();
            });
        }
        
        // Modal açıldığında tetikle
        $('#addB2bModal').on('shown.bs.modal', function () {
            initCategoryRepeater();
        });
"""

if "initCategoryRepeater" not in content:
    content = content.replace("</script>", js_block + "\n    </script>")

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Replaced.")
