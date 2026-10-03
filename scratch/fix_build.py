import codecs

path_ctrl = 'GMK360.Web/Controllers/PhaseZeroController.cs'
with codecs.open(path_ctrl, 'r', 'utf-8-sig') as f:
    ctrl_content = f.read()

# Fix the controller AddFirmQuote method
ctrl_content = ctrl_content.replace('Status = "Aktif"', 'OwnerAgencyId = quoteRequest.RequesterAgencyId,\n                  AddedByUserId = quoteRequest.RequesterUserId')

with codecs.open(path_ctrl, 'w', 'utf-8-sig') as f:
    f.write(ctrl_content)


path_view = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path_view, 'r', 'utf-8-sig') as f:
    view_content = f.read()

# Fix the childQuote usage
# We can just change the button condition to evaluate it directly
view_content = view_content.replace('@if(childQuote != null) {', '@if((ViewBag.Quotes as IEnumerable<GMK360.Core.Entities.B2B.B2BQuoteRequest>)?.FirstOrDefault(q => q.SourceReferenceId == childDoc.Id) != null) {')
# Also we need to replace @childQuote.Id inside the button!
# Let's just do a regex sub to declare childQuote_tmp before the buttons, or replace the parameter
# Let's just replace @childQuote.Id with @((ViewBag.Quotes as IEnumerable<GMK360.Core.Entities.B2B.B2BQuoteRequest>)?.FirstOrDefault(q => q.SourceReferenceId == childDoc.Id)?.Id)
view_content = view_content.replace('openInviteModal(@childQuote.Id', 'openInviteModal(@(((ViewBag.Quotes as IEnumerable<GMK360.Core.Entities.B2B.B2BQuoteRequest>)?.FirstOrDefault(q => q.SourceReferenceId == childDoc.Id))?.Id)')

with codecs.open(path_view, 'w', 'utf-8-sig') as f:
    f.write(view_content)