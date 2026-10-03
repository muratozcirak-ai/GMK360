import codecs

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Add canRequestChildQuote
insert_pos = content.find('var childDoc = Model.FirstOrDefault(d => d.SystemTemplateId == pr.PrerequisiteTemplateId);')
if insert_pos != -1:
    # Find the end of this line
    end_of_line = content.find('\n', insert_pos)
    if end_of_line != -1:
        vars_to_add = '''
                                                bool canRequestChildQuote = childDoc?.SystemTemplate?.IssuedBy != null && biddableKeywords.Any(k => childDoc.SystemTemplate.IssuedBy.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
        '''
        content = content[:end_of_line] + vars_to_add + content[end_of_line:]

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)