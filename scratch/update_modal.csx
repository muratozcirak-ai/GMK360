using System.Text.RegularExpressions;

string filepath = @"GMK360.Web\Views\Admin\B2bList.cshtml";
string content = System.IO.File.ReadAllText(filepath);

string ustaFieldsOld = @"<div class=""mb-3"">
                        <label class=""form-label fw-bold small"">Usta Adı Soyadı (veya Taşeron Ekip Adı)</label>
                        <input type=""text"" class=""form-control"" placeholder=""Örn: Ahmet Yılmaz"" />
                    </div>
                    <div class=""mb-3"">
                        <label class=""form-label fw-bold small"">TC Kimlik No</label>
                        <input type=""text"" class=""form-control"" placeholder=""İsteğe Bağlı"" />
                    </div>";

string ustaFieldsNew = @"<div class=""mb-3"">
                        <label class=""form-label fw-bold small"">Usta Adı Soyadı (veya Taşeron Ekip Adı)</label>
                        <input type=""text"" class=""form-control"" placeholder=""Örn: Ahmet Yılmaz"" />
                    </div>
                    <div class=""row"">
                        <div class=""col-6 mb-3"">
                            <label class=""form-label fw-bold small"">TC Kimlik No</label>
                            <input type=""text"" class=""form-control"" placeholder=""İsteğe Bağlı"" />
                        </div>
                        <div class=""col-6 mb-3"">
                            <label class=""form-label fw-bold small"">Telefon Numarası</label>
                            <input type=""text"" class=""form-control"" placeholder=""05XX XXX XX XX"" />
                        </div>
                    </div>
                    <div class=""row"">
                        <div class=""col-4 mb-3"">
                            <label class=""form-label fw-bold small"">Hizmet İli</label>
                            <select class=""form-select border-0 shadow-sm select2-search"" id=""modalCity"" style=""width: 100%;"">
                                <option value="""">İl Seçin</option>
                                @if(cities != null)
                                {
                                    foreach(var c in cities)
                                    {
                                        <option value=""@c.Id"">@c.Name</option>
                                    }
                                }
                            </select>
                        </div>
                        <div class=""col-4 mb-3"">
                            <label class=""form-label fw-bold small"">İlçe</label>
                            <select class=""form-select border-0 shadow-sm select2-search"" id=""modalDistrict"" style=""width: 100%;"">
                                <option value="""">İlçe Seçin</option>
                            </select>
                        </div>
                        <div class=""col-4 mb-3"">
                            <label class=""form-label fw-bold small"">Mahalle</label>
                            <select class=""form-select border-0 shadow-sm select2-search"" id=""modalNeighborhood"" style=""width: 100%;"">
                                <option value="""">Mahalle Seçin</option>
                            </select>
                        </div>
                    </div>";

content = content.Replace(ustaFieldsOld, ustaFieldsNew);
System.IO.File.WriteAllText(filepath, content);
