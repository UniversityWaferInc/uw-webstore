using Nop.Core.Domain.Catalog;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.UniversityWafer.Models
{
    public class UWProductDetailsModel
    {
        public int Id { get; set; }

        public string Sku { get; set; }

        public string FullDescription { get; set; }

        public string LeadTime { get; set; }

        public int StockQuantity { get; set; }

        public bool DisableBuyButton { get; set; }

        public int ProductAvailabilityRangeId { get; set; }

        public IDictionary<string, string> ProductSpecifications { get; set; }

        public IEnumerable<TierPrice> TierPrices { get; set; }

        public object AddToCart { get; set; }

        public partial class ProductSpecificationsModel
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string ValueRaw { get; set; }

            public int ProductId { get; set; }
        }

    }
}
