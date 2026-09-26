import re

filepath = r'GMK360.Web\Views\Admin\B2bList.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Remove all instances of the added JS block
pattern = r'// --- YENİ EKLENEN: Tekrarlı Kategori.*?initCategoryRepeater\(\);\s*\}\);'
content = re.sub(pattern, '', content, flags=re.DOTALL)

# Add exactly one copy back before the final </script>
js_block = """
        // --- YENİ EKLENEN: Tekrarlı Kategori (Branş) Seçici Mantığı ---
        function initCategoryRepeater() {
            $('.single-select2').select2({
                dropdownParent: $('#addModal')
            });

            $('#addCategoryBtn').off('click').on('click', function() {
                var container = $('#categoryRepeaterContainer');
                var firstRow = container.find('.category-row').first();
                
                firstRow.find('select').select2('destroy');
                
                var newRow = firstRow.clone();
                newRow.find('select').val(''); 
                newRow.find('.remove-category-btn').show(); 
                
                container.append(newRow);
                
                container.find('select.single-select2').select2({
                    dropdownParent: $('#addModal')
                });
            });

            $(document).on('click', '.remove-category-btn', function() {
                $(this).closest('.category-row').remove();
            });
        }
        
        $('#addModal').on('shown.bs.modal', function () {
            initCategoryRepeater();
        });
"""

# The last </script> tag
last_script_idx = content.rfind('</script>')
if last_script_idx != -1:
    content = content[:last_script_idx] + js_block + '\n    </script>' + content[last_script_idx+9:]
else:
    # If no script tag exists at all, just append it
    content += f"\n@section Scripts {{\n<script>{js_block}</script>\n}}"

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Cleaned and injected JS.")
