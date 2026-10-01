using System;
using System.IO;

string filepath = @"GMK360.Web\Controllers\ProjectFinanceController.cs";
string content = File.ReadAllText(filepath, System.Text.Encoding.UTF8);

content = content.Replace(".Include(p => p.Address)", "");

File.WriteAllText(filepath, content, System.Text.Encoding.UTF8);
