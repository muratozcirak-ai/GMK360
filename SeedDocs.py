import pyodbc

conn_str = r'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\mssqllocaldb;Database=GMK360Db;Trusted_Connection=yes;'
conn = pyodbc.connect(conn_str, autocommit=True)
cursor = conn.cursor()

docs = [
    ('Arsa Tapusu', 'Tapu Müdürlüğü', 'Construction', 1),
    ('İmar Durumu Belgesi', 'Belediye', 'Construction', 1),
    ('Mimari Proje Onayı', 'Belediye İmar Müdürlüğü', 'Construction', 1),
    ('Zemin Etüdü Raporu', 'Özel Zemin/Jeoloji Firması', 'Construction', 1),
    ('Yapı Ruhsatı', 'Belediye', 'Construction', 1)
]

cursor.execute('SELECT COUNT(*) FROM SystemLegalDocumentTemplates')
if cursor.fetchone()[0] == 0:
    for name, issued, target, req in docs:
        cursor.execute('''
            INSERT INTO SystemLegalDocumentTemplates (Name, IssuedBy, TargetModule, IsMandatory, CreatedAt, IsDeleted) 
            VALUES (?, ?, ?, ?, GETUTCDATE(), 0)
        ''', (name, issued, target, req))
    print('Evraklar eklendi.')
else:
    print('Evraklar zaten var.')
