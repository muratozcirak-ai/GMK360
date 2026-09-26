import sys
import re

filepath = 'GMK360.Web/Views/Admin/GlobalProviders.cshtml'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace Category type cast
content = content.replace("IEnumerable<GMK360.Core.Entities.B2b.B2bCategory>", "IEnumerable<GMK360.Core.Entities.DefinitionValue>")

# Add Cities cast
if "var cities" not in content:
    content = content.replace("var categories = ViewBag.Categories", "var categories = ViewBag.Categories as IEnumerable<GMK360.Core.Entities.DefinitionValue>;\n    var cities = ViewBag.Cities as IEnumerable<GMK360.Core.Entities.City>;")

# Inject Select2 and Cities filter
cities_html = """
                    <select class="form-select border-0 shadow-sm select2-search" style="width: 200px;" data-placeholder="Tüm İller">
                        <option value="">Tüm İller</option>
                        @if(cities != null)
                        {
                            foreach(var city in cities)
                            {
                                <option value="@city.Id">@city.Name</option>
                            }
                        }
                    </select>
"""

# replace the closing </select> of categories with the cities html
target_select = """</select>
                    <button class="btn rounded-pill text-white px-4 shadow-sm" """

if "data-placeholder=\"Tüm İller\"" not in content:
    content = content.replace(target_select, f"</select>{cities_html}                    <button class=\"btn rounded-pill text-white px-4 shadow-sm\" ")

# Change category rendering in the table
content = content.replace("@cc.B2bCategory?.Name", "@cc.DefinitionValue?.Name")

# Add Select2 script to the bottom
select2_script = """
@section Scripts {
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
    <script>
        $(document).ready(function() {
            $('.select2-search').select2({
                theme: "classic",
                width: 'resolve'
            });
        });
    </script>
}
"""
if "select2.min.js" not in content:
    content += select2_script

# Make the form selects in the header use select2
content = content.replace("<select class=\"form-select rounded-pill border-0 shadow-sm\"", "<select class=\"form-select border-0 shadow-sm select2-search\"")
content = content.replace("<select class=\"form-select\"", "<select class=\"form-select select2-search\"")

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
    print("GlobalProviders View updated with Select2 and Cities.")
