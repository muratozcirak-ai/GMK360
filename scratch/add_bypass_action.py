import codecs
import re

path = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

action = '''
        [HttpPost]
        public async Task<IActionResult> BypassPhase(int projectId, int phaseId, string redirectUrl)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project != null)
            {
                var bp = project.BypassedPhases ?? "";
                var phaseStr = phaseId.ToString();
                
                if (!bp.Split(',').Contains(phaseStr))
                {
                    if (string.IsNullOrEmpty(bp)) bp = phaseStr;
                    else bp += "," + phaseStr;
                    project.BypassedPhases = bp;
                    
                    // Log the critical action
                    var audit = new GMK360.Core.Entities.System.SystemAuditLog
                    {
                        Action = "PHASE_BYPASS_LOCK",
                        Description = $"Proje ID: {projectId}, Faz ID: {phaseId} için Yasal Uyarı Hard Lock mekanizması kullanıcı yetkisiyle kırıldı. Dijital taahhüt onaylandı.",
                        UserId = _userManager.GetUserId(User),
                        Timestamp = System.DateTime.UtcNow
                    };
                    _context.SystemAuditLogs.Add(audit);
                    
                    await _context.SaveChangesAsync();
                }
            }
            return Redirect(redirectUrl);
        }
'''

if 'BypassPhase' not in content:
    content = re.sub(
        r'(public async Task<IActionResult> SyncAllPhases\(int id\)\s*\{[^\}]*\})',
        r'\1\n' + action,
        content, flags=re.DOTALL
    )
    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)
    print("Action added")