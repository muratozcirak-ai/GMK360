import os

# Set Layout for all Views in ProjectCrm, ProjectFinance, AgencyStaff, ConstructionProject etc.
views_dir = r'GMK360.Web\Views'
internal_folders = ['ProjectCrm', 'ProjectFinance', 'AgencyStaff', 'ConstructionProject', 'Dashboard', 'BuildingManager', 'FinancialStrategy', 'CustomerDashboard']

for folder in internal_folders:
    folder_path = os.path.join(views_dir, folder)
    if os.path.exists(folder_path):
        for file in os.listdir(folder_path):
            if file.endswith('.cshtml'):
                file_path = os.path.join(folder_path, file)
                with open(file_path, 'r', encoding='utf-8-sig', errors='ignore') as f:
                    content = f.read()
                
                # If there's no Layout defined, insert it inside @{ ... }
                if 'Layout =' not in content and '@{' in content:
                    content = content.replace('@{', '@{\n    Layout = "~/Views/Shared/_ConstructionLayout.cshtml";', 1)
                    with open(file_path, 'w', encoding='utf-8-sig') as f:
                        f.write(content)
