import codecs
import re
import glob

files = glob.glob('GMK360.Web/Views/Phase*/Index.cshtml')
for path in files:
    with codecs.open(path, 'r', 'utf-8-sig') as f:
        content = f.read()

    # Replace hardcoded var subCategories = new List<string> { ... }; with dynamic one
    target = r'var subCategories = new List<string>\s*\{[\s\S]*?\};'
    replacement = 'var subCategories = Model.Where(x => !string.IsNullOrEmpty(x.SubCategory)).Select(x => x.SubCategory).Distinct().OrderBy(c => c).ToList();'
    content = re.sub(target, replacement, content)

    # Some views might use var subCategories = new[] { ... };
    target2 = r'var subCategories = new\[\]\s*\{[\s\S]*?\};'
    content = re.sub(target2, replacement, content)

    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)