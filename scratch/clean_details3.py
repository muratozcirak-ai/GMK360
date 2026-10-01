import io
import re

filepath = r'GMK360.Web\Views\ConstructionProject\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Just wipe out any @if(drafts.Any()) or @foreach(var taskMessage in ...) blocks
content = re.sub(r'var drafts = .*?;', 'var drafts = new List<object>();', content)
content = re.sub(r'drafts\.Any\(\)', 'false', content)
content = re.sub(r'@foreach\s*\(var d in drafts\).*?\}', '', content, flags=re.DOTALL)
content = re.sub(r'recentPhotos', 'new List<object>()', content)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
