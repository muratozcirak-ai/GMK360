import codecs

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# We need to define the variables at the top of the loop.
# Find 'var rule = globalRules.FirstOrDefault' and insert them there.
insert_pos = content.find('var rule = globalRules.FirstOrDefault')
if insert_pos != -1:
    vars_to_add = '''
                                        var biddableKeywords = new[] { "Firma", "Taşeron", "Ofis", "Mühendis", "Mimar", "Laboratuvar", "OSGB" };
                                        bool isChildOfSomeone = Model.Any(m => globalRules.FirstOrDefault(r => r.SystemLegalDocumentTemplateId == m.SystemTemplateId)?.Prerequisites?.Any(p => p.PrerequisiteTemplateId == doc.SystemTemplateId) == true);
                                        bool canRequestQuote = doc.SystemTemplate?.IssuedBy != null && biddableKeywords.Any(k => doc.SystemTemplate.IssuedBy.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
    '''
    content = content[:insert_pos] + vars_to_add + content[insert_pos:]

# For childQuote:
insert_pos_child = content.find('bool isReadyToApply = true;')
if insert_pos_child != -1:
    vars_to_add_child = '''
                                        
    '''
    # Wait, we need canRequestChildQuote inside the child loop.
    # Let's find 'var cDoc = Model.FirstOrDefault(d => d.SystemTemplateId == pr.PrerequisiteTemplateId);'
    child_loop_pos = content.find('var childDoc = Model.FirstOrDefault(d => d.SystemTemplateId == pr.PrerequisiteTemplateId);')
    if child_loop_pos == -1:
        # maybe it's called cDoc? Let's check!
        pass

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)