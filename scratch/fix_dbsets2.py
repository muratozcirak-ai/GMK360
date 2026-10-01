import codecs

path = 'GMK360.Data/Contexts/ApplicationDbContext.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

missing_dbsets = '''
        public DbSet<GMK360.Core.Entities.Construction.ConstructionProjectExpense> ConstructionProjectExpenses { get; set; }
        public DbSet<GMK360.Core.Entities.Construction.ProjectStakeholder> ProjectStakeholders { get; set; }
        public DbSet<GMK360.Core.Entities.Finance.ProjectCashRequest> ProjectCashRequests { get; set; }
        public DbSet<GMK360.Core.Entities.Logistics.CompanyVehicle> CompanyVehicles { get; set; }
        public DbSet<GMK360.Core.Entities.Logistics.VehicleAssignment> VehicleAssignments { get; set; }
        public DbSet<GMK360.Core.Entities.Logistics.VehicleTask> VehicleTasks { get; set; }
        public DbSet<GMK360.Core.Entities.Logistics.VehicleExpense> VehicleExpenses { get; set; }
        public DbSet<GMK360.Core.Entities.Construction.SiteDailyLog> SiteDailyLogs { get; set; }
        public DbSet<GMK360.Core.Entities.Construction.ProjectManagementInvitation> ProjectManagementInvitations { get; set; }
        public DbSet<GMK360.Core.Entities.B2B.B2BNetworkConnection> B2BNetworkConnections { get; set; }
'''

target = 'public DbSet<ConstructionProject> ConstructionProjects { get; set; }'

if 'ProjectStakeholders' not in content:
    content = content.replace(target, target + '\n' + missing_dbsets.strip())
    
with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Restored missing DbSets reliably!')