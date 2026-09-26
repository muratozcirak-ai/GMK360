import re

with open('GMK360.Web/Views/AdminLegalDocument/Index.cshtml', 'r', encoding='utf-8') as f:
    text = f.read()

# Find and replace the whole script block
pattern = r'@section Scripts \{.*'

new_script = """@section Scripts {
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
        });

        function openEditModal(id, name, stage, module, issuedBy, isMand, prereqsStr) {
            $('#edit-id').val(id);
            $('#edit-name').val(name);
            $('#edit-stage').val(stage);
            $('#edit-module').val(module);
            $('#edit-issuedby').val(issuedBy);
            $('#edit-mand').prop('checked', isMand === 'True' || isMand === 'true');
            
            if(prereqsStr) {
                var arr = prereqsStr.split(',');
                $('#edit-prereqs').val(arr).trigger('change');
            } else {
                $('#edit-prereqs').val(null).trigger('change');
            }
            
            $('#editModal').modal('show');
        }
    </script>
}"""

text = re.sub(pattern, new_script, text, flags=re.DOTALL)

with open('GMK360.Web/Views/AdminLegalDocument/Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print("Script fixed")
