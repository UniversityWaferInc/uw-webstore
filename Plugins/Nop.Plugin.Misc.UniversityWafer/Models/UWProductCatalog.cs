using System.Collections.Generic;

namespace Nop.Plugin.Misc.UniversityWafer.Models
{
    public class UWProductCatalog
    {        
        public UWProductCatalog()
        {
            Prices = new[] { 1, 5, 10, 25, 50, 100, 200, 500 };
        }

        public IEnumerable<UWProductDetailsModel> UWProductDetails { get; set; }

        public IEnumerable<string> SpecificationAttributes { get; set; }

        public int[] Prices { get; set; }
    }
}
