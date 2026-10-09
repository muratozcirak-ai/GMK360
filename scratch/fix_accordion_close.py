import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

target = '''    </div>
    }
</div>



<!-- Custom Doc Modal at the bottom -->'''

replacement = '''    </div>
    }
</div>
            </div> <!-- accordion-body -->
        </div> <!-- accordion-collapse -->
    </div> <!-- accordion-item -->
</div> <!-- accordion -->



<!-- Custom Doc Modal at the bottom -->'''

content = content.replace(target, replacement)

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
print("done")
