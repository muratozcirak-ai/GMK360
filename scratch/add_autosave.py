import re

with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'r', encoding='utf-8') as f:
    content = f.read()

# Add auto-save script at the end of the script block
auto_save_script = '''
        // --- AUTO-SAVE MANTIĞI (AŞAMA 2) ---
        let autoSaveTimeout;
        function autoSaveStep2() {
            clearTimeout(autoSaveTimeout);
            autoSaveTimeout = setTimeout(() => {
                const form = document.getElementById('wizardForm');
                const formData = new FormData(form);
                fetch('/ConstructionProject/SaveStep2', {
                    method: 'POST',
                    body: formData
                })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        console.log("Aşama 2 otomatik kaydedildi.");
                        // Küçük bir "Kaydedildi" ibaresi gösterebiliriz
                    }
                })
                .catch(err => console.error("AutoSave Error: ", err));
            }, 1000); // 1 saniye bekleme (Debounce)
        }

        // Adım 2'deki herhangi bir input değiştiğinde Auto-Save tetikle
        document.getElementById('step2').addEventListener('input', function(e) {
            // Sadece form elemanları ise tetikle
            if(e.target.tagName === 'INPUT' || e.target.tagName === 'SELECT' || e.target.tagName === 'TEXTAREA') {
                autoSaveStep2();
            }
        });
        document.getElementById('step2').addEventListener('change', function(e) {
            if(e.target.tagName === 'INPUT' || e.target.tagName === 'SELECT' || e.target.tagName === 'TEXTAREA') {
                autoSaveStep2();
            }
        });
'''

# Find the end of script
end_script_idx = content.rfind('</script>')
if end_script_idx != -1:
    content = content[:end_script_idx] + auto_save_script + content[end_script_idx:]

# Also trigger autoSaveStep2 immediately after generateOldBlocks and generateNewBlocks finish!
# So when user changes structure count, it saves instantly.
content = content.replace('container.insertAdjacentHTML(\'beforeend\', html);\n            }\n        }\n    }', 'container.insertAdjacentHTML(\'beforeend\', html);\n            }\n        }\n        autoSaveStep2();\n    }')

with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'w', encoding='utf-8') as f:
    f.write(content)
print("Auto-save added successfully!")
