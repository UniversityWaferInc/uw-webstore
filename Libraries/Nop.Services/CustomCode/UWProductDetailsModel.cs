using Nop.Core.Domain.Catalog;
using System.Collections.Generic;

namespace Nop.Services.CustomCode
{
    public class UWProductDetailsModel
    {
        public int Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string FullDescription { get; set; }
        public string ShortDescription { get; set; }
        public string Diameter { get; set; }
        public string Type { get; set; }
        public string Dopant { get; set; }
        public string Orientation { get; set; }
        public string ResistivityMin { get; set; }
        public string ResistivityMax { get; set; }
        public string Thickness { get; set; }
        public string Polish { get; set; }
        public string Grade { get; set; }
        public string LeadTime { get; set; }
        //public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
        public int StockQuantity { get; set; }
        public bool DisableBuyButton { get; set; }
        public string AllowedQuantities { get;set;}
        public int ProductAvailabilityRangeId { get; set; }
        public object AddToCart { get; set; }
        public string ProductSeName { get; set; }
        public IEnumerable<TierPrice> TierPrices { get; set; }
        public string OneUnitPrice {get;set;}
        public string FiveUnitPrice { get;set;}
        public string TenUnitPrice { get;set;}
        public string TwentyFiveUnitPrice { get;set;}
        public string FiftyUnitPrice { get;set;}
        public string HundredUnitPrice { get;set;}
        public string TwoHundredUnitPrice { get;set;}
        public string FiveHundredUnitPrice { get;set;}








        //public int Id { get; set; }

        //public string Sku { get; set; }
        //public string Name { get; set; }
        //public string FullDescription { get; set; }
        //public string ShortDescription { get; set; }
        //public string Diameter { get; set; }
        //public string Type { get; set; }
        //public string Dopant { get; set; }
        //public string Orientation { get; set; }
        //public string ResistivityMin { get; set; }
        //public string ResistivityMax { get; set; }
        //public string Thickness { get; set; }
        //public string Polish { get; set; }
        //public string Grade { get; set; }
        //public string LeadTime { get; set; }
        //public int Quantity { get; set; }
        //public decimal UnitPrice { get; set; }
        //public decimal Total { get; set; }

        //public int StockQuantity { get; set; }

        //public bool DisableBuyButton { get; set; }

        //public int ProductAvailabilityRangeId { get; set; }

        //public IDictionary<string, string> ProductSpecifications { get; set; }

        //public IEnumerable<TierPrice> TierPrices { get; set; }

        //public object AddToCart { get; set; }
        //public string ProductSeName { get; set; }


        //public partial class ProductSpecificationsModel
        //{
        //    public int Id { get; set; }

        //    public string Name { get; set; }

        //    public string ValueRaw { get; set; }

        //    public int ProductId { get; set; }
        //}

    }
}
