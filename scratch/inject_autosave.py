with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

new_js = """
        // ========================================================
        // CANLI AUTO-SAVE MOTORU (DEBOUNCE İLE)
        // ========================================================
        let autoSaveTimer = null;
        function autoSaveStep2() {
            clearTimeout(autoSaveTimer);
            autoSaveTimer = setTimeout(() => {
                var form = document.getElementById('wizardForm');
                var formData = new FormData(form);
                fetch('/ConstructionProject/SaveStep2', {
                    method: 'POST',
                    body: formData
                })
                .then(res => res.json())
                .then(data => {
                    if(data.success) {
                        console.log("Auto-save başarılı: 2. Aşama");
                    }
                }).catch(err => console.error("Auto-save hatası", err));
            }, 800); // 800ms bekler, peş peşe yazarken yormaz
        }

        // Tüm Step 2 girişlerini dinle (input, select)
        document.addEventListener('DOMContentLoaded', function() {
            var step2 = document.getElementById('step2');
            if(step2) {
                step2.addEventListener('input', function(e) {
                    if(e.target.tagName === 'INPUT' || e.target.tagName === 'SELECT' || e.target.tagName === 'TEXTAREA') {
                        autoSaveStep2();
                    }
                });
            }
        });
"""

if "function autoSaveStep2" not in content:
    content = content.replace('// IL/ILCE/MAHALLE/SOKAK DINAMIK YUKLEME', new_js + '\n\n        // IL/ILCE/MAHALLE/SOKAK DINAMIK YUKLEME')
    with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "w", encoding="utf-8") as f:
        f.write(content)
    print("AutoSave JS injected")
else:
    print("Already exists")
