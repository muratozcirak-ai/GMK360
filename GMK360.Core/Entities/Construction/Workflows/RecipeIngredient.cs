using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class RecipeIngredient : BaseEntity
    {
        public int MaterialRecipeId { get; set; }
        public virtual MaterialRecipe MaterialRecipe { get; set; }
        
        public int MasterMaterialId { get; set; }
        public virtual MasterMaterial MasterMaterial { get; set; }
                public decimal Quantity { get; set; }    // 1 m2 için miktar: Örn: 6.5 (adet), 3.0 (kg)
    }
}
