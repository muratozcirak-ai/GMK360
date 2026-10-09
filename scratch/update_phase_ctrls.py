import codecs
import re
import glob

# 1. Update Phase Controllers
ctrls = glob.glob('GMK360.Web/Controllers/Phase*Controller.cs')
for path in ctrls:
    if 'PhaseZero' in path: continue
    with codecs.open(path, 'r', 'utf-8-sig') as f:
        content = f.read()

    if 'ViewData["BypassedPhases"]' not in content:
        content = content.replace(
            'ViewData["ProjectName"] = project.Name;',
            'ViewData["ProjectName"] = project.Name;\n            ViewData["BypassedPhases"] = project.BypassedPhases;'
        )
        with codecs.open(path, 'w', 'utf-8-sig') as f:
            f.write(content)
        print(f"Updated {path}")