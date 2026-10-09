import sqlite3
import os

db_path = "GMK360.Web/GMK360Db.sqlite"
if not os.path.exists(db_path):
    print("DB NOT FOUND")
    exit()

conn = sqlite3.connect(db_path)
cursor = conn.cursor()
cursor.execute("SELECT Id, ConstructionProjectId, BlockName, IsExistingBuilding, ParentBuildingId FROM Buildings")
rows = cursor.fetchall()
for r in rows:
    print(f"ID: {r[0]} | ProjeID: {r[1]} | Ad: {r[2]} | EskiMi: {r[3]} | Parent: {r[4]}")
conn.close()
