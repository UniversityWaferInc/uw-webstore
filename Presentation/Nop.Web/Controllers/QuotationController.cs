using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Http.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.CustomCode;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Customer;
using Nop.Web.Models.Quotation;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


namespace Nop.Web.Controllers
{
    public class QuotationController : Controller
    {

        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ShippingSettings _shippingSettings;
        private readonly ICheckoutModelFactory _checkoutModelFactory;
        private readonly ICurrencyService _currencyService;
        private readonly IDateRangeService _dateRangeService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly TaxSettings _taxSettings;
        private readonly IGiftCardService _giftCardService;
        private readonly IPaymentService _paymentService;
        private readonly ITaxService _taxService;
        private readonly IShippingService _shippingService;
        private readonly ICustomerService _customerService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IWebHelper _webHelper;
        private readonly IAddressService _addressService;
        private readonly IOrderService _orderService;
        private readonly ICustomNumberFormatter _customNumberFormatter;
        private readonly IRewardPointService _rewardPointService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IPdfService _pdfService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly OrderSettings _orderSettings;
        private readonly IHostingEnvironment _environment;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;

        #endregion

        #region Ctor

        public QuotationController(
            IShoppingCartModelFactory shoppingCartModelFactory,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IPermissionService permissionService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ShippingSettings shippingSettings,
            ICheckoutModelFactory checkoutModelFactory,
            ICurrencyService currencyService,
            ISpecificationAttributeService specificationAttributeService,
             IDateRangeService dateRangeService,
            IPriceFormatter priceFormatter,
            RewardPointsSettings rewardPointsSettings,
            TaxSettings taxSettings,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            IPaymentService paymentService,
            ITaxService taxService,
            ICustomerService customerService,
            IShippingService shippingService,
            IOrderProcessingService orderProcessingService,
            IPaymentPluginManager paymentPluginManager,
            IWebHelper webHelper,
            IAddressService addressService,
            IOrderService orderService,
            ICustomNumberFormatter customNumberFormatter,
            IRewardPointService rewardPointService,
            ILocalizationService localizationService,
             IWorkflowMessageService workflowMessageService,
            ICustomerModelFactory customerModelFactory,
            IPdfService pdfService,
            CatalogSettings catalogSettings,
            IProductService productService,
            IPriceCalculationService priceCalculationService,
            OrderSettings orderSettings,
            IProductModelFactory productModelFactory,
             LocalizationSettings localizationSettings,
            IHostingEnvironment environment)
        {
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _workContext = workContext;
            _permissionService = permissionService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _shippingSettings = shippingSettings;
            _checkoutModelFactory = checkoutModelFactory;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _dateRangeService = dateRangeService;
            _rewardPointsSettings = rewardPointsSettings;
            _taxSettings = taxSettings;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _paymentService = paymentService;
            _taxService = taxService;
            _customerService = customerService;
            _catalogSettings = catalogSettings;
            _shippingService = shippingService;
            _orderProcessingService = orderProcessingService;
            _paymentPluginManager = paymentPluginManager;
            _webHelper = webHelper;
            _addressService = addressService;
            _orderService = orderService;
            _customNumberFormatter = customNumberFormatter;
            _rewardPointService = rewardPointService;
            _localizationService = localizationService;
            _customerModelFactory = customerModelFactory;
            _pdfService = pdfService;
            _productService = productService;
            _localizationSettings = localizationSettings;
            _workflowMessageService = workflowMessageService;
            _specificationAttributeService = specificationAttributeService;
            _priceCalculationService = priceCalculationService;
            _productModelFactory = productModelFactory;
            _orderSettings = orderSettings;
            _environment = environment;
        }

        #endregion

