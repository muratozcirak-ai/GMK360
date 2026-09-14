import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Agenda\Create.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

participants_html = '''
            <div class="mb-3">
                <label class="form-label">Katılımcılar (Çoklu Seçim)</label>
                <select name="selectedParticipants" class="form-select" asp-items="ViewBag.ParticipantsList" multiple>
                </select>
                <small class="text-muted">Ctrl (veya Cmd) tuşuna basılı tutarak birden fazla kişi seçebilirsiniz. Sistem kayıtlı olanlara giriş paneli linki, misafirlere özel token linki üretecektir.</small>
            </div>
'''
content = content.replace('<!-- Right Column (Meta Data) -->', '<!-- Right Column (Meta Data) -->\r\n' + participants_html)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
