using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(path, Encoding.UTF8);

        // Add hidden input for DraftProjectId after the form tag
        string formStart = "enctype=\"multipart/form-data\">";
        if (view.Contains(formStart) && !view.Contains("DraftProjectId"))
        {
            view = view.Replace(formStart, formStart + "\n                    <input type=\"hidden\" id=\"DraftProjectId\" name=\"DraftProjectId\" value=\"0\" />");
        }

        // Change the Devam Et button in step 1 to call saveStep1AndContinue()
        string oldBtn = @"onclick=""nextStep(2)""";
        string newBtn = @"onclick=""saveStep1AndContinue()""";
        if (view.Contains(oldBtn))
        {
            // Replace only the first occurrence which is in Step 1
            int idx = view.IndexOf(oldBtn);
            if (idx != -1) {
                view = view.Remove(idx, oldBtn.Length).Insert(idx, newBtn);
            }
        }

        // Add the JS function
        string scriptCode = @"
        function saveStep1AndContinue() {
            var form = document.getElementById('wizardForm');
            if(!form.checkValidity()) {
                form.reportValidity();
                return;
            }

            // AJAX POST
            var formData = new FormData(form);
            fetch('/ConstructionProject/SaveStep1', {
                method: 'POST',
                body: formData
            })
            .then(res => res.json())
            .then(data => {
                if(data.success) {
                    document.getElementById('DraftProjectId').value = data.projectId;
                    nextStep(2);
                } else {
                    alert('Kayıt sırasında bir hata oluştu.');
                }
            })
            .catch(err => {
                console.error(err);
                alert('Bağlantı hatası.');
            });
        }
";
        if (!view.Contains("var formData = new FormData(form);"))
        {
            view = view.Replace("function nextStep(step) {", scriptCode + "\n        function nextStep(step) {");
            File.WriteAllText(path, view, new UTF8Encoding(true));
            Console.WriteLine("Added Draft JS.");
        }
    }
}
