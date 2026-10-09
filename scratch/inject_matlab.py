import codecs
import re
import glob

html_snippet = '''
                <div class="row g-2 mb-3">
                    <div class="col-md-6">
                        <label class="form-label fw-bold text-dark">Tahmini Malzeme (₺)</label>
                        <input type="number" step="0.01" name="estimatedMaterialCost" id="manageMaterial" class="form-control" oninput="calcTotal()" />
                    </div>
                    <div class="col-md-6">
                        <label class="form-label fw-bold text-dark">Tahmini İşçilik (₺)</label>
                        <input type="number" step="0.01" name="estimatedLaborCost" id="manageLabor" class="form-control" oninput="calcTotal()" />
                    </div>
                </div>
                <script>
                    function calcTotal() {
                        let m = parseFloat(document.getElementById('manageMaterial').value) || 0;
                        let l = parseFloat(document.getElementById('manageLabor').value) || 0;
                        document.getElementById('managePrice').value = m + l;
                    }
                </script>
'''

views = glob.glob('GMK360.Web/Views/Phase*/Index.cshtml')
for path in views:
    if 'PhaseZero' in path: continue
    with codecs.open(path, 'r', 'utf-8-sig') as f:
        content = f.read()

    if 'manageMaterial' not in content:
        content = re.sub(
            r'(<div class="mb-3">\s*<label class="form-label fw-bold text-primary">Tahmini Toplam Tutar)',
            html_snippet + r'\n                \1',
            content
        )
        # Update openManageModal definition
        content = content.replace(
            'function openManageModal(id, title, price, desc, qty, unit) {',
            'function openManageModal(id, title, price, desc, qty, unit, mat, lab) {'
        )
        content = content.replace(
            "document.getElementById('managePrice').value = (price === '0' || price === '0,00' || price === '0.00') ? '' : price;",
            "document.getElementById('managePrice').value = (price === '0' || price === '0,00' || price === '0.00') ? '' : price;\n        document.getElementById('manageMaterial').value = (mat === 'null' || mat === '0' || mat === '') ? '' : mat;\n        document.getElementById('manageLabor').value = (lab === 'null' || lab === '0' || lab === '') ? '' : lab;"
        )
        
        # We need to update the button onclick passing the new arguments!
        # The existing is: onclick="openManageModal('@item.Id', '@item.ItemName.Replace("'","\\'")', '@item.PlannedTotalCost', '@item.Description', '@item.Quantity', '@item.Unit')"
        content = re.sub(
            r"onclick=\"openManageModal\('@item\.Id', '@item\.ItemName\.Replace\(\"'\",\"\\\\'\"\)', '@item\.PlannedTotalCost', '@item\.Description', '@item\.Quantity', '@item\.Unit'\)\"",
            r"onclick=\"openManageModal('@item.Id', '@item.ItemName.Replace(\"'\",\"\\\\'\")', '@item.PlannedTotalCost', '@item.Description', '@item.Quantity', '@item.Unit', '@item.EstimatedMaterialCost', '@item.EstimatedLaborCost')\"",
            content
        )

        with codecs.open(path, 'w', 'utf-8-sig') as f:
            f.write(content)
        print(f"Updated View: {path}")

# Now update Phase Controllers
ctrls = glob.glob('GMK360.Web/Controllers/Phase*Controller.cs')
for path in ctrls:
    if 'PhaseZero' in path: continue
    with codecs.open(path, 'r', 'utf-8-sig') as f:
        content = f.read()

    # Update Action Signature
    content = content.replace(
        'public async Task<IActionResult> UpdatePrice(int id, decimal totalCost, decimal quantity, string unit, string description)',
        'public async Task<IActionResult> UpdatePrice(int id, decimal totalCost, decimal quantity, string unit, string description, decimal? estimatedMaterialCost, decimal? estimatedLaborCost)'
    )
    
    # Add assignment inside UpdatePrice
    content = content.replace(
        'item.QuoteStatus = BudgetQuoteStatus.EstimatedOrQuoted;',
        'item.EstimatedMaterialCost = estimatedMaterialCost;\n            item.EstimatedLaborCost = estimatedLaborCost;\n            item.QuoteStatus = BudgetQuoteStatus.EstimatedOrQuoted;'
    )

    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)
    print(f"Updated Controller: {path}")
