import codecs
import re
import glob

files = glob.glob('GMK360.Web/Views/Phase*/Index.cshtml')
for path in files:
    with codecs.open(path, 'r', 'utf-8-sig') as f:
        content = f.read()

    # Find and remove the relatedQuote if block for B2B quotes
    target = r'@if\s*\(relatedQuote\s*!=\s*null[\s\S]*?<!-- End B2B Quotes row -->'
    
    # Wait, does the block have an end comment? Let's check.
    # I'll just use a more generic match.
    # It's an entire <tr> element starting with @if (relatedQuote...
    # I'll use regex to remove it.