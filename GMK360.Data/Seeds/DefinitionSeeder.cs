using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GMK360.Data.Seeds
{
    public static class DefinitionSeeder
    {
        public static async Task SeedDefinitionsAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();



            // 3. İlan Durumu (Satılık, Kiralık vs.)
            var statusCat = await context.DefinitionCategories.FirstOrDefaultAsync(c => c.SystemCode == "PROPERTY_STATUS");
            if (statusCat == null)
            {
                statusCat = new DefinitionCategory { Name = "İlan Durumu", SystemCode = "PROPERTY_STATUS" };
                context.DefinitionCategories.Add(statusCat);
                await context.SaveChangesAsync();

                context.DefinitionValues.AddRange(
                    new DefinitionValue { CategoryId = statusCat.Id, Name = "Satılık", SystemCode = "SALE", Order = 1 },
                    new DefinitionValue { CategoryId = statusCat.Id, Name = "Kiralık", SystemCode = "RENT", Order = 2 },
                    new DefinitionValue { CategoryId = statusCat.Id, Name = "Günlük Kiralık", SystemCode = "DAILY", Order = 3 },
                    new DefinitionValue { CategoryId = statusCat.Id, Name = "Devren Satılık", SystemCode = "DEVREN_SALE", Order = 4 },
                    new DefinitionValue { CategoryId = statusCat.Id, Name = "Devren Kiralık", SystemCode = "DEVREN_RENT", Order = 5 }
                );
            }

            // 4. İlan Türü (Konut, İş Yeri vs.)
            var typeCat = await context.DefinitionCategories.FirstOrDefaultAsync(c => c.SystemCode == "PROPERTY_TYPE");
            if (typeCat == null)
            {
                typeCat = new DefinitionCategory { Name = "Emlak Türü", SystemCode = "PROPERTY_TYPE" };
                context.DefinitionCategories.Add(typeCat);
                await context.SaveChangesAsync();

                var konutType = new DefinitionValue { CategoryId = typeCat.Id, Name = "Konut", SystemCode = "RESIDENTIAL", Order = 1 };
                var isYeriType = new DefinitionValue { CategoryId = typeCat.Id, Name = "İş Yeri", SystemCode = "COMMERCIAL", Order = 2 };
                var arsaType = new DefinitionValue { CategoryId = typeCat.Id, Name = "Arsa", SystemCode = "LAND", Order = 3 };
                var binaType = new DefinitionValue { CategoryId = typeCat.Id, Name = "Bina", SystemCode = "BUILDING", Order = 4 };
                var devreMulkType = new DefinitionValue { CategoryId = typeCat.Id, Name = "Devre Mülk", SystemCode = "TIMESHARE", Order = 5 };
                var turistikTesisType = new DefinitionValue { CategoryId = typeCat.Id, Name = "Turistik Tesis", SystemCode = "TOURISM", Order = 6 };
                var projeType = new DefinitionValue { CategoryId = typeCat.Id, Name = "Konut Projesi", SystemCode = "PROJECT", Order = 7 };

                context.DefinitionValues.AddRange(konutType, isYeriType, arsaType, binaType, devreMulkType, turistikTesisType, projeType);
                await context.SaveChangesAsync();
            }

            // 4.1 Alt Türler (SUB_TYPE_RESIDENTIAL vb.)
            var subTypeResCat = await context.DefinitionCategories.FirstOrDefaultAsync(c => c.SystemCode == "SUB_TYPE_RESIDENTIAL");
            var subTypeComCat = await context.DefinitionCategories.FirstOrDefaultAsync(c => c.SystemCode == "SUB_TYPE_COMMERCIAL");
            var subTypeLandCat = await context.DefinitionCategories.FirstOrDefaultAsync(c => c.SystemCode == "SUB_TYPE_LAND");
            var subTypeTourismCat = await context.DefinitionCategories.FirstOrDefaultAsync(c => c.SystemCode == "SUB_TYPE_TOURISM");

            if (subTypeResCat == null)
            {
                subTypeResCat = new DefinitionCategory { Name = "Konut Alt Türleri", SystemCode = "SUB_TYPE_RESIDENTIAL" };
                subTypeComCat = new DefinitionCategory { Name = "İş Yeri Alt Türleri", SystemCode = "SUB_TYPE_COMMERCIAL" };
                subTypeLandCat = new DefinitionCategory { Name = "Arsa Alt Türleri", SystemCode = "SUB_TYPE_LAND" };
                subTypeTourismCat = new DefinitionCategory { Name = "Turistik Tesis Alt Türleri", SystemCode = "SUB_TYPE_TOURISM" };
                
                context.DefinitionCategories.AddRange(subTypeResCat, subTypeComCat, subTypeLandCat, subTypeTourismCat);
                await context.SaveChangesAsync();
            }

            var daireExists = await context.DefinitionValues.AnyAsync(v => v.SystemCode == "RES_DAIRE");
            if (!daireExists)
            {
                // Update existing ones to avoid FK errors if they exist
                var existingDaireBina = await context.DefinitionValues.FirstOrDefaultAsync(v => v.SystemCode == "RES_DAIRE_BINA" || v.SystemCode == "RES_DAIRE_SITE");
                if (existingDaireBina != null) { existingDaireBina.Name = "Daire"; existingDaireBina.SystemCode = "RES_DAIRE"; }

                var existingVillaBina = await context.DefinitionValues.FirstOrDefaultAsync(v => v.SystemCode == "RES_VILLA_BINA" || v.SystemCode == "RES_VILLA_SITE");
                if (existingVillaBina != null) { existingVillaBina.Name = "Villa"; existingVillaBina.SystemCode = "RES_VILLA"; }

                await context.SaveChangesAsync();
            }

            var hasSubTypeValues = await context.DefinitionValues.AnyAsync(v => v.CategoryId == subTypeResCat.Id);
            if (!hasSubTypeValues)
            {
                context.DefinitionValues.AddRange(
                    // Konutlar
                    new DefinitionValue { CategoryId = subTypeResCat.Id, Name = "Daire", SystemCode = "RES_DAIRE", Order = 1 },
                    new DefinitionValue { CategoryId = subTypeResCat.Id, Name = "Residence", SystemCode = "RES_RESIDENCE", Order = 2 },
                    new DefinitionValue { CategoryId = subTypeResCat.Id, Name = "Müstakil Ev", SystemCode = "RES_MUSTAKIL", Order = 3 },
                    new DefinitionValue { CategoryId = subTypeResCat.Id, Name = "Villa", SystemCode = "RES_VILLA", Order = 4 },
                    new DefinitionValue { CategoryId = subTypeResCat.Id, Name = "Çiftlik Evi", SystemCode = "RES_CIFT", Order = 5 },
                    new DefinitionValue { CategoryId = subTypeResCat.Id, Name = "Yazlık", SystemCode = "RES_YAZLIK", Order = 6 },
                    // İş Yerleri
                    new DefinitionValue { CategoryId = subTypeComCat.Id, Name = "Ofis", SystemCode = "COM_OFIS", Order = 1 },
                    new DefinitionValue { CategoryId = subTypeComCat.Id, Name = "Dükkan & Mağaza", SystemCode = "COM_DUKKAN", Order = 2 },
                    new DefinitionValue { CategoryId = subTypeComCat.Id, Name = "Depo", SystemCode = "COM_DEPO", Order = 3 },
                    new DefinitionValue { CategoryId = subTypeComCat.Id, Name = "Fabrika", SystemCode = "COM_FABRIKA", Order = 4 },
                    new DefinitionValue { CategoryId = subTypeComCat.Id, Name = "Plaza Katı", SystemCode = "COM_PLAZA", Order = 5 },
                    new DefinitionValue { CategoryId = subTypeComCat.Id, Name = "İş Hanı Katı", SystemCode = "COM_ISHANI", Order = 6 },
                    // Arsalar
                    new DefinitionValue { CategoryId = subTypeLandCat.Id, Name = "İmarlı Arsa", SystemCode = "LND_IMARLI", Order = 1 },
                    new DefinitionValue { CategoryId = subTypeLandCat.Id, Name = "Tarla", SystemCode = "LND_TARLA", Order = 2 },
                    new DefinitionValue { CategoryId = subTypeLandCat.Id, Name = "Bahçe", SystemCode = "LND_BAHCE", Order = 3 },
                    // Turistik
                    new DefinitionValue { CategoryId = subTypeTourismCat.Id, Name = "Otel", SystemCode = "TOUR_OTEL", Order = 1 },
                    new DefinitionValue { CategoryId = subTypeTourismCat.Id, Name = "Pansiyon", SystemCode = "TOUR_PANSIYON", Order = 2 }
                );
                await context.SaveChangesAsync();
            }

            // 4.2 Kimden (FROM_WHOM)
            var fromWhomCat = await context.DefinitionCategories.FirstOrDefaultAsync(c => c.SystemCode == "FROM_WHOM");
            if (fromWhomCat == null)
            {
                fromWhomCat = new DefinitionCategory { Name = "Kimden", SystemCode = "FROM_WHOM" };
                context.DefinitionCategories.Add(fromWhomCat);
                await context.SaveChangesAsync();
            }

            var hasFromWhomValues = await context.DefinitionValues.AnyAsync(v => v.CategoryId == fromWhomCat.Id);
            if (!hasFromWhomValues)
            {
                context.DefinitionValues.AddRange(
                    new DefinitionValue { CategoryId = fromWhomCat.Id, Name = "Sahibinden", SystemCode = "OWNER", Order = 1 },
                    new DefinitionValue { CategoryId = fromWhomCat.Id, Name = "Emlak Ofisinden", SystemCode = "AGENCY", Order = 2 },
                    new DefinitionValue { CategoryId = fromWhomCat.Id, Name = "Danışmandan", SystemCode = "CONSULTANT", Order = 3 },
                    new DefinitionValue { CategoryId = fromWhomCat.Id, Name = "İnşaat Firmasından", SystemCode = "BUILDER", Order = 4 },
                    new DefinitionValue { CategoryId = fromWhomCat.Id, Name = "Bankadan", SystemCode = "BANK", Order = 5 }
                );
                await context.SaveChangesAsync();
            }



            // 5. Hizmet (Usta/Firma) Kategorileri
            var hasServiceCategories = await context.Set<ServiceCategory>().AnyAsync();
            if (!hasServiceCategories)
            {
                context.Set<ServiceCategory>().AddRange(
                    new ServiceCategory { Name = "Boya & Badana", Slug = "boya-badana", Icon = "ph-paint-roller" },
                    new ServiceCategory { Name = "Su Tesisatı", Slug = "su-tesisati", Icon = "ph-wrench" },
                    new ServiceCategory { Name = "Elektrik", Slug = "elektrik", Icon = "ph-lightning" },
                    new ServiceCategory { Name = "Fayans & Seramik", Slug = "fayans-seramik", Icon = "ph-squares-four" },
                    new ServiceCategory { Name = "Çatı & İzolasyon", Slug = "cati-izolasyon", Icon = "ph-house-line" },
                    new ServiceCategory { Name = "Güneş Enerjisi (Solar)", Slug = "gunes-enerjisi", Icon = "ph-sun" },
                    new ServiceCategory { Name = "Sigorta Şirketleri", Slug = "sigorta-sirketleri", Icon = "ph-shield-check" },
                    new ServiceCategory { Name = "Pimapen & Kepenk", Slug = "pimapen-kepenk", Icon = "ph-door" },
                    new ServiceCategory { Name = "Cam Balkon & Duşakabin", Slug = "cam-balkon-dusakabin", Icon = "ph-shower" }
                );
                await context.SaveChangesAsync();
            }

            // 6. Usta ve Kurumsal Firma Abonelik Paketleri (Komik Rakamlarla Sürümden Kazanma)
            var hasProviderPackages = await context.Set<SubscriptionPackage>().AnyAsync(p => p.TargetUserType == GMK360.Core.Entities.Identity.UserType.ServiceProvider);
            if (!hasProviderPackages)
            {
                context.Set<SubscriptionPackage>().AddRange(
                    new SubscriptionPackage 
                    { 
                        Name = "Yerel Usta Yıllık Paketi", 
                        Price = 299.00m, 
                        Period = PackagePeriod.Yearly, 
                        TargetUserType = GMK360.Core.Entities.Identity.UserType.ServiceProvider,
                        IsActive = true
                    },
                    new SubscriptionPackage 
                    { 
                        Name = "Kurumsal Firma Yıllık Paketi", 
                        Price = 899.00m, 
                        Period = PackagePeriod.Yearly, 
                        TargetUserType = GMK360.Core.Entities.Identity.UserType.ServiceProvider,
                        IsActive = true
                    }
                );
                await context.SaveChangesAsync();
            }

            // 7. Bireysel Abonelik Paketleri (Ev Sahibi Premium vb.)
            var hasIndividualPackages = await context.Set<SubscriptionPackage>().AnyAsync(p => p.TargetUserType == GMK360.Core.Entities.Identity.UserType.Individual);
            if (!hasIndividualPackages)
            {
                context.Set<SubscriptionPackage>().AddRange(
                    new SubscriptionPackage 
                    { 
                        Name = "Bireysel Premium (Çoklu Ev & AI)", 
                        Price = 199.00m, 
                        Period = PackagePeriod.Yearly, 
                        TargetUserType = GMK360.Core.Entities.Identity.UserType.Individual,
                        IsActive = true
                    }
                );
                await context.SaveChangesAsync();
            }

            await context.SaveChangesAsync();
        }
    }
}