        public async Task<IActionResult> RequestQuotation()
        {
            //var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);           
            var cart =await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, _storeContext.GetCurrentStore().Id);           
            var model = new ShoppingCartModel();
            //model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);
            model =await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);
            IList<ShoppingCartItem> list = cart;
            ViewBag.Cart = list;
            //string r = _priceFormatter.FormatPrice(1, true, false);
            string r =await _priceFormatter.FormatPriceAsync(1, true, false);
            string[] formatPrice = r.Split("1.00");
            string currency = formatPrice[0];
            ViewBag.Currency = currency;
            Address address = new Address();
            //var shippingMethodModel = _checkoutModelFactory.PrepareShippingMethodModel(cart, address);
            var shippingMethodModel = _checkoutModelFactory.PrepareShippingMethodModelAsync(cart, address);
            //ViewBag.ShippingMethodModel = shippingMethodModel.ShippingMethods;
            ViewBag.ShippingMethodModel =(await shippingMethodModel).ShippingMethods;
            //var isRegistered = _customerService.IsRegistered(_workContext.CurrentCustomer);
            var isRegistered = _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync());
            if (await isRegistered)
            {
                var customerInfo = new CustomerInfoModel();
                //customerInfo = _customerModelFactory.PrepareCustomerInfoModel(customerInfo, _workContext.CurrentCustomer, false);
                customerInfo =await _customerModelFactory.PrepareCustomerInfoModelAsync(customerInfo,await _workContext.GetCurrentCustomerAsync(), false);
                ViewBag.CustomerInfoModel = customerInfo;
            }

            return View(model);
        }
        public async Task<JsonResult> CalculateOrderTotal(decimal SubTotal, string ShippingOption)
        {
            try
            {
                decimal Total = 0;
                string shippingCharge = String.Empty;
                string result = String.Empty;
                //var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);
                var cart =await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
                var selectedName = String.Empty;
                var shippingRateComputationMethodSystemName = String.Empty;
                if (ShippingOption != null)
                {
                    var splittedOption = ShippingOption.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                    selectedName = splittedOption[0];
                    shippingRateComputationMethodSystemName = splittedOption[1];
                }
                else
                {
                    Total = Convert.ToDecimal(SubTotal);
                    shippingCharge = "0";
                    //result = _priceFormatter.FormatPrice(Total, true, false);
                    result =await _priceFormatter.FormatPriceAsync(Total, true, false);
                    return Json(new { Total, result, shippingCharge });
                }
                //var shippingOptions = _genericAttributeService.GetAttribute<List<ShippingOption>>(_workContext.CurrentCustomer,
                var shippingOptions =await _genericAttributeService.GetAttributeAsync<List<ShippingOption>>(await _workContext.GetCurrentCustomerAsync(),
                    //NopCustomerDefaults.OfferedShippingOptionsAttribute, _storeContext.CurrentStore.Id);
                    NopCustomerDefaults.OfferedShippingOptionsAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (shippingOptions == null || !shippingOptions.Any())
                {
                    //not found? let's load them using shipping service
                    //ippingOptions = _shippingService.GetShippingOptions(cart, _customerService.GetCustomerShippingAddress(_workContext.CurrentCustomer),
                    shippingOptions =(await _shippingService.GetShippingOptionsAsync(cart,await _customerService.GetCustomerShippingAddressAsync(await _workContext.GetCurrentCustomerAsync()),
                        //_workContext.CurrentCustomer, shippingRateComputationMethodSystemName, _storeContext.CurrentStore.Id).ShippingOptions.ToList();
                        await _workContext.GetCurrentCustomerAsync(), shippingRateComputationMethodSystemName,(await _storeContext.GetCurrentStoreAsync()).Id)).ShippingOptions.ToList();
                }
                else
                {
                    //loaded cached results. let's filter result by a chosen shipping rate computation method
                    shippingOptions = shippingOptions.Where(so => so.ShippingRateComputationMethodSystemName.Equals(shippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase))
                        .ToList();
                }

                var shippingOption = shippingOptions
                    .Find(so => !string.IsNullOrEmpty(so.Name) && so.Name.Equals(selectedName, StringComparison.InvariantCultureIgnoreCase));
                if (shippingOption == null)
                    throw new Exception("Selected shipping method can't be loaded");

                //save
                //_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, NopCustomerDefaults.SelectedShippingOptionAttribute, shippingOption, _storeContext.CurrentStore.Id);
                await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedShippingOptionAttribute, shippingOption, (await _storeContext.GetCurrentStoreAsync()).Id);
                Address address = new Address();

                //var shippingMethodModel = _checkoutModelFactory.PrepareShippingMethodModel(cart, address);
                var shippingMethodModel =await _checkoutModelFactory.PrepareShippingMethodModelAsync(cart, address);


                //foreach (var sm in shippingMethodModel.ShippingMethods)
                foreach (var sm in shippingMethodModel.ShippingMethods)
                {
                    if (sm.Selected)
                    {
                        Total = Convert.ToDecimal(sm.ShippingOption.Rate) + Convert.ToDecimal(SubTotal);
                        shippingCharge = sm.ShippingOption.Rate.ToString();
                    }
                }
                //result = _priceFormatter.FormatPrice(Total, true, false);
                result = await _priceFormatter.FormatPriceAsync(Total, true, false);

                return Json(new { Total, result, shippingCharge });
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost]
        public async Task<JsonResult> AddQuoteRequest(QuoteRequestModel quoteRequest)
        {
            try
            {
            var result =await _orderProcessingService.AddQuoteRequest(quoteRequest);
            var propertyInfo = result.GetType().GetProperty("OrderId");
            var value = propertyInfo.GetValue(result, null);

            var billingAddressProperyInfo = result.GetType().GetProperty("BillingAddressId");
            var billingAddressId = billingAddressProperyInfo.GetValue(result, null);

            var pdf =await PdfQuoteRequest(Convert.ToInt32(value));
            var by = pdf.GetType().GetProperty("FileContents");
            var fd = pdf.GetType().GetProperty("FileDownloadName");
            var fileDownloadName = fd.GetValue(pdf, null);
            var byval = by.GetValue(pdf, null);
            byte[] obj = (byte[])byval;
            string path = this._environment.WebRootPath;
            string pathWithFileName = path + "//files//exportimport//" + fileDownloadName;
            System.IO.File.WriteAllBytes(pathWithFileName, obj);
            //var customer = _workContext.CurrentCustomer;
            var customer =await _workContext.GetCurrentCustomerAsync();
            string attachmentFilePath = pathWithFileName;
            string attachmentFileName = Convert.ToString(fileDownloadName);
            int languageId = _localizationSettings.DefaultAdminLanguageId;
            if (pathWithFileName != null)
            {
                //if (customer.Email != null)
                if ((customer).Email != null)
                {
                    //var orderdetail = _orderService.GetOrderById(Convert.ToInt32(value));
                    var orderdetail =await _orderService.GetOrderByIdAsync(Convert.ToInt32(value));
                    await _workflowMessageService.SendPDFCompletedCustomerNotification(customer, languageId, attachmentFilePath, attachmentFileName, orderdetail);
                }
                else
                {
                    customer.Email = quoteRequest.Email;
                    customer.BillingAddressId = Convert.ToInt32(billingAddressId);
                    //var orderdetail = _orderService.GetOrderById(Convert.ToInt32(value));
                    var orderdetail =await _orderService.GetOrderByIdAsync(Convert.ToInt32(value));
                    await _workflowMessageService.SendPDFCompletedCustomerNotification(customer, languageId, attachmentFilePath, attachmentFileName, orderdetail);
                }
            }

            //after doing operation on file it deleted from above path
            // System.IO.File.Delete(pathWithFileName);
            return Json(new { pdf = pdf, filename = fileDownloadName });


            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public virtual async Task<IActionResult> PdfQuoteRequest(int orderId)
        {
            try
            {
                //a vendor should have access only to his products
                //var vendorId = 0;
                ////if (_workContext.CurrentVendor != null)
                //if (_workContext.GetCurrentVendorAsync()!= null)
                //{
                //    //vendorId = _workContext.CurrentVendor.Id;
                //    vendorId =(await _workContext.GetCurrentVendorAsync()).Id;
                //}

                ////var order = _orderService.GetOrderById(orderId);
                //var order =await _orderService.GetOrderByIdAsync(orderId);
                //var orders = new List<Order>
                //{
                //    order
                //};

                //a vendor should have access only to his products
                var vendorId = 0;
                if (await _workContext.GetCurrentVendorAsync() != null)
                {
                    vendorId = (await _workContext.GetCurrentVendorAsync()).Id;
                }

                var order = await _orderService.GetOrderByIdAsync(orderId);
                var orders = new List<Order>
                {
                    order
                };

                //var items = _orderService.GetOrderItems(order.Id).ToList();
                var items = await _orderService.GetOrderItemsAsync(order.Id);
                var productDetailsList = new List<UWProductDetailsModel>();
                foreach (var i in items)
                {
                    UWProductDetailsModel objItems = new UWProductDetailsModel();
                    //var product = _productService.GetProductById(i.ProductId);
                    var product = await _productService.GetProductByIdAsync(i.ProductId);
                    //model
                    //var productDetail = _productModelFactory.PrepareProductDetailsModel(product);
                    var productDetail = await _productModelFactory.PrepareProductDetailsModelAsync(product);


                    //var allSpec = _specificationAttributeService.GetSpecificationAttributes().Where(s => s.DisplayOrder < 100).ToList();
                    var allSpec = (await _specificationAttributeService.GetSpecificationAttributesAsync()).Where(s => s.DisplayOrder < 100).ToList();

                    objItems.Id = i.ProductId;
                    foreach (var spec in allSpec)
                    {

                        var finalSpec = "";
                        foreach(var pd in productDetail.ProductSpecificationModel.Groups)
                        {
                            if (pd.Attributes.Any(x => x.Id.Equals(spec.Id))){
                                finalSpec =pd.Attributes.First(x => x.Id.Equals(spec.Id)).Values.First().ValueRaw.ToString();
                            }
                        }
                        //if (productDetail.ProductSpecifications.Any(ps => ps.SpecificationAttributeId.Equals(spec.Id)))
                        //if ((productDetail.ProductSpecificationModel.Groups).Any(ps => ps.Attributes.Equals(spec.Name)))
                        //{

                        //    //finalSpec = productDetail.ProductSpecifications.First(p => p.SpecificationAttributeId.Equals(spec.Id)).ValueRaw;
                        //    finalSpec = productDetail.ProductSpecificationModel.Groups.First(p => p.Attributes.Equals(spec.Id)).ToString();//.ValueRaw;

                        //}
                        if (spec.Id == 6)
                        {
                            objItems.Diameter = finalSpec;
                        }
                        else if (spec.Id == 7)
                        {
                            objItems.Type = finalSpec;
                        }
                        else if (spec.Id == 8)
                        {
                            objItems.Dopant = finalSpec;
                        }
                        else if (spec.Id == 9)
                        {
                            objItems.Orientation = finalSpec;
                        }
                        else if (spec.Id == 10)
                        {
                            objItems.ResistivityMin = finalSpec;
                        }
                        else if (spec.Id == 11)
                        {
                            objItems.ResistivityMax = finalSpec;
                        }
                        else if (spec.Id == 12)
                        {
                            objItems.Thickness = finalSpec;
                        }
                        else if (spec.Id == 13)
                        {
                            objItems.Polish = finalSpec;
                        }
                        else if (spec.Id == 14)
                        {
                            objItems.Grade = finalSpec;
                        }
                    }
                    productDetailsList.Add(objItems);
                }



                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    //_pdfService.PrintQuoteRequestToPdf(stream, orders, productDetailsList, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : _workContext.WorkingLanguage.Id, vendorId);
                    await _pdfService.PrintQuoteRequestToPdf(stream, orders, productDetailsList, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 :(await _workContext.GetWorkingLanguageAsync()).Id, vendorId);
                    bytes = stream.ToArray();
                }

                return File(bytes, MimeTypes.ApplicationPdf, $"order_{order.Id}.pdf");


            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public virtual byte[] Validate(string downloadUrl)
        {
            using (var client = new HttpClient())
            {
                using (var result = client.GetAsync(downloadUrl).Result)
                {
                    if (result.IsSuccessStatusCode)
                    {
                        return result.Content.ReadAsByteArrayAsync().Result;
                    }
                }
            }
            return null;
        }
        public IActionResult Download(string filename)
        {

            var hostname = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            var path = hostname + "//files//exportimport//" + filename;
            var bytes = Validate(path);
            if (bytes == null)
                return Content("Download data is not available any more.");


            var contentType1 = MimeTypes.ApplicationOctetStream;
            return new FileContentResult(bytes, contentType1) { FileDownloadName = filename };
        }


        #region DropDownList
        public async Task<JsonResult> DdlCountry()
        {
            try
            {
                //var countryList = _countryService.GetAllCountriesForBilling();
                var countryList =await _countryService.GetAllCountriesForBillingAsync();
                return Json(countryList);

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<JsonResult> DdlState(int ID)
        {
            try
            {
                //var stateList = _stateProvinceService.GetStateProvincesByCountryId(ID);
                var stateList =await _stateProvinceService.GetStateProvincesByCountryIdAsync(ID);
                return Json(stateList);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion


    }
}
