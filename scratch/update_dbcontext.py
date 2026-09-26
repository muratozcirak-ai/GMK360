import sys
import re

filepath = 'GMK360.Data/Contexts/ApplicationDbContext.cs'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Check if already added
if "public DbSet<GMK360.Core.Entities.B2b.B2bCompany> B2bCompanies" not in content:
    target = "public DbSet<B2bSupplier> B2bSuppliers { get; set; }"
    replacement = """public DbSet<B2bSupplier> B2bSuppliers { get; set; }
        
        // B2B Marketplace (Yeni)
        public DbSet<GMK360.Core.Entities.B2b.B2bCompany> B2bCompanies { get; set; }
        public DbSet<GMK360.Core.Entities.B2b.B2bCategory> B2bCategories { get; set; }
        public DbSet<GMK360.Core.Entities.B2b.B2bCompanyCategory> B2bCompanyCategories { get; set; }
        public DbSet<GMK360.Core.Entities.B2b.B2bBranch> B2bBranches { get; set; }
        public DbSet<GMK360.Core.Entities.B2b.B2bContact> B2bContacts { get; set; }"""
        
    content = content.replace(target, replacement)
    
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
        print("ApplicationDbContext updated.")
else:
    print("Already updated.")
