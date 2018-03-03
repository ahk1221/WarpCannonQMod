using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarpCannonQMod
{
    public class CraftConfig
    {
        public int CraftAmount = 1;
        public List<CraftIngredient> Ingredients = new List<CraftIngredient>();
    }

    public class CraftIngredient
    {
        public string ItemName;
        public int Amount;
    }
}
