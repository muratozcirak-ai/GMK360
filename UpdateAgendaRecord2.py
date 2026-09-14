import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Construction\AgendaRecord.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

if 'public virtual System.Collections.Generic.ICollection<AgendaParticipant> Participants { get; set; }' not in content:
    content = content.replace('public virtual System.Collections.Generic.ICollection<AgendaItem> Items { get; set; }', 
                              'public virtual System.Collections.Generic.ICollection<AgendaItem> Items { get; set; }\r\n        public virtual System.Collections.Generic.ICollection<AgendaParticipant> Participants { get; set; }')
    
with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
