using Nop.Plugin.Payments.UWPayflowPro.Domains;
using Nop.Plugin.Payments.UWPayflowPro.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
namespace Nop.Plugin.Payments.UWPayflowPro.Controllers
{
    public class PayflowProPaymentController : BasePaymentController
    {
        private readonly IHttpContextAccessor a;
        private readonly ILocalizationService b;
        private readonly IGenericAttributeService c;
        private readonly IStoreContext d;
        private readonly IPaymentPluginManager e;
        private readonly IOrderService f;
        private readonly IProductService g;
        private readonly IOrderProcessingService h;
        private readonly IOrderTotalCalculationService i;
        private readonly IShoppingCartService j;
        private readonly IWorkContext k;
        private readonly IWebHelper l;
        private readonly ILogger m;
        private readonly PayflowProSettings n;
        private bool o;
        private readonly FNSLogger p;

        public PayflowProPaymentController(
          IHttpContextAccessor httpContextAccessor,
          ILocalizationService localizationService,
          IGenericAttributeService genericAttributeService,
          IStoreContext storeContext,
          IPaymentPluginManager paymentPluginManager,
          IOrderService orderService,
          IProductService productService,
          IOrderProcessingService orderProcessingService,
          IOrderTotalCalculationService orderTotalCalculationService,
          IShoppingCartService shoppingCartService,
          IWorkContext workContext,
          IWebHelper webHelper,
          ILogger logger,
          PayflowProSettings payflowProSettings)
        {
            
            this.g = productService;
            this.h = orderProcessingService;
            this.i = orderTotalCalculationService;
            this.j = shoppingCartService;
            this.k = workContext;
            this.l = webHelper;
            this.m = logger;
            this.n = payflowProSettings;
            this.o = this.n.showDebugInfo;
        }

        private void a(string message)
        {
            if (!this.o || this.p == null)
                return;
            this.p.LogMessage(message);
        }

        private string b(string viewname)
        {
            return global::a.a("\xF08B\xF0DA\xF0A5\xF099\xF080\xF092\xF09C\xF09B\xF086\xF0DA\xF0B3\xF09A\xF08D\xF0BB\xF090\xF081\xF0A6\xF09A\xF093\xF081\xF0DB\xF0A5\xF094\xF08C\xF093\xF099\xF09A\xF082\xF0A5\xF087\xF09A\xF0DA\xF0A3\xF09C\xF090\xF082\xF086\xF0DA\xF0A5\xF094\xF08C\xF093\xF099\xF09A\xF082\xF0A5\xF087\xF09A\xF0DA", 61556) + viewname + global::a.a("\xEA79\xEA34\xEA24\xEA3F\xEA23\xEA3A\xEA3B", 59927);
        }

        private string c(string key, IQueryCollection form)
        {
            return (form.get_Keys().Contains(key) ? form.get_Item(key).ToString() : (string)this.l.QueryString<string>(key)) ?? string.Empty;
        }

        private void d(IList<ShoppingCartItem> shoppingCart, out Decimal? orderTotal)
        {
            Decimal num1;
            List<Discount> discountList;
            List<AppliedGiftCard> appliedGiftCardList;
            int num2;
            Decimal num3;
            orderTotal = this.i.GetShoppingCartTotal(shoppingCart, ref num1, ref discountList, ref appliedGiftCardList, ref num2, ref num3, new bool?(), true);
        }

