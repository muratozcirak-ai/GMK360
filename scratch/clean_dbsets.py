import io
import re

filepath = r'GMK360.Data\Contexts\ApplicationDbContext.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = []
skip = False
for line in lines:
    if any(x in line for x in [
        'DbSet<ProjectPhase>',
        'DbSet<GMK360.Core.Entities.Construction.PhaseTask>',
        'DbSet<GMK360.Core.Entities.Construction.PhaseApproval>',
        'DbSet<GMK360.Core.Entities.Construction.PhaseMessage>',
        'DbSet<GMK360.Core.Entities.Construction.TaskCost>',
        'DbSet<GMK360.Core.Entities.Construction.PhaseWorkerDemand>',
        'DbSet<GMK360.Core.Entities.Construction.PhaseTaskTimesheet>'
    ]):
        continue
    
    # Let's also check for specific fluent API configurations
    # .HasOne(t => t.ProjectPhase) ...
    # This might be more complex. Let's just remove specific blocks manually or let compiler show errors, then fix.

    new_lines.append(line)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.writelines(new_lines)

print("Cleaned DbSets")
