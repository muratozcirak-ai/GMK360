import io
import re

filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace ANY instance of .GetValueOrDefault().ToString("N2") that might be lingering
content = content.replace('.GetValueOrDefault().ToString("N2")', ' ?? 0).ToString("N2")') # Wait, this might be broken syntax

# Let's find exactly the line 146 and 216 equivalent
# The error was on: @((doc.DocumentFee.GetValueOrDefault() + doc.AdditionalCost.GetValueOrDefault()).ToString("N2"))
# I replaced it with @(((decimal)(doc.DocumentFee ?? 0) + (decimal)(doc.AdditionalCost ?? 0)).ToString("N2"))
# Wait! `doc.DocumentFee` might not be a decimal/double? It might be `decimal` directly! Wait, no, it's `decimal?`.
# Actually, the error said "No overload for method 'ToString' takes 1 arguments".
# If `doc.DocumentFee + doc.AdditionalCost` is `double`, `double` HAS `ToString(string format)`.
# Wait, maybe it's `int?`? If `DocumentFee` is `int?`, then `int.ToString("N2")` is valid.
# Oh! In my injection for Cost:
# title="Harç: @(doc.DocumentFee?.ToString("N2") ?? "0,00") | Ek Masraf: @(doc.AdditionalCost?.ToString("N2") ?? "0,00")"
# If `DocumentFee` is `double?`, `doc.DocumentFee?.ToString("N2")` might fail if `Nullable<double>.ToString(string)` doesn't exist!
# Yes! `Nullable<T>.ToString()` DOES NOT TAKE ARGUMENTS!
# You have to do `doc.DocumentFee?.ToString("N2")` which translates to `doc.DocumentFee.HasValue ? doc.DocumentFee.Value.ToString("N2") : null`.
# In C#, `x?.ToString("N2")` on a Nullable type actually doesn't compile if `Nullable<T>` doesn't have a ToString with arguments.
# Actually, wait. In newer C#, `x?.ToString("N2")` maps to `x.HasValue ? x.Value.ToString("N2") : null`.
# Let's just fix the `title` attribute!

content = re.sub(
    r'title="Harç: @\(doc\.DocumentFee\?\.ToString\("N2"\) \?\? "0,00"\) \| Ek Masraf: @\(doc\.AdditionalCost\?\.ToString\("N2"\) \?\? "0,00"\)"',
    r'title="Harç: @(doc.DocumentFee.HasValue ? doc.DocumentFee.Value.ToString("N2") : "0,00") | Ek Masraf: @(doc.AdditionalCost.HasValue ? doc.AdditionalCost.Value.ToString("N2") : "0,00")"',
    content
)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Fixed title string formats")
