import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Construction\AgencyWorker.cs'

with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

new_fields = '''        public string WorkerType { get; set; } = "Firma Personeli"; // Taşeron, Firma Personeli, Yevmiyeci
        public string SubcontractorName { get; set; } // Eğer taşeron ise firma adı

        // Taşeronun rehber kaydı (Gölge kullanıcı bağlantısı için)
        public int? SubcontractorContactId { get; set; }
        public GMK360.Core.Entities.AgencyPhonebook SubcontractorContact { get; set; }'''

content = content.replace('        public string WorkerType { get; set; } = "Firma Personeli"; // Taşeron, Firma Personeli, Yevmiyeci\r\n        public string SubcontractorName { get; set; } // Eğer taşeron ise firma adı', new_fields).replace('        public string WorkerType { get; set; } = "Firma Personeli"; // Taşeron, Firma Personeli, Yevmiyeci\n        public string SubcontractorName { get; set; } // Eğer taşeron ise firma adı', new_fields)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
