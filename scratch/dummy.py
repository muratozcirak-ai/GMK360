import codecs
import re

path = 'GMK360.Web/Views/CompanyGarage/AllTasks.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Since we don't have ViewBag.Projects in AllTasks, I need to add it to the Controller's AllTasks method first.