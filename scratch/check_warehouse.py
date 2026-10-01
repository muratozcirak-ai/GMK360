import codecs
path = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    if 'Warehouse' in f.read():
        print("Found Warehouse")
    else:
        print("Not Found")