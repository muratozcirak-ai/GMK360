import sys
filepath = 'GMK360.Web/Views/AdminLegalDocument/Index.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the jQuery click handler with vanilla Bootstrap 5
js_block_old = """@section Scripts {
    <script>
        $(document).ready(function() {
            $(".edit-btn").on("click", function() {
                var btn = $(this);
                $("#edit-id").val(btn.data("id"));
                $("#edit-name").val(btn.data("name"));
                $("#edit-issuedby").val(btn.data("issuedby"));
                $("#editModal").modal("show");
            });
        });
    </script>
}"""

js_block_new = """@section Scripts {
    <script>
        document.addEventListener("DOMContentLoaded", function() {
            var editButtons = document.querySelectorAll(".edit-btn");
            editButtons.forEach(function(btn) {
                btn.addEventListener("click", function() {
                    document.getElementById("edit-id").value = this.getAttribute("data-id");
                    document.getElementById("edit-name").value = this.getAttribute("data-name");
                    document.getElementById("edit-issuedby").value = this.getAttribute("data-issuedby");
                    
                    var myModal = new bootstrap.Modal(document.getElementById('editModal'));
                    myModal.show();
                });
            });
        });
    </script>
}"""

if js_block_old in content:
    content = content.replace(js_block_old, js_block_new)
else:
    # Just to be safe if indentation is different
    content = content.replace('$("#editModal").modal("show");', 'var myModal = new bootstrap.Modal(document.getElementById("editModal")); myModal.show();')

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
