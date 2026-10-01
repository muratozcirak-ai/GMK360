import codecs

path = 'GMK360.Data/Contexts/ApplicationDbContext.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = 'public DbSet<GMK360.Core.Entities.Construction.ProjectManagementInvitation> ProjectManagementInvitations { get; set; }'
replacement = target + '\n        public DbSet<GMK360.Core.Entities.B2b.B2BNetworkConnection> B2BNetworkConnections { get; set; }'

if 'B2BNetworkConnections' not in content:
    content = content.replace(target, replacement)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Fixed B2BNetworkConnection!')