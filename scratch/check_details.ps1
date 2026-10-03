import codecs
import re

path = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# I want to find the <div class="card shadow-sm border-0 mb-4"> that contains "stagesAccordion"
# We can search for "ViewBag.Phase0Docs" and replace the entire card content.
target = r'<div class="card shadow-sm border-0 mb-4">\s*<div class="card-header bg-white border-bottom-0 pt-4 pb-3">.*?ViewBag\.Phase0Docs.*?</script>\s*</div>\s*</div>'

# Wait, let's just do a string replacement for the inner part or regex for the whole card.
# It's safer to find the section. Let's see the context of "ViewBag.Phase0Docs"