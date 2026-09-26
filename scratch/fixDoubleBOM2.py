files = ['GMK360.Web/Views/ConstructionProject/Details.cshtml', 'GMK360.Web/Views/ConstructionProject/Amenities.cshtml']
for file_path in files:
    with open(file_path, 'rb') as f:
        data = f.read()
    
    # Strip ALL BOMs
    while data.startswith(b'\xef\xbb\xbf'):
        data = data[3:]
        
    with open(file_path, 'wb') as f:
        f.write(b'\xef\xbb\xbf' + data)
