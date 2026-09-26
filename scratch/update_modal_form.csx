using System.Text.RegularExpressions;

string filepath = @"GMK360.Web\Views\Admin\B2bList.cshtml";
string content = System.IO.File.ReadAllText(filepath);

// Replace generic modal-body with form
string oldBody = @"<div class=""modal-body"">";
string newBody = @"<form asp-action=""AddUsta"" method=""post"">
                <div class=""modal-body"">";
content = content.Replace(oldBody, newBody);

// Name inputs
content = content.Replace(@"placeholder=""Örn: Ahmet Yılmaz"" />", @"placeholder=""Örn: Ahmet Yılmaz"" name=""name"" required />");
content = content.Replace(@"placeholder=""İsteğe Bağlı"" />", @"placeholder=""İsteğe Bağlı"" name=""tcKimlik"" />");
content = content.Replace(@"placeholder=""05XX XXX XX XX"" />", @"placeholder=""05XX XXX XX XX"" name=""phone"" required />");

// Select names
content = content.Replace(@"id=""modalCity""", @"id=""modalCity"" name=""cityId"" required");
content = content.Replace(@"id=""modalDistrict""", @"id=""modalDistrict"" name=""districtId""");
content = content.Replace(@"id=""modalNeighborhood""", @"id=""modalNeighborhood"" name=""neighborhoodId""");

// Category select
content = content.Replace(@"multiple=""multiple"" style=""width: 100%;"">", @"multiple=""multiple"" style=""width: 100%;"" name=""categoryIds"" required>");

// Modal footer buttons
string oldFooter = @"<button type=""button"" class=""btn @btnColor rounded-pill px-4 fw-bold"">Kaydet (Mock)</button>";
string newFooter = @"<button type=""submit"" class=""btn @btnColor rounded-pill px-4 fw-bold"">Kaydet</button>";
content = content.Replace(oldFooter, newFooter);

// Close form
string oldFooterEnd = @"</div>
        </div>
    </div>
</div>";
string newFooterEnd = @"</div>
            </form>
        </div>
    </div>
</div>";
content = content.Replace(oldFooterEnd, newFooterEnd);

System.IO.File.WriteAllText(filepath, content);
