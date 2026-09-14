import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Agenda\Create.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

broken_script = '''
        .ready(function() {
            if ($.fn.select2) {
                .select2.select2({
                    theme: 'bootstrap-5',
                    width: '100%'
                });
            }
        });

        let itemIndex = 1;'''

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

        let itemIndex = 1;'''

content = content.replace(broken_script, new_script)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
