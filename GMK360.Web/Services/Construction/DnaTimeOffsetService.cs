using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Construction.Workflows;
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Services.Construction
{
    /// <summary>
    /// Otonom Şantiye Tetikleyici Görev Modeli (T-, T, T+)
    /// </summary>
    public class DnaTriggerTask
    {
        public int Id { get; set; }
        public int OffsetDays { get; set; } // Örn: -7, -2, -1, 0, 1, 7, 28
        public DateTime TargetDate { get; set; }
        public string OffsetLabel => OffsetDays < 0 ? $"T{OffsetDays} Gün" : (OffsetDays == 0 ? "T Günü (Uygulama)" : $"T+{OffsetDays} Gün");
        
        public string Role { get; set; } // Örn: "Şantiye Şefi", "Satın Alma", "Laboratuvar / Denetim", "İSG Uzmanı", "Taşeron"
        public string RoleBadgeClass { get; set; } = "bg-primary";
        
        public string Category { get; set; } // "Ön Hazırlık (T-)", "Ana İmalat & İSG (T 0)", "Teknik Takip & Kür (T+)", "Yasal Kabul & Rapor (T+)"
        public string CategoryBadgeClass { get; set; } = "bg-secondary";
        
        public string Title { get; set; }
        public string Description { get; set; }
        public string CriticalWarning { get; set; } // Sahadaki hayati teknik kural
        
        public string SourcePhase { get; set; } // Örn: "Adım 0", "Adım 2", "Adım 4"
        public string SourceItem { get; set; }  // Örn: "Radye Temel ve C30 Beton", "Lamine Parke"
        public string ProfessionName { get; set; } // Örn: "Betoncu", "Parkeci", "İzolasyoncu"
        
        public List<string> CheckItems { get; set; } = new List<string>();
        public bool IsCompleted { get; set; } = false;
    }

    /// <summary>
    /// GMK360 Otonom Şantiye Motoru: DNA kararlarını (sıfır metraj) sahanın altın T- / T+ kurallarına dönüştüren motor.
    /// </summary>
    public class DnaTimeOffsetService
    {
        private readonly ApplicationDbContext _context;

        public DnaTimeOffsetService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Seçilen DNA Profiline ve Şefin belirlediği ana başlangıç tarihine (T Günü) göre
        /// şantiye ajandasının tüm görevlerini kronolojik olarak türetir.
        /// </summary>
        public List<DnaTriggerTask> GenerateTasksForProfile(ProjectDnaProfile profile, DateTime baseDate)
        {
            var tasks = new List<DnaTriggerTask>();
            if (profile == null) return tasks;

            var details = profile.Details != null && profile.Details.Any()
                ? profile.Details.ToList()
                : _context.ProjectDnaDetails.Where(d => d.ProfileId == profile.Id).ToList();

            int taskIdSeq = 1;

            // 1. BETONARME, STATİK VE TAŞIYICI İMALATLAR (Radye Temel, Kolon, Perde, C25/30)
            bool hasConcrete = details.Any(d => IsMatch(d.SelectedItem, "beton", "c25", "c30", "c35", "radye", "statik", "kalıp", "demir"))
                               || profile.Name.ToLower().Contains("beton") || profile.Name.ToLower().Contains("kaba");

            if (hasConcrete || !details.Any()) // Fallback: Eğer hiç detay yoksa standart temel/kaba kuralını işlet
            {
                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = -2,
                    TargetDate = baseDate.AddDays(-2),
                    Role = "Satın Alma & Şef",
                    RoleBadgeClass = "bg-warning text-dark",
                    Category = "Ön Hazırlık (T-)",
                    CategoryBadgeClass = "bg-warning text-dark",
                    Title = "Beton Santrali & Pompa Rezervasyonu",
                    Description = "Santral aranarak döküm başlama saati, mikser sıklığı (15-20 dk aralık) ve uygun bom uzunluğuna sahip pompa tipi teyit edilmelidir.",
                    CriticalWarning = "Rezervasyon son güne bırakılırsa transmikserler gecikir, soğuk derz (katman ayrışması) riski doğar!",
                    SourcePhase = "Adım 0: Statik & Taşıyıcılar",
                    SourceItem = "Betonarme & Taşıyıcı İskelet",
                    ProfessionName = "Beton Santrali & Pompa",
                    CheckItems = new List<string> { "Santral ile m³ ve saat teyidi yapıldı", "Yedek transmikser hattı planlandı", "Pompa kurulum alanı boşaltıldı" }
                });

                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = -1,
                    TargetDate = baseDate.AddDays(-1),
                    Role = "Şantiye Şefi & Formen",
                    RoleBadgeClass = "bg-primary",
                    Category = "Kritik Kontrol (T-)",
                    CategoryBadgeClass = "bg-danger",
                    Title = "Kalıp Yağı, Paspayı & Donatı Ön Kabulü",
                    Description = "Kalıp yüzeyine kalıp ayırıcı yağ sürüldü mü? Donatı paspayı takozları eksiksiz mi? Kalıp içi talaş ve moloz temizlendi mi? Vibratörler test edildi mi?",
                    CriticalWarning = "Paspayı olmayan donatı dış hava ve nemle temas ederek korozyona uğrar ve taşıyıcı ömrünü yarıya indirir!",
                    SourcePhase = "Adım 0: Statik & Taşıyıcılar",
                    SourceItem = "Kalıp & Donatı",
                    ProfessionName = "Kalıpçı & Demirci",
                    CheckItems = new List<string> { "Donatı paspayları kontrol edildi (min 3-5 cm)", "Kalıp temizliği ve kalıp yağı onaylandı", "Vibratör ve yedek benzinli vibratör çalışır durumda" }
                });

                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = 0,
                    TargetDate = baseDate,
                    Role = "Kalite / Yapı Denetim",
                    RoleBadgeClass = "bg-danger",
                    Category = "Ana İmalat & İSG (T 0)",
                    CategoryBadgeClass = "bg-danger",
                    Title = "Slump (Çökme) Deneyi & Silindir Numune Alımı",
                    Description = "Gelen mikserlerin irsaliyesi ve su eklenip eklenmediği denetlenir. Slump testi yapılarak kıvam ölçülür. 7 ve 28 günlük basınç kırımları için 6 adet silindir/küp numune alınır ve etiketlenir.",
                    CriticalWarning = "Şantiyede transmiksere fazladan su ekletmek betonun mukavemet sınıfını C30'dan C16 seviyesine düşürür! Kesinlikle izin verilmemelidir.",
                    SourcePhase = "Adım 0: Statik & Taşıyıcılar",
                    SourceItem = "Hazır Beton Dökümü",
                    ProfessionName = "Laboratuvar & Yapı Denetim",
                    CheckItems = new List<string> { "İrsaliye üzerindeki beton sınıfı kontrol edildi", "Slump testi yapıldı (hedef: S3/S4)", "6 adet numune alınıp barkodlandı" }
                });

                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = 1,
                    TargetDate = baseDate.AddDays(1),
                    Role = "Şantiye Şefi & Saha Ekibi",
                    RoleBadgeClass = "bg-info text-dark",
                    Category = "Kürleme & Bakım (T+)",
                    CategoryBadgeClass = "bg-info text-dark",
                    Title = "Beton Kürü ve Düzenli Sulama (En az 7 Gün)",
                    Description = "Beton dökümünden sonraki ilk 24-72 saat hidrasyon ısısı nedeniyle kritik buharlaşma dönemidir. Sabah, öğle ve akşam günde en az 3 kez bol suyla sulanmalı veya kür membranı sıkılmalıdır.",
                    CriticalWarning = "Kürlenmeyen beton yüzeyinde kılcal rötre çatlakları oluşur, su sızdırmazlığı kaybolur!",
                    SourcePhase = "Adım 0: Statik & Taşıyıcılar",
                    SourceItem = "Beton Bakımı & Hidrasyon",
                    ProfessionName = "Şantiye Saha Ekibi",
                    CheckItems = new List<string> { "Sabah sulaması yapıldı", "Öğle buharlaşma kontrolü yapıldı", "Gece ıslaklığı korundu" }
                });

                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = 7,
                    TargetDate = baseDate.AddDays(7),
                    Role = "Laboratuvar & Statik Şef",
                    RoleBadgeClass = "bg-primary",
                    Category = "Teknik Denetim (T+)",
                    CategoryBadgeClass = "bg-primary",
                    Title = "7 Günlük Erken Dayanım Kırma Testi",
                    Description = "Alınan numunelerin ilk serisi laboratuvar presinde kırılır. Hedef dayanımın en az %65-70'ine ulaşıldığı teyit edilerek döşeme ve perde kalıp sökümüne yeşil ışık yakılır.",
                    CriticalWarning = "7 günlük sonuç gelmeden ana döşeme taşıyıcı dikmeleri sökülemez!",
                    SourcePhase = "Adım 0: Statik & Taşıyıcılar",
                    SourceItem = "Kırma Testi & Kalıp Söküm",
                    ProfessionName = "Yapı Denetim Laboratuvarı",
                    CheckItems = new List<string> { "Pres kırma sonucu sisteme yüklendi", "Kalıp söküm izni şantiye defterine işlendi" }
                });

                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = 28,
                    TargetDate = baseDate.AddDays(28),
                    Role = "Yapı Denetim & Statik Mühendis",
                    RoleBadgeClass = "bg-success",
                    Category = "Nihai Yasal Kabul (T+)",
                    CategoryBadgeClass = "bg-success",
                    Title = "28 Günlük Karakteristik Mukavemet Kesin Raporu",
                    Description = "Betonun yasal tasarım dayanımını (Örn: C30 için 30 MPa) kanıtlayan 28. gün kırma sonuçları onaylanır ve belediye yapı denetim dosyasına resmen işlenir.",
                    CriticalWarning = "28 günlük kırım sonucu barajı geçmezse karot testi zorunlu hale gelir!",
                    SourcePhase = "Adım 0: Statik & Taşıyıcılar",
                    SourceItem = "Yasal Statik Kabul",
                    ProfessionName = "Yetkili Yapı Denetim Firması",
                    CheckItems = new List<string> { "28 günlük kesin rapor onaylandı", "Dosya kapatma onayı verildi" }
                });
            }

            // 2. PARKE & ZEMİN İMALATLARI (Lamine Parke, Laminat Parke)
            bool hasParquet = details.Any(d => IsMatch(d.SelectedItem, "parke", "lamine", "laminat"));
            if (hasParquet)
            {
                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = -7,
                    TargetDate = baseDate.AddDays(-7),
                    Role = "Satın Alma & Lojistik",
                    RoleBadgeClass = "bg-warning text-dark",
                    Category = "Ön Hazırlık (T-)",
                    CategoryBadgeClass = "bg-warning text-dark",
                    Title = "Parke Malzeme Teslimatı & Ortam Aklimatizasyonu",
                    Description = "Parke paketleri montaj yapılacak dairelere taşınmalı ve palet bantları gevşetilerek mekanın sıcaklık ve bağıl nemine alışması için en az 48-72 saat bekletilmelidir.",
                    CriticalWarning = "Kamyondan indirildiği gün döşenen parke, ortam nemini emerek genleşir, bel verir ve kabarır!",
                    SourcePhase = "Adım 4: İç Mekan",
                    SourceItem = "Lamine / Laminat Parke",
                    ProfessionName = "Parke Tedarikçisi",
                    CheckItems = new List<string> { "Paketler dairelere dağıtıldı", "Ortam sıcaklığı min 18°C olarak sağlandı", "Pencereler kapalı tutuldu" }
                });

                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = -3,
                    TargetDate = baseDate.AddDays(-3),
                    Role = "Şantiye Şefi",
                    RoleBadgeClass = "bg-primary",
                    Category = "Kritik Kontrol (T-)",
                    CategoryBadgeClass = "bg-danger",
                    Title = "Şap Nem Ölçümü (CM Cihazı ile Nem Testi)",
                    Description = "Kalsiyum Karbür (CM) cihazı ile şapın derinlik nemi ölçülür. Çimento esaslı şapta nem %2'nin altında (yerden ısıtmada %1.8) olmalıdır.",
                    CriticalWarning = "Nem %2'nin üzerindeyse parkeci sahaya KESİNLİKLE sokulmaz! Aksi halde parke altı küflenir ve kararır.",
                    SourcePhase = "Adım 4: İç Mekan",
                    SourceItem = "Şap Nem Kabulü",
                    ProfessionName = "Şantiye Şefi & Parkeci",
                    CheckItems = new List<string> { "3 farklı noktadan CM nem numunesi alındı", "Nem değeri %2'nin altında çıktı", "Usta kabul tutanağı imzalandı" }
                });

                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = -1,
                    TargetDate = baseDate.AddDays(-1),
                    Role = "Formen & Parkeci",
                    RoleBadgeClass = "bg-info text-dark",
                    Category = "Ön Hazırlık (T-)",
                    CategoryBadgeClass = "bg-info text-dark",
                    Title = "Zemin Süpürme, Pürüz Giderme & Şilte Serimi",
                    Description = "Şap yüzeyindeki harç kabarcıkları raspa ile kazınır, endüstriyel vakumla tozsuzlaştırılır ve ses/nem yalıtım şiltesi (kapron) serilir.",
                    CriticalWarning = "Toz ve çapak temizlenmezse parkeye basıldığında çıtırtı ve gıcırtı sesleri yapar.",
                    SourcePhase = "Adım 4: İç Mekan",
                    SourceItem = "Zemin Hazırlığı",
                    ProfessionName = "Parke Ekibi",
                    CheckItems = new List<string> { "Yüzey raspa ile düzeltildi", "Tozsuzlaştırma yapıldı", "Şilte ek yerleri bantlandı" }
                });

                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = 0,
                    TargetDate = baseDate,
                    Role = "👷 Parkeci Ustası",
                    RoleBadgeClass = "bg-success",
                    Category = "Ana İmalat (T 0)",
                    CategoryBadgeClass = "bg-success",
                    Title = "Parke Montajı (12-15 mm Duvar Genleşme Boşluğu)",
                    Description = "Parke montajına ışık kaynağına (pencereye) dik yönde başlanır. Duvar diplerinde takoz yardımıyla min 12-15 mm genleşme boşluğu bırakılır.",
                    CriticalWarning = "Duvar dibine sıfır dayanan parke, mevsimsel genleşmede yukarı fırlar!",
                    SourcePhase = "Adım 4: İç Mekan",
                    SourceItem = "Parke Uygulaması",
                    ProfessionName = "Parke Ustası",
                    CheckItems = new List<string> { "Genleşme takozları yerleştirildi", "Kilit mekanizmaları tam oturtuldu" }
                });

                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = 2,
                    TargetDate = baseDate.AddDays(2),
                    Role = "Şantiye Şefi & Formen",
                    RoleBadgeClass = "bg-primary",
                    Category = "Teknik Kontrol (T+)",
                    CategoryBadgeClass = "bg-primary",
                    Title = "Süpürgelik, Kapı Altı Boşluğu & Kot Kontrolü",
                    Description = "İç kapı kasalarının ve kanatlarının parkeye sürtme mesafesi (min 6-8 mm) ve süpürgeliklerin duvar gönyesi denetlenir.",
                    CriticalWarning = "Süpürgelik parkeye değil, daima duvara çakılmalıdır; aksi halde parke hareket edemez!",
                    SourcePhase = "Adım 4: İç Mekan",
                    SourceItem = "Süpürgelik & Kapı Kotu",
                    ProfessionName = "Parkeci & Marangoz",
                    CheckItems = new List<string> { "Kapı altı boşlukları kontrol edildi", "Köşe gönyeleri tam birleşti" }
                });

                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = 3,
                    TargetDate = baseDate.AddDays(3),
                    Role = "Formen & Temizlik",
                    RoleBadgeClass = "bg-secondary",
                    Category = "Koruma & Teslim (T+)",
                    CategoryBadgeClass = "bg-secondary",
                    Title = "Oluklu Mukavva ile Zemin Yüzey Koruması",
                    Description = "Döşenen parkenin üzeri kalın oluklu mukavva veya zemin koruma keçesi ile tamamen kaplanır; diğer taşeronların (boyacı, elektrikçi) çizmesi engellenir.",
                    CriticalWarning = "Korunmayan parke, iskele ayağı veya boya damlamasıyla tamirat kabul etmez hale gelir.",
                    SourcePhase = "Adım 4: İç Mekan",
                    SourceItem = "Zemin Koruma",
                    ProfessionName = "Şantiye Saha Ekibi",
                    CheckItems = new List<string> { "Oluklu mukavva serildi ve bantlandı", "Ağır malzeme konulmaması için uyarı levhası asıldı" }
                });
            }

            // 3. SU YALITIMI & İZOLASYON (Membran, Likit Bitüm, Temel Bohçalama)
            bool hasWaterproofing = details.Any(d => IsMatch(d.SelectedItem, "membran", "yalıtım", "izolasyon", "likit", "bohçalama"));
            if (hasWaterproofing)
            {
                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = -1,
                    TargetDate = baseDate.AddDays(-1),
                    Role = "Şantiye Şefi",
                    RoleBadgeClass = "bg-warning text-dark",
                    Category = "Ön Hazırlık (T-)",
                    CategoryBadgeClass = "bg-warning text-dark",
                    Title = "Yüzey Pahları & Bitüm Astar (Primer) Kabulü",
                    Description = "Temel-perde kesişim köşelerinde 45 derece pah yapıldı mı? Yüzey tozdan arındırılıp soğuk uygulamalı bitüm astar tamamen kurudu mu?",
                    CriticalWarning = "Pah yapılmayan 90 derecelik dik köşelerde membran gerilerek yırtılır ve su içeri dolar!",
                    SourcePhase = "Adım 2: Kot 0 & Temel İzolasyonu",
                    SourceItem = "Su Yalıtımı Yüzey Hazırlığı",
                    ProfessionName = "İzolasyon Ustası",
                    CheckItems = new List<string> { "45° köşebent pahları kontrol edildi", "Astar kuruma testi yapıldı" }
                });

                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = 0,
                    TargetDate = baseDate,
                    Role = "👷 İzolasyon Ustası",
                    RoleBadgeClass = "bg-primary",
                    Category = "Ana İmalat (T 0)",
                    CategoryBadgeClass = "bg-primary",
                    Title = "Çift Kat Membran / Sürme İzolasyon Kaynağı",
                    Description = "Şaloma alevi ile enine 10 cm, boyuna 15 cm bindirme payları şaşırtmalı olarak tam kaynatılır. İkinci kat, ilk kata dik ve şaşırtmalı uygulanır.",
                    CriticalWarning = "Hava kabarcığı kalan yerler kesilip yamanmalı, bindirmelerde alev ısısı tam ayarlanmalıdır.",
                    SourcePhase = "Adım 2: Kot 0 & Temel İzolasyonu",
                    SourceItem = "Membran Uygulaması",
                    ProfessionName = "İzolasyon Ustası",
                    CheckItems = new List<string> { "Bindirme kaynakları kontrol edildi", "Hava boşlukları giderildi" }
                });

                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = 1,
                    TargetDate = baseDate.AddDays(1),
                    Role = "Şantiye Şefi & Denetim",
                    RoleBadgeClass = "bg-danger",
                    Category = "Kritik Test (T+)",
                    CategoryBadgeClass = "bg-danger",
                    Title = "24-48 Saat Su Basma (Havuzlama) Testi",
                    Description = "İzolasyon yapılan alan su ile doldurularak 24-48 saat boyunca bekletilir; alt kattan veya perde arkasından nem/damlama kontrolü yapılır.",
                    CriticalWarning = "Toprak dolgusu yapılmadan önce havuzlama testi yapılmazsa sonradan sızıntıyı bulmak imkansızdır!",
                    SourcePhase = "Adım 2: Kot 0 & Temel İzolasyonu",
                    SourceItem = "Sızdırmazlık Testi",
                    ProfessionName = "Şantiye Şefi & İzolasyoncu",
                    CheckItems = new List<string> { "Su seviye çizgisi çekildi", "24 saat sonra su seviyesi kontrol edildi", "Sızıntı olmadığı tutanağa bağlandı" }
                });

                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = 3,
                    TargetDate = baseDate.AddDays(3),
                    Role = "Formen & Altyapı Ekibi",
                    RoleBadgeClass = "bg-success",
                    Category = "Mekanik Koruma (T+)",
                    CategoryBadgeClass = "bg-success",
                    Title = "Drenaj Levhası & Kademeli Toprak Dolgusu",
                    Description = "Yalıtımın üzerine kabarcıklı drenaj levhası monte edilir. Çevre drenaj borusu keçeyle sarılır ve kademeli olarak dolgu sıkıştırılır.",
                    CriticalWarning = "Doğrudan moloz veya sivri taşla dolgu yapılırsa membran delinir ve yalıtım tamamen çöpe gider!",
                    SourcePhase = "Adım 2: Kot 0 & Temel İzolasyonu",
                    SourceItem = "Yalıtım Koruma & Drenaj",
                    ProfessionName = "Altyapı Taşeronu",
                    CheckItems = new List<string> { "Drenaj levhası kabarcıkları duvara bakacak şekilde asıldı", "Kademeli sıkıştırma yapıldı" }
                });
            }

            // 4. DIŞ CEPHE MANTOLAMA & İSKELE (Taş Yünü, EPS, XPS)
            bool hasFacade = details.Any(d => IsMatch(d.SelectedItem, "mantolama", "taş yünü", "eps", "xps", "cephe"));
            if (hasFacade)
            {
                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = -3,
                    TargetDate = baseDate.AddDays(-3),
                    Role = "İSG Uzmanı & Şef",
                    RoleBadgeClass = "bg-danger",
                    Category = "İSG Kabulü (T-)",
                    CategoryBadgeClass = "bg-danger",
                    Title = "Dış Cephe İskelesi Muayenesi & Yeşil Etiket",
                    Description = "İskele ankrajlarının duvara tespiti, çift sıra korkuluklar, topuk tahtaları ve güvenlik ağları İSG uzmanı tarafından kontrol edilerek YEŞİL ETİKET asılır.",
                    CriticalWarning = "Yeşil etiket olmayan iskeleye işçi çıkarmak ağır ceza ve şantiye mühürleme sebebidir!",
                    SourcePhase = "Adım 3: Gövde & Dış Cephe",
                    SourceItem = "İskele Güvenliği",
                    ProfessionName = "İskele Kurulum Ekibi & İSG",
                    CheckItems = new List<string> { "Duvar ankrajları teyit edildi", "Topuk levhaları tamamlandı", "Yeşil etiket asıldı" }
                });

                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = 0,
                    TargetDate = baseDate,
                    Role = "👷 Mantolama Ekibi",
                    RoleBadgeClass = "bg-primary",
                    Category = "Ana İmalat (T 0)",
                    CategoryBadgeClass = "bg-primary",
                    Title = "Yalıtım Levhalarının Şaşırtmalı Yapıştırılması",
                    Description = "Levhalar kenarlarına şerit, ortasına noktasal harç sürülerek (min %40 yapışma alanı) tuğla örgüsü gibi şaşırtmalı derzle cepheye yapıştırılır.",
                    CriticalWarning = "Köşelerde düz derz bırakılmaz; pencere köşelerinde L kesim levha kullanılmalıdır, aksi halde çapraz çatlaklar kaçınılmazdır!",
                    SourcePhase = "Adım 3: Gövde & Dış Cephe",
                    SourceItem = "Levha Yapıştırma",
                    ProfessionName = "Mantolama Ustası",
                    CheckItems = new List<string> { "Pencerelerde L-kesim uygulandı", "Köşe profilleri yerleştirildi" }
                });

                tasks.Add(new DnaTriggerTask
                {
                    Id = taskIdSeq++,
                    OffsetDays = 1,
                    TargetDate = baseDate.AddDays(1),
                    Role = "Şantiye Şefi",
                    RoleBadgeClass = "bg-warning text-dark",
                    Category = "Mekanik Bağlantı (T+)",
                    CategoryBadgeClass = "bg-warning text-dark",
                    Title = "Mekanik Dübelleme & Fileli Sıva Uygulaması",
                    Description = "Yapıştırıcı kuruduktan sonra m²'ye en az 6 adet çelik/plastik çivili dübel çakılır. 160 gr/m² alkali dayanımlı sıva filesi ilk kat sıvaya gömülür.",
                    CriticalWarning = "Dübel sayısı az tutulursa rüzgar vakumuyla levhalar cepheden blok halinde kopar!",
                    SourcePhase = "Adım 3: Gövde & Dış Cephe",
                    SourceItem = "Dübelleme & Sıva",
                    ProfessionName = "Mantolama Ekibi",
                    CheckItems = new List<string> { "Dübel adedi sayıldı (min 6 adet/m²)", "File ek yerlerinde 10 cm bindirme yapıldı" }
                });
            }

            // Kronolojik sıralama: T-7'den T+28'e kadar gün sırasına göre diz
            return tasks.OrderBy(t => t.OffsetDays).ThenBy(t => t.Id).ToList();
        }

        /// <summary>
        /// Üretilen tetikleyici görevleri sistemin mevcut Takvim / Ajandasına (AgendaRecord) tek tıkla işler.
        /// </summary>
        public async Task<int> SyncTasksToAgendaAsync(int agencyId, int projectId, List<DnaTriggerTask> tasks)
        {
            if (tasks == null || !tasks.Any()) return 0;

            int count = 0;
            foreach (var t in tasks)
            {
                // Aynı başlık ve tarihte kayıt var mı kontrol et
                bool exists = await _context.AgendaRecords.AnyAsync(a =>
                    a.AgencyId == agencyId &&
                    a.ProjectId == projectId &&
                    a.EventDate.Date == t.TargetDate.Date &&
                    a.Title == t.Title &&
                    !a.IsDeleted);

                if (!exists)
                {
                    var record = new AgendaRecord
                    {
                        AgencyId = agencyId,
                        ProjectId = projectId,
                        Title = $"[{t.OffsetLabel}] [{t.Role}] {t.Title}",
                        Notes = $"Kategori: {t.Category}\nİlgili İmalat: {t.SourceItem} ({t.SourcePhase})\nTaşeron/Ekip: {t.ProfessionName}\n\nİş Tanımı:\n{t.Description}\n\n⚠️ KRİTİK ŞANTİYE UYARISI:\n{t.CriticalWarning}\n\nKontrol Maddeleri:\n- " + string.Join("\n- ", t.CheckItems),
                        EventDate = t.TargetDate,
                        RecordTypeId = AgendaConstants.TypeSantiyeDenetimi,
                        StatusId = AgendaConstants.StatusPlanlandi,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.AgendaRecords.Add(record);
                    count++;
                }
            }

            if (count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return count;
        }

        private bool IsMatch(string text, params string[] keywords)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            var lower = text.ToLower();
            return keywords.Any(k => lower.Contains(k.ToLower()));
        }
    }
}
