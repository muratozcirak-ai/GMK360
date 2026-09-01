document.getElementById("btnCek").addEventListener("click", async () => {
    let btn = document.getElementById("btnCek");
    let mesaj = document.getElementById("mesaj");
    
    btn.disabled = true;
    btn.innerText = "İşleniyor...";
    mesaj.innerText = "";
    mesaj.className = "";

    try {
        let [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
        
        // Açık olan sayfada content.js dosyasını çalıştır
        chrome.scripting.executeScript({
            target: { tabId: tab.id },
            files: ['content.js']
        }, (results) => {
            if (chrome.runtime.lastError) {
                mesaj.innerText = "Hata: " + chrome.runtime.lastError.message;
                mesaj.className = "error";
                btn.disabled = false;
                btn.innerText = "Bu İlanı Sisteme Çek";
                return;
            }
            
            // content.js çalıştıktan sonra sonucunu alıyoruz (eğer dönerse)
            mesaj.innerText = "İşlem başlatıldı, arka planda gönderiliyor...";
            mesaj.className = "success";
            
            setTimeout(() => {
                window.close();
            }, 2000);
        });

    } catch (err) {
        mesaj.innerText = "Uzantı hatası: " + err.message;
        mesaj.className = "error";
        btn.disabled = false;
        btn.innerText = "Bu İlanı Sisteme Çek";
    }
});
