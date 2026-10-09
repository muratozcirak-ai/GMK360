import codecs
import re
import glob

# Views to update
views = glob.glob('GMK360.Web/Views/Phase*/Index.cshtml')

for path in views:
    try:
        with codecs.open(path, 'r', 'utf-8-sig') as f:
            content = f.read()

        # 1. Fix the Javascript openManageModal
        content = content.replace("document.getElementById('managePrice').value = price;", 
                                  "document.getElementById('managePrice').value = (price === '0' || price === '0,00' || price === '0.00') ? '' : price;")

        # 2. Add Total to Accordion Headers
        # Find where items are filtered: var items = Model.Where(m => m.SubCategory == subCategory).ToList();
        # And the button right after it.
        
        # We need to inject the sum logic
        if 'var categoryTotal' not in content:
            # Inject sum calculation
            content = re.sub(r'(var items = Model\.Where\(m => m\.SubCategory == subCategory\)\.ToList\(\);)', 
                             r'\1\n                    var categoryTotal = items.Sum(i => i.PlannedTotalCost);', 
                             content)

            # Inject badge into the button
            # Look for: <span class="fw-bold fs-5">@subCategory</span>
            content = re.sub(r'(<span class="fw-bold fs-5">@subCategory</span>)', 
                             r'\1\n                                    <span class="ms-auto badge bg-white text-primary rounded-pill fs-6 px-3 py-2 shadow-sm border border-primary">Toplam: @categoryTotal.ToString("N2") ₺</span>', 
                             content)

        with codecs.open(path, 'w', 'utf-8-sig') as f:
            f.write(content)
            
        print(f"Updated {path}")
    except Exception as e:
        print(f"Error processing {path}: {e}")
