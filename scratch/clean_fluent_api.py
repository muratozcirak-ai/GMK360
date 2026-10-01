import io
import re

filepath = r'GMK360.Data\Contexts\ApplicationDbContext.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Pattern to remove the DailyTimesheet fluent API for ProjectPhase and PhaseTask
pattern = r'builder\.Entity<GMK360\.Core\.Entities\.Construction\.DailyTimesheet>\(\)\s*\.HasOne\(t => t\.ProjectPhase\)\s*\.WithMany\(\)\s*\.HasForeignKey\(t => t\.ProjectPhaseId\)\s*\.OnDelete\(DeleteBehavior\.Restrict\);\s*builder\.Entity<GMK360\.Core\.Entities\.Construction\.DailyTimesheet>\(\)\s*\.HasOne\(t => t\.PhaseTask\)\s*\.WithMany\(\)\s*\.HasForeignKey\(t => t\.PhaseTaskId\)\s*\.OnDelete\(DeleteBehavior\.Restrict\);'

content = re.sub(pattern, '', content, flags=re.MULTILINE)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Cleaned fluent API")
