import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Agenda\Index.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

script_to_add = '''
        // Canlı notların AJAX ile kaydedilmesi (Debounce ile)
        let typingTimer;
        const doneTypingInterval = 1000; // 1 saniye bekle

        $(document).on('input', '.live-note-input', function () {
            clearTimeout(typingTimer);
            let input = $(this);
            let itemId = input.data('item-id');
            let notes = input.val();
            let statusEl = $('#status-' + itemId);
            
            statusEl.hide();

            typingTimer = setTimeout(function () {
                $.post('/Agenda/SaveLiveNotes', { itemId: itemId, notes: notes })
                .done(function() {
                    statusEl.fadeIn().delay(2000).fadeOut();
                });
            }, doneTypingInterval);
        });
'''

content = content.replace('function loadDetails(id, element) {', script_to_add + '\r\n        function loadDetails(id, element) {')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
