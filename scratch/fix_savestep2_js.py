with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

import re

js_fix = r"""        function saveStep2() {
            var form = document.getElementById('step2Form');
            var formData = new FormData(form);
            
            // Fix ASP.NET MVC Unchecked Checkbox issue
            // Find all checkboxes in the form
            var checkboxes = form.querySelectorAll('input[type="checkbox"]');
            checkboxes.forEach(function(cb) {
                if (!cb.checked && cb.name) {
                    formData.append(cb.name, 'false');
                }
            });

            fetch('/ConstructionProject/SaveStep2', {"""

content = re.sub(r'function saveStep2\(\) \{\s*var form = document\.getElementById\(\'step2Form\'\);\s*var formData = new FormData\(form\);\s*fetch\(\'/ConstructionProject/SaveStep2\', \{', js_fix, content, flags=re.DOTALL)

with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
print("Fixed saveStep2 to include unchecked checkboxes as false!")
