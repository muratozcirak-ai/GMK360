import io
import re

def clean_file(filepath, strings_to_remove):
    try:
        with io.open(filepath, 'r', encoding='utf-8') as f:
            lines = f.readlines()
        
        new_lines = []
        skip = False
        for i, line in enumerate(lines):
            # If line is an anchor tag opening with specific action
            if any(s in line for s in strings_to_remove):
                if '<a ' in line and '</a>' not in line:
                    skip = True
                continue
            
            if skip:
                if '</a>' in line:
                    skip = False
                continue

            new_lines.append(line)
            
        with io.open(filepath, 'w', encoding='utf-8') as f:
            f.writelines(new_lines)
    except Exception as e:
        print(e)

# Details.cshtml
clean_file(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', ['asp-action="Feasibility"', 'asp-action="ManagePhases"'])

# _ProjectLayout.cshtml
clean_file(r'GMK360.Web\Views\Shared\_ProjectLayout.cshtml', ['ManagePhases', 'CostAnalysis'])

print("Cleaned views")
