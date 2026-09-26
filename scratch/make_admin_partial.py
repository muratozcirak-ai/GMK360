import sys

filepath = 'GMK360.Web/Controllers/AdminController.cs'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

if "public class AdminController : Controller" in content:
    content = content.replace("public class AdminController : Controller", "public partial class AdminController : Controller")
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
