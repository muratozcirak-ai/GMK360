import io
import re

filepath = r'GMK360.Web\Views\ConstructionProject\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

# Pattern for the block to move
block_pattern = r'(<h6 class="fw-bold border-bottom pb-2 mb-3">.*?<div class="mt-4 pt-3 border-top position-relative">)'

match = re.search(block_pattern, content, flags=re.DOTALL)
if match:
    block = match.group(1)
    
    # Remove from original place
    new_content = content.replace(block, '<div class="mt-2 position-relative">')
    
    # Modify labels inside the block
    modified_block = block.replace('Başlangıç Tarihi', 'Proje Başlama Tarihi')
    modified_block = modified_block.replace('Balang Tarihi', 'Proje Başlama Tarihi') # Encoding fix if needed
    modified_block = modified_block.replace('Bitiş Tarihi', 'Tahmini Teslim Tarihi')
    modified_block = modified_block.replace('Biti Tarihi', 'Tahmini Teslim Tarihi')
    
    # Insert into new place
    target_pattern = r'(<div id="collapseNewSummary" class="accordion-collapse collapse" aria-labelledby="headingNewSummary">\s*<div class="accordion-body p-4">\s*<div class="row g-4">\s*<div class="col-md-8">\s*)<div class="mt-2">'
    
    new_content = re.sub(target_pattern, r'\1' + modified_block.replace('\\', '\\\\') + '\n', new_content, flags=re.DOTALL)
    
    with io.open(filepath, 'w', encoding='utf-8') as f:
        f.write(new_content)
    print("Moved successfully in Details.cshtml")
else:
    print("Pattern not found in Details.cshtml")
