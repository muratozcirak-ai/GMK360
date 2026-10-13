using System.Collections.Generic;
using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class MaterialRecipe : BaseEntity
    {
        public string WorkItemCode { get; set; } // Örn: "DUVAR_YTONG_20"
        public string WorkItemName { get; set; } // Örn: "20'lik Ytong Duvar Örümü"
        public string BaseUnit { get; set; }     // Daima "m2" veya "m3"
        
        public virtual ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
    }
}
