using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        string searchPoint = @"<div class=""text-muted small"">Toplam Arazi Alan"; // partial match
        
        int idx1 = text.IndexOf(searchPoint);
        if (idx1 != -1)
        {
            // Find the end of that col-sm-6 block
            int idx2 = text.IndexOf("</div>", idx1);
            int idx3 = text.IndexOf("</div>", idx2 + 6);
            int idx4 = text.IndexOf("</div>", idx3 + 6);
            int endOfBlock = idx4 + 6;

            string insertBina = @"
                          <div class=""col-sm-6"">
                              <div class=""d-flex align-items-center"">
                                  <i class=""bi bi-building text-primary fs-4 me-3""></i>
                                  <div>
                                      <div class=""text-muted small"">Toplam Bina Alanı (Oturum)</div>
                                      <div class=""fw-bold text-dark"">@(Model.Blocks?.Sum(b => b.BaseArea ?? 0) > 0 ? Model.Blocks.Sum(b => b.BaseArea ?? 0) + "" m²"" : ""Belirtilmedi"")</div>
                                  </div>
                              </div>
                          </div>";

            text = text.Insert(endOfBlock, insertBina);
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("Added Toplam Bina Alani");
        }
        else
        {
            Console.WriteLine("Could not find Toplam Arazi Alani");
        }
    }
}
