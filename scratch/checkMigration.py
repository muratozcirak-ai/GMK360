with open(r'GMK360.Data\Migrations\20260914083841_InitialCreate.cs', 'rb') as f:
    text = f.read(5000).decode('utf-8')
    print('T?rkiye' in text)
    print('Türkiye' in text)
