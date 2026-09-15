import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\_ProjectCardPartial.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

old_footer = '''    <div class="card-footer bg-white border-top-0 p-3 pt-0 text-center">
        <a href="/ConstructionProject/Details/@Model.Id" class="btn btn-outline-primary btn-sm rounded-pill w-100 fw-bold">Şantiye Panosuna Git <i class="bi bi-arrow-right"></i></a>
    </div>'''

new_footer = '''    <div class="card-footer bg-white border-top-0 p-3 pt-0 text-center">
        @if(Model.Status == ProjectStatus.Tamamlandi_Teslim)
        {
            <button class="btn btn-warning btn-sm rounded-pill w-100 fw-bold mb-2 shadow-sm text-dark" onclick="event.stopPropagation(); alert('Bu binanın (Dijital İkiz) davet linki yöneticiye SMS olarak gönderilecek. Bina yönetimi modülü aktif olduğunda devreye girecektir.');"><i class="bi bi-person-plus-fill me-1"></i> 👑 Bina Yöneticisini Davet Et</button>
            <a href="/ConstructionProject/Details/@Model.Id" class="btn btn-outline-secondary btn-sm rounded-pill w-100 fw-bold">Geçmiş Şantiye Arşivi <i class="bi bi-archive"></i></a>
        }
        else
        {
            <a href="/ConstructionProject/Details/@Model.Id" class="btn btn-outline-primary btn-sm rounded-pill w-100 fw-bold">Şantiye Panosuna Git <i class="bi bi-arrow-right"></i></a>
        }
    </div>'''

content = content.replace(old_footer, new_footer)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
