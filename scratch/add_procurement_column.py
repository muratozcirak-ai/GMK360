# -*- coding: utf-8 -*-
import pyodbc

conn_str = (
    r"Driver={ODBC Driver 17 for SQL Server};"
    r"Server=(localdb)\MSSQLLocalDB;"
    r"Database=GMK360Db;"
    r"Trusted_Connection=yes;"
)

conn = pyodbc.connect(conn_str, autocommit=True)
cursor = conn.cursor()

try:
    cursor.execute("ALTER TABLE ConstructionBudgetItems ADD ProcurementStrategy INT NOT NULL DEFAULT 0")
    print("Added ProcurementStrategy column to ConstructionBudgetItems")
except Exception as e:
    print("Column might already exist or error: ", e)