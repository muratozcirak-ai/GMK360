with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

start_idx = 27478
end_idx = 46302

print(content[start_idx:start_idx+1000])
