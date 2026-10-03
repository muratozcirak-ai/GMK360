import pyodbc
import codecs

with codecs.open('scratch/create_phase_templates.sql', 'r', 'utf-8-sig') as f:
    sql = f.read()

conn_str = (
    r"Driver={ODBC Driver 17 for SQL Server};"
    r"Server=(localdb)\MSSQLLocalDB;"
    r"Database=GMK360Db;"
    r"Trusted_Connection=yes;"
)

try:
    conn = pyodbc.connect(conn_str, autocommit=True)
    cursor = conn.cursor()
    # Execute commands separated by semicolons or just execute the whole block
    cursor.execute(sql)
    print("Table created and seeded successfully!")
    conn.close()
except pyodbc.Error as e:
    print(f"Error: {e}")