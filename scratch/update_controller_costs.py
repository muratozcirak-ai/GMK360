import sys

filepath = 'GMK360.Web/Controllers/ConstructionProjectController.cs'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Update GET JSON response
target_get = """issueNotes = doc.IssueNotes,
                filePath = doc.FilePath,"""
replacement_get = """issueNotes = doc.IssueNotes,
                filePath = doc.FilePath,
                documentFee = doc.DocumentFee,
                additionalCost = doc.AdditionalCost,"""

content = content.replace(target_get, replacement_get)

# 2. Update POST signature
target_post_sig = """public async Task<IActionResult> UpdateDocumentDetails(int Id, string Status, string AssignedUserId, string InstitutionContact, DateTime? StartDate, DateTime? CompletedDate, string IssueNotes, IFormFile UploadedFile)"""
replacement_post_sig = """public async Task<IActionResult> UpdateDocumentDetails(int Id, string Status, string AssignedUserId, string InstitutionContact, DateTime? StartDate, DateTime? CompletedDate, string IssueNotes, decimal? DocumentFee, decimal? AdditionalCost, IFormFile UploadedFile)"""

content = content.replace(target_post_sig, replacement_post_sig)

# 3. Update POST assignment
target_post_assign = """doc.IssueNotes = IssueNotes;"""
replacement_post_assign = """doc.IssueNotes = IssueNotes;
            doc.DocumentFee = DocumentFee;
            doc.AdditionalCost = AdditionalCost;"""

content = content.replace(target_post_assign, replacement_post_assign)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
    print("Controller updated for costs.")
