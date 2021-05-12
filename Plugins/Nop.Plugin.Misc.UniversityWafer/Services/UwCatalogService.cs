using LinqToDB.Data;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Misc.UniversityWafer.Models;
using Nop.Services.Catalog;
using Nop.Services.Shipping.Date;
using System.Collections.Generic;
using System.Linq;
using static Nop.Plugin.Misc.UniversityWafer.Models.UWProductDetailsModel;

namespace Nop.Plugin.Misc.UniversityWafer.Services
{
    public class UwCatalogService
    {
        private readonly INopDataProvider _nopDataProvider;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IDateRangeService _dateRangeSerivce;
        public UwCatalogService(INopDataProvider nopDataProvider, ISpecificationAttributeService specificationAttributeService, IDateRangeService dateRangeSerivce)
        {
            _nopDataProvider = nopDataProvider;
            _specificationAttributeService = specificationAttributeService;
            _dateRangeSerivce = dateRangeSerivce;
        }

        public UWProductCatalog GetProductCatalog(int categoryId)
        {
            var specs = _specificationAttributeService.GetProductSpecificationAttributes().Where(s => s.DisplayOrder < 100).ToList();

            var tierPriceQuery = "select ProductId, Quantity, Price from TierPrice";

            var tierPrices = _nopDataProvider.Query<TierPrice>(tierPriceQuery);

            var leadTimes = _dateRangeSerivce.GetAllProductAvailabilityRanges();

            var productQuery = @"SELECT p.Id, p.Name, Sku, FullDescription, StockQuantity, DisableBuyButton, AllowedQuantities, ProductAvailabilityRangeId FROM Product p
                                INNER JOIN Product_Category_Mapping pcm ON pcm.ProductId = p.Id
                                INNER JOIN Category c ON c.Id = pcm.CategoryId
                                WHERE pcm.CategoryId = @CategoryId OR pcm.CategoryId IN (SELECT Id from Category WHERE ParentCategoryId = @CategoryId)";

            var parameters = new List<DataParameter>() { new DataParameter("CategoryId", categoryId) };

            var products = _nopDataProvider.Query<UWProductDetailsModel>(productQuery, parameters.ToArray());

            var productsSpecQuery = @"select sa.Id, sa.Name, sao.Name AS 'ValueRaw', psm.ProductId from Product_SpecificationAttribute_Mapping psm
                                      inner join SpecificationAttributeOption sao on sao.Id = psm.SpecificationAttributeOptionId
                                      inner join SpecificationAttribute sa on sa.Id = sao.SpecificationAttributeId";

            var productsSpec = _nopDataProvider.Query<UWProductDetailsModel.ProductSpecificationsModel>(productsSpecQuery);

            foreach (var product in products)
            {
                product.ProductSpecifications = productsSpec.Where(sp => sp.ProductId.Equals(product.Id)).ToDictionary(s => s.Name, s => s.ValueRaw);
                product.TierPrices = tierPrices.Where(tp => tp.ProductId.Equals(product.Id));                
                if (leadTimes.Any(lt => lt.Id.Equals(product.ProductAvailabilityRangeId)))
                {
                    product.LeadTime = leadTimes.First(lt => lt.Id.Equals(product.ProductAvailabilityRangeId)).Name;
                }
            }
            return new UWProductCatalog() {
               UWProductDetails = products,
               SpecificationAttributes = productsSpec.Select(s => s.Name).Distinct()
            };
        }

    }
}
