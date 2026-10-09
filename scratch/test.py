with open("GMK360.Web/Views/ConstructionProject/Details.cshtml", "r", encoding="utf-8") as f:
    lines = f.readlines()

for i in range(max(0, 1003-10), min(len(lines), 1003+35)):
    print(f"{i+1}: {lines[i].rstrip()}")