        private Order e(Guid orderGuid, out IList<string> warnings)
        {
            warnings = (IList<string>)new List<string>();
            if (this.o)
                this.a(string.Format(global::a.a("\xF3B1\xF380\xF397\xF393\xF386\xF397\xF3BD\xF380\xF396\xF397\xF380\xF3DC", 62448)));
            IList<ShoppingCartItem> shoppingCart = this.j.GetShoppingCart(this.k.get_CurrentCustomer(), new ShoppingCartType?((ShoppingCartType)1), ((BaseEntity)this.d.get_CurrentStore()).get_Id(), new int?(), new DateTime?(), new DateTime?());
            if (!((IEnumerable<ShoppingCartItem>)shoppingCart).Any<ShoppingCartItem>())
            {
                warnings.Add(this.b.GetResource(global::a.a("\xF334\xF31D\xF30A\xF33C\xF317\xF306\xF321\xF31D\xF314\xF306\xF35C\xF322\xF31E\xF307\xF315\xF31B\xF31C\xF35C\xF322\xF313\xF30B\xF31F\xF317\xF31C\xF306\xF301\xF35C\xF322\xF313\xF30B\xF314\xF31E\xF31D\xF305\xF322\xF300\xF31D\xF35C\xF337\xF300\xF300\xF31D\xF300\xF35C\xF337\xF31F\xF302\xF306\xF30B\xF321\xF31A\xF31D\xF302\xF302\xF31B\xF31C\xF315\xF331\xF313\xF300\xF306", 62320)));
                return (Order)null;
            }
            Decimal? orderTotal;
            this.d(shoppingCart, out orderTotal);
            if (!orderTotal.HasValue)
            {
                warnings.Add(this.b.GetResource(global::a.a("\xE8B6\xE8A1\xE897\xE8BC\xE8AD\xE88A\xE8B6\xE8BF\xE8AD\xE8F7\xE889\xE8B5\xE8AC\xE8BE\xE8B0\xE8B7\xE8F7\xE889\xE8B8\xE8A0\xE8B4\xE8BC\xE8B7\xE8AD\xE8AA\xE8F7\xE889\xE8B8\xE8A0\xE8BF\xE8B5\xE8B6\xE8AE\xE889\xE8AB\xE8B6\xE8F7\xE89C\xE8AB\xE8AB\xE8B6\xE8AB\xE8F7\xE89C\xE8B4\xE8A9\xE8AD\xE8A0\xE896\xE8AB\xE8BD\xE8BC\xE8AB\xE88D\xE8B6\xE8AD\xE8B8\xE8B5", 59585)));
                return (Order)null;
            }
            try
            {
                this.c.SaveAttribute<string>((BaseEntity)this.k.get_CurrentCustomer(), NopCustomerDefaults.get_SelectedPaymentMethodAttribute(), (M0)PluginLog.SystemName, ((BaseEntity)this.d.get_CurrentStore()).get_Id());
                ProcessPaymentRequest processPaymentRequest = new ProcessPaymentRequest();
                processPaymentRequest.set_StoreId(((BaseEntity)this.d.get_CurrentStore()).get_Id());
                processPaymentRequest.set_CustomerId(((BaseEntity)this.k.get_CurrentCustomer()).get_Id());
                processPaymentRequest.set_PaymentMethodSystemName(PluginLog.SystemName);
                processPaymentRequest.set_OrderGuid(orderGuid);
                PlaceOrderResult placeOrderResult = this.h.PlaceOrder(processPaymentRequest);
                if (placeOrderResult.get_Success())
                    return placeOrderResult.get_PlacedOrder();
                warnings.Add(this.b.GetResource(global::a.a("\xF888\xF8A1\xF8B6\xF880\xF8AB\xF8BA\xF89D\xF8A1\xF8A8\xF8BA\xF8E0\xF89E\xF8A2\xF8BB\xF8A9\xF8A7\xF8A0\xF8E0\xF89E\xF8AF\xF8B7\xF8A3\xF8AB\xF8A0\xF8BA\xF8BD\xF8E0\xF89E\xF8AF\xF8B7\xF8A8\xF8A2\xF8A1\xF8B9\xF89E\xF8BC\xF8A1\xF8E0\xF88B\xF8BC\xF8BC\xF8A1\xF8BC\xF8E0\xF88D\xF8BC\xF8AB\xF8AF\xF8BA\xF8AB\xF8AA\xF881\xF8BC\xF8AA\xF8AB\xF8BC", 63690)));
                foreach (string error in (IEnumerable<string>)placeOrderResult.get_Errors())
                    warnings.Add(string.Format(global::a.a("\xED04\xED4F\xED02", 60776), (object)error));
            }
            catch (Exception ex)
            {
                warnings.Add(string.Format(this.b.GetResource(global::a.a("\xE9B3\xE99A\xE98D\xE9BB\xE990\xE981\xE9A6\xE99A\xE993\xE981\xE9DB\xE9A5\xE999\xE980\xE992\xE99C\xE99B\xE9DB\xE9A5\xE994\xE98C\xE998\xE990\xE99B\xE981\xE986\xE9DB\xE9A5\xE994\xE98C\xE993\xE999\xE99A\xE982\xE9A5\xE987\xE99A\xE9DB\xE9B0\xE987\xE987\xE99A\xE987\xE9DB\xE9B0\xE98D\xE996\xE990\xE985\xE981\xE99C\xE99A\xE99B", 59780)), ex.InnerException != null ? (object)ex.InnerException.Message : (object)ex.Message));
            }
            return (Order)null;
        }

