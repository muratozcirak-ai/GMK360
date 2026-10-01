import io
import re

filepath = r'GMK360.Data\Contexts\ApplicationDbContext.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

db_sets = """
        public DbSet<GMK360.Core.Entities.Logistics.VehicleTask> VehicleTasks { get; set; }
        public DbSet<GMK360.Core.Entities.Logistics.VehicleExpense> VehicleExpenses { get; set; }
"""

# Insert after VehicleAssignments
content = re.sub(r'public DbSet<GMK360.Core.Entities.Logistics.VehicleAssignment> VehicleAssignments \{ get; set; \}', r'public DbSet<GMK360.Core.Entities.Logistics.VehicleAssignment> VehicleAssignments { get; set; }' + db_sets, content)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Injected DbSets into ApplicationDbContext")
