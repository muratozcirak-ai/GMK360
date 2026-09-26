with open('GMK360.Web/Views/AdminLegalDocument/Index.cshtml', 'rb') as f:
    data = f.read()
    
# Strip all BOMs
while data.startswith(b'\xef\xbb\xbf'):
    data = data[3:]
    
with open('GMK360.Web/Views/AdminLegalDocument/Index.cshtml', 'wb') as f:
    f.write(b'\xef\xbb\xbf' + data)
