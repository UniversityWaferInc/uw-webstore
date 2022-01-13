using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Shipping.Date;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Seo;
using System.Text.Json;
using Newtonsoft.Json;

namespace Nop.Services.CustomCode
{
    public partial class UwCatalogService : IUwCatalogService
    {
        private readonly INopDataProvider _nopDataProvider;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IDateRangeService _dateRangeSerivce;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IProductService _productService;
        public UwCatalogService(INopDataProvider nopDataProvider, ISpecificationAttributeService specificationAttributeService, IDateRangeService dateRangeSerivce, IUrlRecordService urlRecordService, IProductService productService)
        {
            _nopDataProvider = nopDataProvider;
            _specificationAttributeService = specificationAttributeService;
            _dateRangeSerivce = dateRangeSerivce;
            _urlRecordService = urlRecordService;
            _productService = productService;
        }

        //public async Task<UWProductCatalog> GetProductCatalog(int categoryId)
        public async Task<UWProductCatalog> GetProductCatalog(int categoryId)
        //public async Task<string> GetProductCatalog(int categoryId)
        {
            //comment for unused call of GetProductSpecificationAttributes
            //var specs = _specificationAttributeService.GetProductSpecificationAttributes().Where(s => s.DisplayOrder < 100).ToList();

            //for multiple query into one time execution or one trip to database
            //start
            //string cmdText = @"select ProductId, Quantity, Price from TierPrice;

            //                  SELECT  p.Id, p.Name, Sku, FullDescription, StockQuantity, DisableBuyButton, AllowedQuantities, ProductAvailabilityRangeId FROM Product p
            //                  INNER JOIN Product_Category_Mapping pcm ON pcm.ProductId = p.Id
            //                  INNER JOIN Category c ON c.Id = pcm.CategoryId
            //                  WHERE pcm.CategoryId = @CategoryId OR pcm.CategoryId IN (SELECT Id from Category WHERE ParentCategoryId = @CategoryId);
                                
            //                  select sa.Id, sa.Name, sao.Name AS 'ValueRaw', psm.ProductId from Product_SpecificationAttribute_Mapping psm
            //                  inner join SpecificationAttributeOption sao on sao.Id = psm.SpecificationAttributeOptionId
            //                  inner join SpecificationAttribute sa on sa.Id = sao.SpecificationAttributeId;";

            //List<TierPrice> tierPrices = new List<TierPrice>();
            List<UWProductDetailsModel> products = new List<UWProductDetailsModel>();
            //List<UWProductDetailsModel.ProductSpecificationsModel> productsSpec = new List<UWProductDetailsModel.ProductSpecificationsModel>();

            //var leadTimes = _dateRangeSerivce.GetAllProductAvailabilityRanges();
            //var leadTimes =await _dateRangeSerivce.GetAllProductAvailabilityRangesAsync();
            var ConnectionString = DataSettingsManager.LoadSettings().ConnectionString;

            using (SqlConnection dbConnection = new SqlConnection(ConnectionString))
            {
                //await dbConnection.Open();
                await dbConnection.OpenAsync();
                //using (SqlCommand dbCommand = new SqlCommand("SP_GetProductListByCategoryId", dbConnection))
                using (SqlCommand dbCommand = new SqlCommand("GetAllProductsByCategoryIdWitSpecificationAttribute", dbConnection))
                {
                    dbCommand.CommandType = CommandType.StoredProcedure;
                    dbCommand.Parameters.AddWithValue("@CategoryId", categoryId);
                    dbCommand.Connection = dbConnection;
                    SqlDataAdapter adp = new SqlDataAdapter(dbCommand);
                    DataSet ds = new DataSet();
                    adp.Fill(ds);

                    //release unused resources
                    //dbCommand.Dispose();
                    await dbCommand.DisposeAsync();
                    adp.Dispose();
                    //dbConnection.Close();
                    await dbConnection.CloseAsync();
                    //dbConnection.Dispose();
                    await dbConnection.DisposeAsync();

                    //get all query data at once
                    if (ds.Tables.Count > 0)
                    {
                        for(int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            UWProductDetailsModel obj = new UWProductDetailsModel();
                            obj.Id= Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                            obj.Sku = ds.Tables[0].Rows[i]["Sku"].ToString();
                            obj.AllowedQuantities = ds.Tables[0].Rows[i]["AllowedQuantities"].ToString();
                            obj.ProductAvailabilityRangeId = ds.Tables[0].Rows[i]["ProductAvailabilityRangeId"].ToString()!=null && ds.Tables[0].Rows[i]["ProductAvailabilityRangeId"].ToString()!=String.Empty ? Convert.ToInt32(ds.Tables[0].Rows[i]["ProductAvailabilityRangeId"]) :0;
                            obj.Diameter = ds.Tables[0].Rows[i]["Diameter"].ToString();
                            obj.Type = ds.Tables[0].Rows[i]["Type"].ToString();
                            obj.Dopant = ds.Tables[0].Rows[i]["Dopant"].ToString();
                            obj.Orientation = ds.Tables[0].Rows[i]["Orientation"].ToString();
                            obj.ResistivityMin = ds.Tables[0].Rows[i]["ResistivityMin"].ToString();
                            obj.ResistivityMax = ds.Tables[0].Rows[i]["ResistivityMax"].ToString();
                            obj.Thickness = ds.Tables[0].Rows[i]["Thickness"].ToString();
                            obj.Polish = ds.Tables[0].Rows[i]["Polish"].ToString();
                            obj.Grade = ds.Tables[0].Rows[i]["Grade"].ToString();
                            obj.StockQuantity = ds.Tables[0].Rows[i]["StockQuantity"].ToString()!=null && ds.Tables[0].Rows[i]["StockQuantity"].ToString()!=String.Empty ? Convert.ToInt32(ds.Tables[0].Rows[i]["StockQuantity"]):0;
                            obj.FullDescription = ds.Tables[0].Rows[i]["FullDescription"].ToString();
                            obj.OneUnitPrice = ds.Tables[0].Rows[i]["1"].ToString()!=null && ds.Tables[0].Rows[i]["1"].ToString() != String.Empty ? Convert.ToDecimal(ds.Tables[0].Rows[i]["1"]).ToString("c2") : String.Empty;
                            obj.FiveUnitPrice = ds.Tables[0].Rows[i]["5"].ToString() != null && ds.Tables[0].Rows[i]["5"].ToString() != String.Empty ? Convert.ToDecimal(ds.Tables[0].Rows[i]["5"]).ToString("c2") : String.Empty;
                            obj.TenUnitPrice = ds.Tables[0].Rows[i]["10"].ToString() != null && ds.Tables[0].Rows[i]["10"].ToString() != String.Empty ? Convert.ToDecimal(ds.Tables[0].Rows[i]["10"]).ToString("c2") : String.Empty;
                            obj.TwentyFiveUnitPrice = ds.Tables[0].Rows[i]["25"].ToString() != null && ds.Tables[0].Rows[i]["25"].ToString() != String.Empty ? Convert.ToDecimal(ds.Tables[0].Rows[i]["25"]).ToString("c2") : String.Empty;
                            obj.FiftyUnitPrice = ds.Tables[0].Rows[i]["50"].ToString() != null && ds.Tables[0].Rows[i]["50"].ToString() != String.Empty ? Convert.ToDecimal(ds.Tables[0].Rows[i]["50"]).ToString("c2") : String.Empty;
                            obj.HundredUnitPrice = ds.Tables[0].Rows[i]["100"].ToString() != null && ds.Tables[0].Rows[i]["100"].ToString() != String.Empty ? Convert.ToDecimal(ds.Tables[0].Rows[i]["100"]).ToString("c2") : String.Empty;
                            obj.TwoHundredUnitPrice = ds.Tables[0].Rows[i]["200"].ToString() != null && ds.Tables[0].Rows[i]["200"].ToString() != String.Empty ? Convert.ToDecimal(ds.Tables[0].Rows[i]["200"]).ToString("c2") : String.Empty;
                            obj.FiveHundredUnitPrice = ds.Tables[0].Rows[i]["500"].ToString() != null && ds.Tables[0].Rows[i]["500"].ToString() != String.Empty ? Convert.ToDecimal(ds.Tables[0].Rows[i]["500"]).ToString("c2") : String.Empty;
                            obj.LeadTime = ds.Tables[0].Rows[i]["LeadTime"].ToString();
                            var pro = await _productService.GetProductByIdAsync(obj.Id);
                            obj.ProductSeName = await _urlRecordService.GetSeNameAsync(pro);
                            products.Add(obj);
                        }

                        //for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        //{
                        //    TierPrice obj = new TierPrice();
                        //    obj.ProductId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProductId"]);
                        //    obj.Quantity = Convert.ToInt32(ds.Tables[0].Rows[i]["Quantity"]);
                        //    obj.Price = Convert.ToDecimal(ds.Tables[0].Rows[i]["Price"]);
                        //    tierPrices.Add(obj);
                        //}
                        //for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                        //{
                        //    UWProductDetailsModel.ProductSpecificationsModel obj = new UWProductDetailsModel.ProductSpecificationsModel();
                        //    obj.Id = Convert.ToInt32(ds.Tables[2].Rows[i]["Id"]);
                        //    obj.Name = ds.Tables[2].Rows[i]["Name"].ToString();
                        //    obj.ValueRaw = ds.Tables[2].Rows[i]["ValueRaw"].ToString();
                        //    obj.ProductId = Convert.ToInt32(ds.Tables[2].Rows[i]["ProductId"]);
                        //    productsSpec.Add(obj);
                        //}

                        //for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                        //{
                        //    UWProductDetailsModel obj = new UWProductDetailsModel();
                        //    obj.Id = Convert.ToInt32(ds.Tables[1].Rows[i]["Id"]);
                        //    obj.Sku = ds.Tables[1].Rows[i]["Sku"].ToString();
                        //    obj.FullDescription = ds.Tables[1].Rows[i]["FullDescription"].ToString();
                        //    obj.StockQuantity = Convert.ToInt32(ds.Tables[1].Rows[i]["StockQuantity"]);
                        //    obj.DisableBuyButton = Convert.ToBoolean(ds.Tables[1].Rows[i]["DisableBuyButton"]);
                        //    obj.ProductAvailabilityRangeId = Convert.ToInt32(ds.Tables[1].Rows[i]["ProductAvailabilityRangeId"]);
                        //    products.Add(obj);
                        //}
                    }

                    //release unused resource 
                    ds.Dispose();
                }
            }
            //UWProductCatalog res = new UWProductCatalog();
            //res.UWProductDetails = products;

            //string json = JsonConvert.SerializeObject(res.UWProductDetails);


            //List<UWProductCatalog> list = JsonConvert.DeserializeObject<List<UWProductCatalog>>(json);

            //return list;

            return new UWProductCatalog()
            {
                UWProductDetails = products,
            };
            //end


            //comment for multiple trip to database

            //var tierPriceQuery = "select ProductId, Quantity, Price from TierPrice";

            //var tierPricess = _nopDataProvider.Query<TierPrice>(tierPriceQuery);

            //var leadTimes = _dateRangeSerivce.GetAllProductAvailabilityRanges();

            ////var productQuery = @"SELECT p.Id, p.Name, Sku, FullDescription, StockQuantity, DisableBuyButton, AllowedQuantities, ProductAvailabilityRangeId FROM Product p
            ////                    INNER JOIN Product_Category_Mapping pcm ON pcm.ProductId = p.Id
            ////                    INNER JOIN Category c ON c.Id = pcm.CategoryId
            ////                    WHERE pcm.CategoryId = @CategoryId OR pcm.CategoryId IN (SELECT Id from Category WHERE ParentCategoryId = @CategoryId)";
            //var productQuery = @"SELECT p.Id, p.Name, Sku, FullDescription, StockQuantity, DisableBuyButton, AllowedQuantities, ProductAvailabilityRangeId FROM Product p
            //                    INNER JOIN Product_Category_Mapping pcm ON pcm.ProductId = p.Id
            //                    INNER JOIN Category c ON c.Id = pcm.CategoryId
            //                    WHERE pcm.CategoryId = @CategoryId OR pcm.CategoryId IN (SELECT Id from Category WHERE ParentCategoryId = @CategoryId)";

            //var parameters = new List<DataParameter>() { new DataParameter("CategoryId", categoryId) };

            //var productss = _nopDataProvider.Query<UWProductDetailsModel>(productQuery, parameters.ToArray());

            //var productsSpecQuery = @"select sa.Id, sa.Name, sao.Name AS 'ValueRaw', psm.ProductId from Product_SpecificationAttribute_Mapping psm
            //                          inner join SpecificationAttributeOption sao on sao.Id = psm.SpecificationAttributeOptionId
            //                          inner join SpecificationAttribute sa on sa.Id = sao.SpecificationAttributeId";

            //var productsSpecs = _nopDataProvider.Query<UWProductDetailsModel.ProductSpecificationsModel>(productsSpecQuery);

            //foreach (var product in products)
            //{
            //    var pro =await _productService.GetProductByIdAsync(product.Id);
            //    product.ProductSeName = await _urlRecordService.GetSeNameAsync(pro);
            //    //product.ProductSpecifications = productsSpec.Where(sp => sp.ProductId.Equals(product.Id)).ToDictionary(s => s.Name, s => s.ValueRaw);
            //    product.TierPrices = tierPrices.Where(tp => tp.ProductId.Equals(product.Id));
            //    if (leadTimes.Any(lt => lt.Id.Equals(product.ProductAvailabilityRangeId)))
            //    {
            //        product.LeadTime = leadTimes.First(lt => lt.Id.Equals(product.ProductAvailabilityRangeId)).Name;
            //    }
            //}
            //return new UWProductCatalog()
            //{
            //    UWProductDetails = products,
            //    SpecificationAttributes = productsSpec.Select(s => s.Name).Distinct()
            //};
        }

    }
}
