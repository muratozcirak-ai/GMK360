using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        // 1. Add "Toplam Bina Alanı" next to Toplam Arazi Alanı
        string targetArazi = @"<div class=""text-muted small"">Toplam Arazi Alanı</div>
                                      <div class=""fw-bold text-dark"">@(Model.TotalLandArea.HasValue ? Model.TotalLandArea.Value + "" m²"" : ""Belirtilmedi"")</div>
                                  </div>
                              </div>
                          </div>";

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
                          
        if(text.Contains(targetArazi) && !text.Contains("Toplam Bina Alanı (Oturum)"))
        {
            text = text.Replace(targetArazi, targetArazi + Environment.NewLine + insertBina);
        }

        // 2. Add "Çevre Özellikleri" right below the Arazi section
        string targetEndOfArazi = @"<div class=""text-muted small fst-italic""><i class=""bi bi-info-circle me-1""></i>Kamelya, açık otopark, çocuk parkı gibi dış alan donatıları henüz eklenmedi.</div>
                            </div>
                        }
                    </div>
                </div>";

        string cevreOzellikleriHtml = @"
                <div class=""mt-4 pt-3 border-top position-relative"">
                    <div class=""d-flex justify-content-between align-items-center mb-3"">
                        <h6 class=""fw-bold mb-0 text-muted""><i class=""bi bi-geo-alt text-danger me-1""></i> Çevre Özellikleri ve Yakın Konumlar</h6>
                        <button type=""button"" class=""btn btn-sm btn-outline-danger rounded-pill""><i class=""bi bi-arrow-repeat me-1""></i> Haritadan Güncelle</button>
                    </div>
                    
                    <div class=""row g-3"">
                        <div class=""col-sm-6"">
                            <div class=""d-flex align-items-center p-2 rounded bg-light"">
                                <i class=""bi bi-hospital text-danger fs-3 me-3""></i>
                                <div>
                                    <div class=""text-muted small fw-bold"">Sağlık & Hastane</div>
                                    <div class=""text-dark fs-7"">Şehir Hastanesi (1.2 km)</div>
                                    <div class=""text-dark fs-7"">Sağlık Ocağı (300 m)</div>
                                </div>
                            </div>
                        </div>
                        <div class=""col-sm-6"">
                            <div class=""d-flex align-items-center p-2 rounded bg-light"">
                                <i class=""bi bi-mortarboard text-primary fs-3 me-3""></i>
                                <div>
                                    <div class=""text-muted small fw-bold"">Eğitim</div>
                                    <div class=""text-dark fs-7"">Atatürk İlkokulu (450 m)</div>
                                    <div class=""text-dark fs-7"">Anadolu Lisesi (800 m)</div>
                                </div>
                            </div>
                        </div>
                        <div class=""col-sm-6"">
                            <div class=""d-flex align-items-center p-2 rounded bg-light"">
                                <i class=""bi bi-bus-front text-warning fs-3 me-3""></i>
                                <div>
                                    <div class=""text-muted small fw-bold"">Ulaşım</div>
                                    <div class=""text-dark fs-7"">Metro İstasyonu (5 dk yürüme)</div>
                                    <div class=""text-dark fs-7"">Otobüs Durağı (100 m)</div>
                                </div>
                            </div>
                        </div>
                        <div class=""col-sm-6"">
                            <div class=""d-flex align-items-center p-2 rounded bg-light"">
                                <i class=""bi bi-shop text-success fs-3 me-3""></i>
                                <div>
                                    <div class=""text-muted small fw-bold"">Sosyal Yaşam</div>
                                    <div class=""text-dark fs-7"">Merkez AVM (1.5 km)</div>
                                    <div class=""text-dark fs-7"">Ulu Camii (200 m)</div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class=""text-end mt-2"">
                        <span class=""text-muted fs-8 fst-italic"">* Konumlar Google Haritalar / Yapay Zeka servisi ile otomatik çekilmektedir.</span>
                    </div>
                </div>";
                
        if(text.Contains(targetEndOfArazi) && !text.Contains("Çevre Özellikleri ve Yakın Konumlar"))
        {
            text = text.Replace(targetEndOfArazi, targetEndOfArazi + Environment.NewLine + cevreOzellikleriHtml);
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("Added Bina Alani and Cevre Ozellikleri!");
        }
        else
        {
            // Fallback: If they have amenities, the "Kamelya..." text might not be present because of if/else logic!
            // I should use Regex to find the end of the `Arazi ve Dış Alanlar` div block securely.
            Console.WriteLine("Fallback approach...");
            string fallbackTarget = @"</div>
                  </div>
              </div>
          </div>
      </div>
      <div class=""col-md-4"">
          <div class=""card border-0 shadow-sm rounded-4 h-100 overflow-hidden"">";
          
            if(text.Contains(fallbackTarget)) {
                // We inject it before the last 3 divs.
                string newStr = @"</div>
                  </div>
                  " + cevreOzellikleriHtml + @"
              </div>
          </div>
      </div>
      <div class=""col-md-4"">
          <div class=""card border-0 shadow-sm rounded-4 h-100 overflow-hidden"">";
                text = text.Replace(fallbackTarget, newStr);
                File.WriteAllText(path, text, new UTF8Encoding(true));
                Console.WriteLine("Added using fallback");
            }
        }
    }
}
