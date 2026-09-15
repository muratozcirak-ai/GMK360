import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs'
with codecs.open(filepath, 'r', 'utf-8') as f:
    content = f.read()

# Replace assignments in SaveStep1
# project.Latitude = model.Latitude;
# project.Longitude = model.Longitude;

lat_logic = '''if (!string.IsNullOrEmpty(model.Latitude)) {
                    if (double.TryParse(model.Latitude.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lat)) {
                        project.Latitude = lat;
                    }
                }
                if (!string.IsNullOrEmpty(model.Longitude)) {
                    if (double.TryParse(model.Longitude.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lng)) {
                        project.Longitude = lng;
                    }
                }'''

content = content.replace('project.Latitude = model.Latitude;', lat_logic)
content = content.replace('project.Longitude = model.Longitude;', '')

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
