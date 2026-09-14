import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Construction\AgendaRecord.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

if 'public virtual ICollection<AgendaItem> Items { get; set; }' not in content:
    content = content.replace('public virtual AgencyPhonebook Phonebook { get; set; }', 
                              'public virtual AgencyPhonebook Phonebook { get; set; }\r\n\r\n        public virtual System.Collections.Generic.ICollection<AgendaItem> Items { get; set; }')
    
with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
