import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Data\Contexts\ApplicationDbContext.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

if "DbSet<ConstructionProject>" not in content:
    dbsets = """
        // İnşaat Modülü (Construction & Timesheets)
        public DbSet<ConstructionProject> ConstructionProjects { get; set; }
        public DbSet<ProjectUnit> ProjectUnits { get; set; }
        public DbSet<Subcontractor> Subcontractors { get; set; }
        public DbSet<ProjectWorker> ProjectWorkers { get; set; }
        public DbSet<WorkerTimesheet> WorkerTimesheets { get; set; }
"""
    # Insert right after another DbSet
    content = re.sub(r'(public DbSet<[^>]+>\s+\w+\s*\{\s*get;\s*set;\s*\})', r'\1\n' + dbsets, content, count=1)
    
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Added DbSets successfully.")
else:
    print("DbSets already exist.")
