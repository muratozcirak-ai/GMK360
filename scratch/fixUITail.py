import re

with open('GMK360.Web/Views/AdminLegalDocument/Index.cshtml', 'r', encoding='utf-8') as f:
    text = f.read()

# Find the start of "@section Scripts {" and cut everything after it
idx = text.find('@section Scripts {')
if idx != -1:
    text = text[:idx]

# Append the clean script block
clean_script = """@section Scripts {
    <script>
        $(document).ready(function() {
            $('.select2-multiple').select2({
                placeholder: "Ön koşul belgelerini seçiniz...",
                allowClear: true,
                width: '100%',
                dropdownParent: $('#addModal')
            });
            
            $('#edit-prereqs').select2({
                placeholder: "Ön koşul belgelerini seçiniz...",
                allowClear: true,
                width: '100%',
                dropdownParent: $('#editModal')
            });
            
            $('.edit-btn').on('click', function() {
                var btn = $(this);
                $('#edit-id').val(btn.data('id'));
                $('#edit-name').val(btn.data('name'));
                $('#edit-stage').val(btn.data('stage'));
                $('#edit-module').val(btn.data('module'));
                $('#edit-issuedby').val(btn.data('issuedby'));
                $('#edit-mand').prop('checked', btn.data('ismandatory') === true || btn.data('ismandatory') === 'true');
                
                var prereqsStr = btn.data('prereqs');
                if(prereqsStr && prereqsStr.toString().trim() !== '') {
                    $('#edit-prereqs').val(prereqsStr.toString().split(',')).trigger('change');
                } else {
                    $('#edit-prereqs').val(null).trigger('change');
                }
                
                $('#editModal').modal('show');
            });
        });
    </script>
}
"""
text = text + clean_script

with open('GMK360.Web/Views/AdminLegalDocument/Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print("UI Syntax fixed")
