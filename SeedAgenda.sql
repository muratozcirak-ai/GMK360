USE GMK360DB;

DECLARE @AgendaId INT;
SELECT TOP 1 @AgendaId = Id FROM AgendaRecords;

IF @AgendaId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM AgendaItems)
BEGIN
    INSERT INTO AgendaItems (AgendaRecordId, OrderNo, TopicTitle, PresentationText, ImageUrl, LiveMeetingNotes)
    VALUES 
    (@AgendaId, 1, 'A Blok Temel Betonu Fiyatları', 'A Blok temel betonu için 3 farklı hazır beton firmasından fiyat alınmıştır. En uygun teklif 150.000 TL olarak belirlendi. Buna göre döküm programı oluşturulacaktır.', 'https://images.unsplash.com/photo-1541888081622-12a832f05a96?auto=format&fit=crop&q=80&w=800', ''),
    
    (@AgendaId, 2, 'Şantiye Güvenlik Denetimi Eksikleri', 'İş güvenliği uzmanı tarafından hazırlanan 10 Ekim tarihli raporda, iskele bağlantılarında 3 noktada zayıflık tespit edilmiştir. Acil müdahale edilmesi gerekmektedir.', 'https://images.unsplash.com/photo-1503387762-592deb58ef4e?auto=format&fit=crop&q=80&w=800', '');
END
