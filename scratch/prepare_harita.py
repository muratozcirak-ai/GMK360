import re

with open('harita.sql', 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

# Add GO before every INSERT INTO to break batches
content = re.sub(r'(?m)^INSERT INTO', 'GO\nINSERT INTO', content)

# But wait, we still need IDENTITY_INSERT logic!
# Since GO resets scope? No, SET IDENTITY_INSERT is session-scoped, not batch-scoped.
# So we can just put it at the very top for all tables?
# No, SQL Server ONLY allows one table to have IDENTITY_INSERT ON at a time in a session!
# So we must turn it OFF for Cities before turning it ON for Districts!

# Let's do this:
content = re.sub(r'GO\nINSERT INTO "Cities"', 'SET IDENTITY_INSERT [Cities] ON;\nGO\nINSERT INTO "Cities"', content, count=1)
content = re.sub(r'GO\nINSERT INTO "Districts"', 'SET IDENTITY_INSERT [Cities] OFF;\nSET IDENTITY_INSERT [Districts] ON;\nGO\nINSERT INTO "Districts"', content, count=1)
content = re.sub(r'GO\nINSERT INTO "Neighborhoods"', 'SET IDENTITY_INSERT [Districts] OFF;\nSET IDENTITY_INSERT [Neighborhoods] ON;\nGO\nINSERT INTO "Neighborhoods"', content, count=1)
content = re.sub(r'GO\nINSERT INTO "Streets"', 'SET IDENTITY_INSERT [Neighborhoods] OFF;\nSET IDENTITY_INSERT [Streets] ON;\nGO\nINSERT INTO "Streets"', content, count=1)

content += '\nGO\nSET IDENTITY_INSERT [Streets] OFF;\nGO\n'

header = '''SET QUOTED_IDENTIFIER ON;
GO
USE GMK360Db;
GO
ALTER TABLE [Streets] NOCHECK CONSTRAINT ALL;
ALTER TABLE [Neighborhoods] NOCHECK CONSTRAINT ALL;
ALTER TABLE [Districts] NOCHECK CONSTRAINT ALL;
ALTER TABLE [Cities] NOCHECK CONSTRAINT ALL;
GO
DELETE FROM [Streets];
DELETE FROM [Neighborhoods];
DELETE FROM [Districts];
DELETE FROM [Cities];
GO
'''

with open('harita_ready.sql', 'w', encoding='utf-8') as f:
    f.write(header + content)

print("harita_ready.sql fixed with GO!")
