import sqlite3

conn = sqlite3.connect('GMK360.Web/GMK360Db.sqlite')
cursor = conn.cursor()
cursor.execute('SELECT Id, Name, CoverImageUrl, CurrentStateImageUrl FROM ConstructionProjects WHERE Id=1')
row = cursor.fetchone()
print(row)
conn.close()
