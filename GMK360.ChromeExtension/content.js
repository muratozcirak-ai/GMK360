(async function() {
    // 1. İlan verilerini siteden topla (Aşağıdaki class/id'ler örnek sitelere göredir, hedef siteye göre Usta güncelleyecektir)
    
    // Güvenli metin okuma fonksiyonu
    const getText = (selector) => {
        let el = document.querySelector(selector);
        return el ? el.innerText.trim() : "";
    };

    let ilanVerisi = {
        Title: getText("h1") || document.title,
        PriceText: getText(".classified-price-wrapper") || getText(".price") || getText(".ilan-fiyati"),
        GrossAreaText: getText(".classifiedInfoList li:nth-child(4) span") || "0", // Örnek
        NetAreaText: getText(".classifiedInfoList li:nth-child(5) span") || "0",
        RoomCount: getText(".classifiedInfoList li:nth-child(6) span") || "",
        BuildingAge: getText(".classifiedInfoList li:nth-child(7) span") || "",
        FloorNumberText: getText(".classifiedInfoList li:nth-child(8) span") || "",
        Description: getText(".classifiedDescription") || getText(".ilan-aciklamasi") || "",
        Url: window.location.href,
        Platform: window.location.hostname
    };

    console.log("Çekilen Veri (Emlak Avcısı):", ilanVerisi);

    // 2. Lokal API'mize (GMK360) POST et
    // NOT: Lokal projeniz https mi http mi kullanıyor ve portu kaç? (Örn: https://localhost:7109)
    // Bunu C# projenizin Properties/launchSettings.json dosyasından kontrol edin.
    const apiUrl = "https://localhost:7109/api/ScraperApi/SaveProperty";

    try {
        let response = await fetch(apiUrl, {
            method: "POST",
            headers: { 
                "Content-Type": "application/json",
                "Accept": "application/json"
            },
            body: JSON.stringify(ilanVerisi)
        });

        if (response.ok) {
            let result = await response.json();
            console.log("GMK360 sistemine başarıyla iletildi!", result);
            alert("Emlak Avcısı: İlan başarıyla GMK360 sistemine çekildi!");
        } else {
            console.error("Sunucu hatası döndürdü:", response.status);
            alert("Emlak Avcısı: Sunucu hatası! (Kod: " + response.status + ")\nC# API'sinin çalıştığından emin olun.");
        }
    } catch (error) {
        console.error("Gönderim Hatası:", error);
        alert("Emlak Avcısı: Gönderim hatası!\n1. C# projenizin açık ve localhost:7109'da çalıştığından emin olun.\n2. CORS ayarlarının yapıldığını kontrol edin.");
    }
})();
