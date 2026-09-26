with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'rb') as f:
    text = f.read()

if b'D\xef\xbf\xbdkkan' in text:
    print("CORRUPTED")
else:
    print("NOT CORRUPTED")
