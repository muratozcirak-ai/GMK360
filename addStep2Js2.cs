using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(path, Encoding.UTF8);

        string oldBtn = @"onclick=""prepareSummary()""";
        string newBtn = @"onclick=""saveStep2AndContinue()""";
        if (view.Contains(oldBtn))
        {
            view = view.Replace(oldBtn, newBtn);
        }

        string scriptCode = @"
        function saveStep2AndContinue() {
            var form = document.getElementById('wizardForm');
            if(!form.checkValidity()) {
                form.reportValidity();
                return;
            }

            var formData = new FormData(form);
            fetch('/ConstructionProject/SaveStep2', {
                method: 'POST',
                body: formData
            })
            .then(res => res.json())
            .then(data => {
                if(data.success) {
                    prepareSummary();
                } else {
                    alert('Kayıt sırasında bir hata oluştu: ' + data.message);
                }
            })
            .catch(err => {
                console.error(err);
                alert('Bağlantı hatası.');
            });
        }
";
        if (!view.Contains("saveStep2AndContinue"))
        {
            int idx = view.IndexOf("function prepareSummary() {");
            if (idx != -1) {
                // backtrack to find start of line
                int start = view.LastIndexOf('\n', idx);
                if (start == -1) start = idx;
                view = view.Insert(start, scriptCode);
                File.WriteAllText(path, view, new UTF8Encoding(true));
                Console.WriteLine("Added SaveStep2 JS.");
            }
        }
    }
}
