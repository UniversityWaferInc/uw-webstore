using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    public class AllProductListModel
    {
        public string PictureThumbnailUrl { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int Id { get; set; }
        public bool Published { get; set; }
        public string FullDescription { get; set; }

        public string Diameter { get; set; }
        public string Type { get; set; }
        public string Dopant { get; set; }
        public string Orientation { get; set; }
        public string ResistivityMin { get; set; }
        public string ResistivityMax { get; set; }
        public string Thickness { get; set; }
        public string Polish { get; set; }
        public string Grade { get; set; }
        public string Resistivity { get; set; }
        public string Flat { get; set; }


        //public IList<ProductSpecificationAttributes> ProductSpecificationAttributes { get; set; }
    }
    //public class ProductSpecificationAttributes
    //{
    //    public string Diameter { get; set; }
    //    public string Type { get; set; }
    //    public string Dopant { get; set; }
    //    public string Orientation { get; set; }
    //    public string ResistivityMin { get; set; }
    //    public string ResistivityMax { get; set; }
    //    public string Thickness { get; set; }
    //    public string Polish { get; set; }
    //    public string Grade { get; set; }
    //    public string Resistivity { get; set; }
    //    public string Flat { get; set; }
    //}
}
