import sys

filepath = 'GMK360.Web/Views/ModuleDocumentRule/Index.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the select to use a standard HTML multiple select with height
content = content.replace(
    """<select name="PrerequisiteTemplateIdsList" class="form-select select2-multiple" multiple="multiple" asp-items="ViewBag.TemplatesSelect" style="width: 100%;">""",
    """<select name="PrerequisiteTemplateIdsList" class="form-select select-multiple-native" multiple="multiple" size="4" asp-items="ViewBag.TemplatesSelect" style="width: 100%;">"""
)

# Remove the Select2 initialization block but keep the toggle logic
js_block_old = """        $(document).ready(function() {
            // Modal içindeki select2'nin düzgün çalışması için dropdownParent ayarı yapıyoruz
            $('.select2-multiple').select2({
                placeholder: "Bağımlı evrakları seçin...",
                allowClear: true,
                dropdownParent: $('#addRuleModal')
            });

            $('#hasPrerequisiteToggle').on('change', function() {
                var div = $('#prerequisiteDiv');
                if(this.checked) {
                    div.slideDown();
                } else {
                    div.slideUp();
                    $('.select2-multiple').val(null).trigger('change');
                }
            });
        });"""

js_block_new = """        document.addEventListener("DOMContentLoaded", function() {
            var toggle = document.getElementById('hasPrerequisiteToggle');
            var div = document.getElementById('prerequisiteDiv');
            var selectMulti = document.querySelector('.select-multiple-native');

            if(toggle) {
                toggle.addEventListener('change', function() {
                    if(this.checked) {
                        div.style.display = 'block';
                    } else {
                        div.style.display = 'none';
                        if(selectMulti) {
                            for(var i=0; i<selectMulti.options.length; i++){
                                selectMulti.options[i].selected = false;
                            }
                        }
                    }
                });
            }
        });"""

content = content.replace(js_block_old, js_block_new)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
