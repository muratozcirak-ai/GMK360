import sys

filepath = 'GMK360.Web/Controllers/AdminController.cs'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

target = """using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;"""

replacement = """using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;"""

if "using Microsoft.EntityFrameworkCore;" not in content:
    content = content.replace(target, replacement)
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
        print("AdminController using updated.")
else:
    print("AdminController already has the using.")
