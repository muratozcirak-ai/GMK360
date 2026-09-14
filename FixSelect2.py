import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Agenda\Create.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

script_addition = '''
        .ready(function() {
            if ($.fn.select2) {
                .select2.select2({
                    theme: 'bootstrap-5',
                    width: '100%'
                });
            }
        });

        let itemIndex = 1;'''

content = content.replace('let itemIndex = 1;', script_addition)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
