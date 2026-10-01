import codecs

# Fix AgencyWorkersController.cs
path1 = 'GMK360.Web/Controllers/AgencyWorkersController.cs'
with codecs.open(path1, 'r', 'utf-8-sig') as f:
    content1 = f.read()

content1 = content1.replace('UserType = UserType.Candidate,', 'UserType = UserType.ServiceProvider')
content1 = content1.replace('InvitationStatus = 0', '')

with codecs.open(path1, 'w', 'utf-8-sig') as f:
    f.write(content1)

# Fix DailyTimesheetsController.cs
path2 = 'GMK360.Web/Controllers/DailyTimesheetsController.cs'
with codecs.open(path2, 'r', 'utf-8-sig') as f:
    content2 = f.read()

content2 = content2.replace('b.ProjectId == projectId.Value', 'b.ConstructionProjectId == projectId.Value')

with codecs.open(path2, 'w', 'utf-8-sig') as f:
    f.write(content2)

print('Fixed compilation errors!')