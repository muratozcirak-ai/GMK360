with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

# I already inserted it. Let's replace the inserted prevStep with a better one.
import re
new_prev = """function prevStep(step) {
            document.querySelectorAll('.wizard-step').forEach(el => el.classList.remove('active'));
            document.getElementById('step' + step).classList.add('active');
            
            if(step == 1) {
                document.getElementById('ind2').classList.remove('active');
                document.getElementById('ind3').classList.remove('active');
            } else if(step == 2) {
                document.getElementById('ind3').classList.remove('active');
            }
        }"""
content = re.sub(r'function prevStep\(step\) \{.*?\}\s*function nextStep', new_prev + '\n\n        function nextStep', content, flags=re.DOTALL)

with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
print("Updated prevStep function")