        private IList<string> f(IQueryCollection parameters, out int orderId)
        {
            orderId = 0;
            IList<string> warnings = (IList<string>)new List<string>();
            StringBuilder stringBuilder1 = new StringBuilder();
            try
            {
                if (this.o)
                {
                    stringBuilder1.AppendLine(global::a.a("\xE68F\xE6BE\xE6A6\xE6B9\xE6B3\xE6B0\xE6A8\xE68F\xE6AD\xE6B0\xE6F1\xE6FF\xE689\xE6BE\xE6B3\xE6B6\xE6BB\xE6BE\xE6AB\xE6BA\xE68D\xE6BA\xE6AF\xE6B3\xE6BE\xE6A6", 59037));
                    foreach (KeyValuePair<string, StringValues> parameter in (IEnumerable<KeyValuePair<string, StringValues>>)parameters)
                        stringBuilder1.AppendLine(string.Format(global::a.a("\xF0BB\xF0D0\xF0FE\xF0E2\xF0A6\xF0E0\xF0AB\xF0E6\xF0B7\xF0BB\xF0CD\xF0FA\xF0F7\xF0EE\xF0FE\xF0A6\xF0E0\xF0AA\xF0E6", 61451), (object)parameter.Key, (object)parameter.Value));
                    this.a(stringBuilder1.ToString());
                }
                if (string.IsNullOrEmpty(this.c(global::a.a("\xF06D\xF07A\xF06C\xF06A\xF073\xF06B", 61502), parameters)) || string.IsNullOrEmpty(this.c(global::a.a("\xF89B\xF885\xF899\xF88E\xF88D", 63554), parameters)) || string.IsNullOrEmpty(this.c(global::a.a("\xEC9F\xEC92\xEC9B\xEC8E", 60482), parameters)))
                {
                    warnings.Add(this.b.GetResource(global::a.a("\xE368\xE341\xE356\xE360\xE34B\xE35A\xE37D\xE341\xE348\xE35A\xE300\xE37E\xE342\xE35B\xE349\xE347\xE340\xE300\xE37E\xE34F\xE357\xE343\xE34B\xE340\xE35A\xE35D\xE300\xE37E\xE34F\xE357\xE348\xE342\xE341\xE359\xE37E\xE35C\xE341\xE300\xE36B\xE35C\xE35C\xE341\xE35C\xE300\xE379\xE35C\xE341\xE340\xE349\xE37C\xE34B\xE35D\xE35E\xE341\xE340\xE35D\xE34B", 58154)));
                    return warnings;
                }
                string str1 = this.c(global::a.a("\xF06D\xF07A\xF06C\xF06A\xF073\xF06B", 61502), parameters);
                string str2 = this.c(global::a.a("\xF89B\xF885\xF899\xF88E\xF88D", 63554), parameters);
                string str3 = this.c(global::a.a("\xEC9F\xEC92\xEC9B\xEC8E", 60482), parameters);
                string str4 = this.c(global::a.a("\xF1B0\xF1A7\xF1B1\xF1B2\xF1AF\xF1B1\xF1A5", 61920), parameters);
                string str5 = this.c(global::a.a("\xEBB8\xEBAC\xEBAD\xEBB1\xEBBA\xEBB6\xEBBD\xEBBC", 60345), parameters);
                if (str1 != global::a.a("\xEACF", 60136))
                {
                    warnings.Add(string.Format(global::a.a("\xF8AE\xF898\xF88B\xF897\xF890\xF897\xF89E\xF8D7\xF8D9\xF8A9\xF898\xF880\xF89F\xF895\xF896\xF88E\xF8D9\xF8A9\xF88B\xF896\xF8D9\xF889\xF898\xF880\xF894\xF89C\xF897\xF88D\xF8D9\xF894\xF89C\xF88D\xF891\xF896\xF89D\xF8D7\xF8D9\xF8AE\xF88B\xF896\xF897\xF89E\xF8D9\xF8AD\xF88B\xF898\xF897\xF88A\xF898\xF89A\xF88D\xF890\xF896\xF897\xF8D9\xF8D1\xF88B\xF89C\xF88A\xF88C\xF895\xF88D\xF8C4\xF882\xF8C9\xF884\xF8D5\xF8D9\xF88B\xF89C\xF88A\xF889\xF894\xF88A\xF89E\xF8C4\xF882\xF8C8\xF884\xF8D0\xF8D7", 63513), (object)str1, (object)str4));
                    return warnings;
                }
                string input = this.c(global::a.a("\xF794\xF782\xF784\xF792\xF795\xF782\xF793\xF788\xF78C\xF782\xF789\xF78E\xF783", 63366), parameters);
                Guid result1;
                if (!Guid.TryParse(input, out result1))
                {
                    warnings.Add(string.Format(this.b.GetResource(global::a.a("\xEB91\xEBB8\xEBAF\xEB99\xEBB2\xEBA3\xEB84\xEBB8\xEBB1\xEBA3\xEBF9\xEB87\xEBBB\xEBA2\xEBB0\xEBBE\xEBB9\xEBF9\xEB87\xEBB6\xEBAE\xEBBA\xEBB2\xEBB9\xEBA3\xEBA4\xEBF9\xEB87\xEBB6\xEBAE\xEBB1\xEBBB\xEBB8\xEBA0\xEB87\xEBA5\xEBB8\xEBF9\xEB92\xEBA5\xEBA5\xEBB8\xEBA5\xEBF9\xEB80\xEBA5\xEBB8\xEBB9\xEBB0\xEB98\xEBA5\xEBB3\xEBB2\xEBA5\xEB9E\xEBB3", 60311)), (object)input));
                    return warnings;
                }
                Order order;
                if (this.n.CreateOrderMode == CreateOrderMode.BeforePayment)
                {
                    order = this.f.GetOrderByGuid(result1);
                }
                else
                {
                    order = this.e(result1, out warnings);
                    if (warnings.Any<string>())
                        return warnings;
                }
                if (order == null)
                {
                    warnings.Add(string.Format(this.b.GetResource(global::a.a("\xEB91\xEBB8\xEBAF\xEB99\xEBB2\xEBA3\xEB84\xEBB8\xEBB1\xEBA3\xEBF9\xEB87\xEBBB\xEBA2\xEBB0\xEBBE\xEBB9\xEBF9\xEB87\xEBB6\xEBAE\xEBBA\xEBB2\xEBB9\xEBA3\xEBA4\xEBF9\xEB87\xEBB6\xEBAE\xEBB1\xEBBB\xEBB8\xEBA0\xEB87\xEBA5\xEBB8\xEBF9\xEB92\xEBA5\xEBA5\xEBB8\xEBA5\xEBF9\xEB80\xEBA5\xEBB8\xEBB9\xEBB0\xEB98\xEBA5\xEBB3\xEBB2\xEBA5\xEB9E\xEBB3", 60311)), (object)input));
                    return warnings;
                }
                orderId = ((BaseEntity)order).get_Id();
                if (!this.h.CanMarkOrderAsPaid(order))
                    return warnings;
                string s = this.c(global::a.a("\xE9EE\xE9E2\xE9FB", 59822), parameters);
                Decimal result2;
                if (!Decimal.TryParse(s, NumberStyles.Number, (IFormatProvider)new CultureInfo(global::a.a("\xEBAE\xEBA5\xEBE6\xEBBE\xEBB8", 60162)).NumberFormat, out result2) || Math.Round(order.get_OrderTotal(), 2) != result2)
                {
                    warnings.Add(this.b.GetResource(string.Format(global::a.a("\xEAAD\xEA84\xEA93\xEAA5\xEA8E\xEA9F\xEAB8\xEA84\xEA8D\xEA9F\xEAC5\xEABB\xEA87\xEA9E\xEA8C\xEA82\xEA85\xEAC5\xEABB\xEA8A\xEA92\xEA86\xEA8E\xEA85\xEA9F\xEA98\xEAC5\xEABB\xEA8A\xEA92\xEA8D\xEA87\xEA84\xEA9C\xEABB\xEA99\xEA84\xEAC5\xEAAE\xEA99\xEA99\xEA84\xEA99\xEAC5\xEABC\xEA99\xEA84\xEA85\xEA8C\xEAA4\xEA99\xEA8F\xEA8E\xEA99\xEABF\xEA84\xEA9F\xEA8A\xEA87", 60002), (object)order.get_OrderTotal(), (object)s)));
                    return warnings;
                }
                StringBuilder stringBuilder2 = new StringBuilder();
                stringBuilder2.AppendLine(global::a.a("\xF3A9\xF398\xF380\xF39F\xF395\xF396\xF38E\xF3D9\xF3A9\xF38B\xF396\xF3D9\xF3AA\xF38C\xF39A\xF39A\xF39C\xF38A\xF38A\xF3C3", 62345));
                foreach (KeyValuePair<string, StringValues> parameter in (IEnumerable<KeyValuePair<string, StringValues>>)parameters)
                    stringBuilder2.AppendLine(string.Format(global::a.a("\xE3A0\xE3EB\xE3A6\xE3E6\xE3A0\xE3EA\xE3A6", 58187), (object)parameter.Key, (object)parameter.Value));
                IOrderService f = this.f;
                OrderNote orderNote = new OrderNote();
                orderNote.set_OrderId(((BaseEntity)order).get_Id());
                orderNote.set_Note(stringBuilder2.ToString());
                orderNote.set_DisplayToCustomer(false);
                orderNote.set_CreatedOnUtc(DateTime.UtcNow);
                f.InsertOrderNote(orderNote);
                if (((IEnumerable<Product>)this.g.GetProductsByIds(((IEnumerable<OrderItem>)this.f.GetOrderItems(((BaseEntity)order).get_Id(), new bool?(), new bool?(), 0)).Select<OrderItem, int>((Func<OrderItem, int>)(o => o.get_ProductId())).ToArray<int>())).Any<Product>((Func<Product, bool>)(p => p.get_IsRecurring())))
                    order.set_SubscriptionTransactionId(str2);
                else if (str3 == global::a.a("\xE4A6", 58405))
                {
                    order.set_CaptureTransactionId(str2);
                    order.set_CaptureTransactionResult(str4);
                }
                else
                {
                    order.set_AuthorizationTransactionId(str2);
                    order.set_AuthorizationTransactionResult(str4);
                    order.set_AuthorizationTransactionCode(str5);
                }
                this.f.UpdateOrder(order);
                if (this.h.CanMarkOrderAsPaid(order))
                {
                    if (str3 == global::a.a("\xE4A6", 58405))
                        this.h.MarkOrderAsPaid(order);
                    else
                        this.h.MarkAsAuthorized(order);
                }
                orderId = ((BaseEntity)order).get_Id();
                return warnings;
            }
            catch (Exception ex)
            {
                orderId = 0;
                warnings.Add(string.Format(this.b.GetResource(global::a.a("\xF024\xF00D\xF01A\xF02C\xF007\xF016\xF031\xF00D\xF004\xF016\xF04C\xF032\xF00E\xF017\xF005\xF00B\xF00C\xF04C\xF032\xF003\xF01B\xF00F\xF007\xF00C\xF016\xF011\xF04C\xF032\xF003\xF01B\xF004\xF00E\xF00D\xF015\xF032\xF010\xF00D\xF04C\xF027\xF010\xF010\xF00D\xF010\xF04C\xF027\xF01A\xF001\xF007\xF012\xF016\xF00B\xF00D\xF00C", 61440)), ex.InnerException != null ? (object)ex.InnerException.Message : (object)ex.Message));
                return warnings;
            }
        }

