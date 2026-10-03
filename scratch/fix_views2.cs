using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

class Program {
    static void Main() {
        string[] phases = { "PhaseOne", "PhaseTwo", "PhaseThree", "PhaseFour", "PhaseFive", "PhaseSix", "PhaseSeven", "PhaseEight" };
        
        foreach (var p in phases) {
            string path = string.Format(@"C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\{0}\Index.cshtml", p);
            if (!File.Exists(path)) continue;
            
            string content = File.ReadAllText(path);
            
            string target = @"<!-- Action Buttons -->.*?</div>";
            string replacement = string.Format(@"<!-- Action Buttons -->
                                                <div class=""d-flex align-items-center"">
                                                    @if (item.PlannedTotalCost == 0)
                                                    {{
                                                        <button class=""btn btn-sm btn-primary rounded-pill fw-bold px-3 me-2"" onclick=""openManageModal(@item.Id, '@item.ItemName', '0', '@item.Description', '1', 'Götürü')"">
                                                            <i class=""bi bi-currency-dollar""></i> Tutar Gir
                                                        </button>
                                                    }}
                                                    else
                                                    {{
                                                        <button class=""btn btn-sm btn-outline-primary rounded-pill fw-bold px-3 me-2"" onclick=""openManageModal(@item.Id, '@item.ItemName', '@item.PlannedTotalCost.ToString(System.Globalization.CultureInfo.InvariantCulture)', '@item.Description', '@item.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture)', '@item.Unit')"">
                                                            <i class=""bi bi-pencil""></i> Düzenle
                                                        </button>
                                                    }}

                                                    <form action=""/{0}/DeleteItem/@item.Id"" method=""post"" class=""d-inline"">
                                                        <button type=""submit"" class=""btn btn-sm btn-outline-danger border-0""><i class=""bi bi-trash""></i></button>
                                                    </form>
                                                </div>", p);
            content = Regex.Replace(content, target, replacement, RegexOptions.Singleline);
            
            content = Regex.Replace(content, @"<!-- Strateji Modal -->.*?<!-- Yeni Kalem Ekle Modal -->", "<!-- Yeni Kalem Ekle Modal -->", RegexOptions.Singleline);
            content = Regex.Replace(content, @"function openStrategyModal\(id, title\).*?}", "", RegexOptions.Singleline);
            
            string patternIfElse = @"@if\s*\(\s*item\.QuoteStatus.*?<!-- Action Buttons -->";
            string replacementIfElse = @"<!-- Action Buttons -->";
            content = Regex.Replace(content, patternIfElse, replacementIfElse, RegexOptions.Singleline);
            
            string trailingDiv = @"</div\s*>\s*</td\s*>\s*</tr\s*>";
            string trailingDivRep = @"</td></tr>";
            content = Regex.Replace(content, trailingDiv, trailingDivRep, RegexOptions.Singleline);

            File.WriteAllText(path, content, System.Text.Encoding.UTF8);
        }
    }
}