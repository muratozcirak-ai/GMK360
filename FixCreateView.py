import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Agenda\Create.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

participants_html = '''
                                <div class="mb-3 mt-4">
                                    <h6 class="fw-bold mb-3 border-bottom pb-2"><i class="fas fa-users text-secondary me-2"></i> Ek Katılımcılar (Çoklu Seçim)</h6>
                                    <select name="selectedParticipants" class="form-select select2" asp-items="ViewBag.ParticipantsList" multiple="multiple" data-placeholder="Toplantıya katılacak diğer kişileri seçin...">
                                    </select>
                                    <small class="text-muted mt-1 d-block"><i class="fas fa-info-circle"></i> Ctrl (veya Cmd) tuşuna basılı tutarak birden fazla kişi seçebilirsiniz. Sistem katılımcılara özel izleme linki (misafir token veya gölge kullanıcı) üretecektir.</small>
                                </div>
'''

content = content.replace('<!-- Sağ Kolon: Bağlantılar ve Medya -->', '<!-- Sağ Kolon: Bağlantılar ve Medya -->\r\n' + participants_html)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
