import re

filepath = r'GMK360.Web\Views\Admin\B2bList.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# I will find the exact index of 'multiple="multiple" name="categoryIds"'
select_start = content.find('<select class="form-select select2-search" multiple="multiple" name="categoryIds"')
if select_start != -1:
    select_end = content.find('</select>', select_start) + len('</select>')
    
    new_html = """<div id="categoryRepeaterContainer">
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
                        </button>"""
                        
    content = content[:select_start] + new_html + content[select_end:]
    
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Replaced select2 multiple.")
else:
    print("Select2 multiple not found. Already replaced?")
