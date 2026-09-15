import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Models\CreateProjectWizardViewModel.cs'
with codecs.open(filepath, 'r', 'utf-8') as f:
    content = f.read()

content = content.replace('public double? Latitude { get; set; }', 'public string Latitude { get; set; }')
content = content.replace('public double? Longitude { get; set; }', 'public string Longitude { get; set; }')
content = content.replace('Model.Latitude.HasValue', '!string.IsNullOrEmpty(Model.Latitude)')
content = content.replace('Model.Longitude.HasValue', '!string.IsNullOrEmpty(Model.Longitude)')

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
