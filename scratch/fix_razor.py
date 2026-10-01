import io
import re

filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix RZ1010 error: Change "@{" to "{" or check if it's already in C# context.
# In razor: 
# @if(isDependent) { <i class="..."></i> } else { <span ...></span> }
# Let's just fix my injection from earlier:
# I used `@{` inside the razor block probably inappropriately?
# Actually, I injected `@{ var relatedQuote = ... }` right after `@foreach (var doc in group...) {`
# Ah! Since it's inside `@foreach (var doc in ...) {`, it's already in C# context!
# So `@{ ... }` is invalid. It should just be `var relatedQuote = ...` 
# Wait, NO, Razor allows `@{ }` inside `@foreach` IF it's rendering HTML. But let's just make it valid C#.

content = content.replace("                                        @{\n                                            var relatedQuote", "                                        \n                                            var relatedQuote")
content = content.replace("var hasQuote = relatedQuote != null;\n                                        }", "var hasQuote = relatedQuote != null;\n                                        ")

# Fix CS1501: (doc.DocumentFee.GetValueOrDefault() + doc.AdditionalCost.GetValueOrDefault()).ToString("N2")
# Wait, DocumentFee is probably double?. GetValueOrDefault() on double? returns double. double has ToString("N2").
# Let's just use string.Format("{0:N2}", ...) to be safe.
# Or `((decimal)(doc.DocumentFee ?? 0) + (decimal)(doc.AdditionalCost ?? 0)).ToString("N2")`

content = re.sub(r'@\(\(doc\.DocumentFee\.GetValueOrDefault\(\) \+ doc\.AdditionalCost\.GetValueOrDefault\(\)\)\.ToString\("N2"\)\)', 
                 r'@(((decimal)(doc.DocumentFee ?? 0) + (decimal)(doc.AdditionalCost ?? 0)).ToString("N2"))', content)


with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Fixed Razor syntax errors")
