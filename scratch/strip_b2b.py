import codecs
import re
import glob

files = glob.glob('GMK360.Web/Views/Phase*/Index.cshtml')
for path in files:
    with codecs.open(path, 'r', 'utf-8-sig') as f:
        content = f.read()

    # Find and remove the relatedQuote if block
    target = r'@if\s*\(relatedQuote\s*!=\s*null\s*&&\s*relatedQuote\.Invites\.Any[\s\S]*?</td></tr>\s*\}'
    content = re.sub(target, '', content)
    
    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)