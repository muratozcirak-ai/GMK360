using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Enums
{
    public enum POICategory
    {
        [Display(Name = "Eğitim")]
        Education = 1,

        [Display(Name = "Sağlık")]
        Health = 2,

        [Display(Name = "Ulaşım")]
        Transport = 3,

        [Display(Name = "Güvenlik")]
        Security = 4,

        [Display(Name = "Sosyal & Park")]
        SocialAndPark = 5,

        [Display(Name = "Alışveriş")]
        Shopping = 6
    }
}
