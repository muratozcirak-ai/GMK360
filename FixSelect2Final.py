import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Agenda\Create.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

new_script = '''
        $(document).ready(function() {
            if ($.fn.select2) {
                $('.select2').select2({
                    theme: 'bootstrap-5',
                    width: '100%',
                    placeholder: 'Seçim yapınız...'
                });
            }
        });

        let itemIndex = 0;'''

if 'theme: \'bootstrap-5\'' not in content:
    content = content.replace('let itemIndex = 0;', new_script)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