        public IActionResult ErrorMessageString(string errorMessage)
        {
            ErrorMessageModel errorMessageModel = new ErrorMessageModel();
            errorMessageModel.Warnings.Add(errorMessage);
            return (IActionResult)((Controller)this).View(this.b(global::a.a("\xF11A\xF12D\xF12D\xF130\xF12D\xF116\xF131\xF139\xF130", 61784)), (object)errorMessageModel);
        }

        public IActionResult ReturnHandler(IpnModel model)
        {
            if (!(((IPluginManager<IPaymentMethod>)this.e).LoadPluginBySystemName(PluginLog.SystemName, (Customer)null, 0) is PayflowProProcessor payflowProProcessor) || !this.e.IsPluginActive((IPaymentMethod)payflowProProcessor))
                throw new NopException(global::a.a("\xE1DF\xE1EE\xE1F6\xE1E9\xE1E3\xE1E0\xE1F8\xE1AF\xE1DF\xE1FD\xE1E0\xE1AF\xE1E2\xE1E0\xE1EB\xE1FA\xE1E3\xE1EA\xE1AF\xE1EC\xE1EE\xE1E1\xE1E1\xE1E0\xE1FB\xE1AF\xE1ED\xE1EA\xE1AF\xE1E3\xE1E0\xE1EE\xE1EB\xE1EA\xE1EB", 57742));
            int orderId;
            IList<string> source = this.f(((ControllerBase)this).get_Request().get_Query(), out orderId);
            if (orderId != 0)
                return (IActionResult)((ControllerBase)this).RedirectToRoute(global::a.a("\xE81C\xE837\xE83A\xE83C\xE834\xE830\xE82A\xE82B\xE81C\xE830\xE832\xE82F\xE833\xE83A\xE82B\xE83A\xE83B", 59423), (object)new
                {
                    orderId = orderId
                });
            if (!source.Any<string>())
                return (IActionResult)((ControllerBase)this).RedirectToAction(global::a.a("\xE2B0\xE297\xE29D\xE29C\xE281", 57993), global::a.a("\xE4AF\xE488\xE48A\xE482", 58598), (object)new
                {
                    area = ""
                });
            StringBuilder stringBuilder1 = new StringBuilder();
            foreach (string str in (IEnumerable<string>)source)
                stringBuilder1.AppendLine(string.Format(global::a.a("\xE5D5\xE5D5\xE5D5\xE58E\xE5C5\xE588", 58708), (object)str));
            this.m.Error(stringBuilder1.ToString(), (Exception)null, (Customer)null);
            if (this.o)
            {
                StringBuilder stringBuilder2 = new StringBuilder();
                stringBuilder2.AppendLine(global::a.a("\xEEFB\xEECC\xEECC\xEED1\xEECC\xEECD\xEE90\xEE9E\xEEEC\xEEDB\xEECA\xEECB\xEECC\xEED0\xEEF6\xEEDF\xEED0\xEEDA\xEED2\xEEDB\xEECC", 61114));
                foreach (string str in (IEnumerable<string>)source)
                    stringBuilder2.AppendLine(string.Format(global::a.a("\xE5D5\xE5D5\xE5D5\xE58E\xE5C5\xE588", 58708), (object)str));
                this.a(stringBuilder2.ToString());
            }
            return (IActionResult)((Controller)this).View(this.b(global::a.a("\xECB8\xEC8F\xEC8F\xEC92\xEC8F\xECB4\xEC93\xEC9B\xEC92", 60668)), (object)new ErrorMessageModel()
            {
                Warnings = source
            });
        }
    }
}
