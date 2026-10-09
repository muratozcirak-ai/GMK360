import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Replace the current onclick
target_pattern = re.compile(r'onclick="alert\(\'Tevhit[^"]+"')
match = target_pattern.search(content)

if match:
    js_code = '''onclick="
        alert('Tevhit işlemi başarıyla tamamlandı. Seçilen bloklar tek bir hedef bina altında birleştirildi.'); 
        var m = bootstrap.Modal.getInstance(document.getElementById('mergeBlocksModal')); 
        if(m) m.hide(); 
        
        var hedefSutun = document.querySelector('h6.text-primary.border-bottom');
        if(hedefSutun) {
            var newCards = hedefSutun.parentElement.querySelectorAll('.card');
            if(newCards.length >= 2) {
                newCards[1].parentElement.style.display = 'none';
                
                var firstCard = newCards[0];
                firstCard.querySelector('h5').innerText = 'Beyaz Konak (Tevhit Edilmiş Yeni Proje)';
                var doorIcon = firstCard.querySelector('.bi-door-open');
                if(doorIcon) {
                    doorIcon.parentElement.innerHTML = '<i class=\\'bi bi-door-open me-1\\'></i>80 Daire, 4 Dükkan';
                }
                
                firstCard.style.transition = 'all 0.5s';
                firstCard.style.backgroundColor = '#e8f0fe';
                firstCard.style.borderWidth = '3px';
                setTimeout(function() {
                    firstCard.style.backgroundColor = '';
                    firstCard.style.borderWidth = '';
                }, 2000);
            }
        }
    "'''
    js_code = js_code.replace('\n', ' ')
    content = content[:match.start()] + js_code + content[match.end():]
    
with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("Demo JS updated!")
