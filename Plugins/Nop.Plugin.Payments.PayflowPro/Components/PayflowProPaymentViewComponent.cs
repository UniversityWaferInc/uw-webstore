using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.UWPayflowPro.Components
{
    [ViewComponent(Name = "PayflowProPayment")]
    public class PayflowProPaymentViewComponent : NopViewComponent
    {
        /*
        private readonly ICustomerService a;
        private readonly ICountryService b;
        private readonly IStateProvinceService c;
        private readonly IOrderTotalCalculationService d;
        private readonly ICurrencyService e;
        private readonly IShoppingCartService f;
        private readonly IWorkContext g;
        private readonly IStoreContext h;
        private readonly IWebHelper i;
        private readonly ShoppingCartSettings j;
        private readonly CurrencySettings k;
        private readonly PayflowProSettings l;
        private readonly bool m;
        //private FNSLogger n;

        public PayflowProPaymentViewComponent(
          ICustomerService customerService,
          ICountryService countryService,
          IStateProvinceService stateProvinceService,
          IOrderTotalCalculationService orderTotalCalculationService,
          ICurrencyService currencyService,
          IShoppingCartService shoppingCartService,
          IWorkContext workContext,
          IStoreContext storeContext,
          IWebHelper webHelper,
          ShoppingCartSettings shoppingCartSettings,
          CurrencySettings currencySettings,
          PayflowProSettings payflowProSettings)
        {
            //this.\u002Ector();
            //this.a = customerService;
            //this.b = countryService;
            this.c = stateProvinceService;
            this.d = orderTotalCalculationService;
            this.e = currencyService;
            this.f = shoppingCartService;
            this.g = workContext;
            this.h = storeContext;
            this.i = webHelper;
            this.j = shoppingCartSettings;
            this.k = currencySettings;
            this.l = payflowProSettings;
            this.m = this.l.showDebugInfo;
            //this.n = new FNSLogger(this.m);
        }

        private void a(string message)
        {
            if (!this.m || this.n == null)
                return;
            this.n.LogMessage(message);
        }

        protected string GetViewName(string viewname)
        {
            return global::a.a("\xEBA9\xEBF8\xEB87\xEBBB\xEBA2\xEBB0\xEBBE\xEBB9\xEBA4\xEBF8\xEB91\xEBB8\xEBAF\xEB99\xEBB2\xEBA3\xEB84\xEBB8\xEBB1\xEBA3\xEBF9\xEB87\xEBB6\xEBAE\xEBB1\xEBBB\xEBB8\xEBA0\xEB87\xEBA5\xEBB8\xEBF8\xEB81\xEBBE\xEBB2\xEBA0\xEBA4\xEBF8\xEB87\xEBB6\xEBAE\xEBB1\xEBBB\xEBB8\xEBA0\xEB87\xEBA5\xEBB8\xEBF8", 60246) + viewname + global::a.a("\xE197\xE1DA\xE1CA\xE1D1\xE1CD\xE1D4\xE1D5", 57649);
        }

        private void b(IList<ShoppingCartItem> shoppingCart, out Decimal? orderTotal)
        {
            Decimal num1;
            List<Discount> discountList;
            List<AppliedGiftCard> appliedGiftCardList;
            int num2;
            Decimal num3;
            orderTotal = this.d.GetShoppingCartTotal(shoppingCart, ref num1, ref discountList, ref appliedGiftCardList, ref num2, ref num3, new bool?(), true);
        }

        public IViewComponentResult Invoke()
        {
            PaymentInfoModel paymentInfoModel = new PaymentInfoModel();
            if (this.l.CreateOrderMode == CreateOrderMode.BeforePayment)
                return (IViewComponentResult)this.View<PaymentInfoModel>(this.GetViewName(global::a.a("\xE25E\xE26F\xE277\xE263\xE26B\xE260\xE27A\xE247\xE260\xE268\xE261", 57866)), (M0)paymentInfoModel);
            IList<ShoppingCartItem> shoppingCart = this.f.GetShoppingCart(this.g.get_CurrentCustomer(), new ShoppingCartType?((ShoppingCartType)1), ((BaseEntity)this.h.get_CurrentStore()).get_Id(), new int?(), new DateTime?(), new DateTime?());
            if (!((IEnumerable<ShoppingCartItem>)shoppingCart).Any<ShoppingCartItem>())
                throw new NopException(global::a.a("\xE49E\xE4BC\xE4AF\xE4A9\xE4FD\xE4B4\xE4AE\xE4FD\xE4B8\xE4B0\xE4AD\xE4A9\xE4A4", 58460));
            Decimal? orderTotal;
            this.b(shoppingCart, out orderTotal);
            if (!orderTotal.HasValue)
                throw new NopException(global::a.a("\xEFB8\xEF85\xEF93\xEF92\xEF85\xEFD7\xEF83\xEF98\xEF83\xEF96\xEF9B\xEFD7\xEF94\xEF98\xEF82\xEF9B\xEF93\xEF99\xEFD0\xEF83\xEFD7\xEF95\xEF92\xEFD7\xEF94\xEF96\xEF9B\xEF94\xEF82\xEF9B\xEF96\xEF83\xEF92\xEF93", 61367));
            string str1 = UrlHelperExtensions.RouteUrl(((ViewComponent)this).get_Url(), PayflowProDefaults.ReturnUrlRoute, (object)null, this.i.get_CurrentRequestProtocol());
            string str2 = UrlHelperExtensions.RouteUrl(((ViewComponent)this).get_Url(), global::a.a("\xEAF0\xEACB\xEACC\xEAD3\xEAD3\xEACA\xEACD\xEAC4\xEAE0\xEAC2\xEAD1\xEAD7", 59939), (object)null, this.i.get_CurrentRequestProtocol());
            string str3 = str2;
            Currency currency = this.e.GetCurrencyById(this.k.get_PrimaryStoreCurrencyId());
            string str4 = Guid.NewGuid().ToString();
            Decimal num = orderTotal.Value;
            if (this.l.SupportMultiCurrency)
            {
                currency = this.g.get_WorkingCurrency();
                num = this.e.ConvertFromPrimaryStoreCurrency(orderTotal.Value, this.g.get_WorkingCurrency());
            }
            if (this.j.get_RoundPricesDuringCalculation())
                num = ((IPriceCalculationService)EngineContext.get_Current().Resolve<IPriceCalculationService>()).RoundPrice(num, currency);
            NameValueCollection requestArraySmall = new NameValueCollection()
      {
        {
          global::a.a("\xEEA7\xEEA1\xEEAB\xEEA7\xEEAA\xEEA3\xEEB6", 61171),
          this.l.TransactMode == TransactMode.AuthorizeAndCapture ? global::a.a("\xE2A6", 57892) : global::a.a("\xEE6F", 60970)
        },
        {
          global::a.a("\xF08C\xF080\xF099", 61452),
          num.ToString(global::a.a("\xF05F\xF041\xF05F\xF05F", 61485), (IFormatProvider) CultureInfo.InvariantCulture)
        },
        {
          global::a.a("\xEC9E\xEC88\xEC8F\xEC8F\xEC98\xEC93\xEC9E\xEC84", 60572),
          currency.get_CurrencyCode()
        },
        {
          global::a.a("\xF494\xF485\xF492\xF496\xF483\xF492\xF484\xF492\xF494\xF482\xF485\xF492\xF483\xF498\xF49C\xF492\xF499", 62599),
          global::a.a("\xF192", 61762)
        },
        {
          global::a.a("\xF1E8\xF1FE\xF1F8\xF1EE\xF1E9\xF1FE\xF1EF\xF1F4\xF1F0\xF1FE\xF1F5\xF1F2\xF1FF", 61755),
          str4
        },
        {
          global::a.a("\xF090\xF08D\xF09B\xF09A\xF08D\xF096\xF09B", 61663),
          str4
        },
        {
          global::a.a("\xE8AB\xE8BC\xE8AD\xE8AC\xE8AB\xE8B7\xE8AC\xE8AB\xE8B5", 59513),
          str1
        },
        {
          global::a.a("\xE6BE\xE6BC\xE6B3\xE6BE\xE6B8\xE6B1\xE6A8\xE6AF\xE6B1", 59068),
          str2
        },
        {
          global::a.a("\xF11A\xF10D\xF10D\xF110\xF10D\xF10A\xF10D\xF113", 61789),
          str3
        }
      };
            Address customerBillingAddress = this.a.GetCustomerBillingAddress(this.g.get_CurrentCustomer());
            if (customerBillingAddress != null)
            {
                Address address = customerBillingAddress;
                requestArraySmall.Add(global::a.a("\xF195\xF19E\xF19B\xF19B\xF183\xF198\xF191\xF19E\xF185\xF184\xF183\xF199\xF196\xF19A\xF192", 61831), address.get_FirstName());
                requestArraySmall.Add(global::a.a("\xECB7\xECBC\xECB9\xECB9\xECA1\xECBA\xECB9\xECB4\xECA6\xECA1\xECBB\xECB4\xECB8\xECB0", 60420), address.get_LastName());
                requestArraySmall.Add(global::a.a("\xF1A5\xF1AE\xF1AB\xF1AB\xF1B3\xF1A8\xF1B4\xF1B3\xF1B5\xF1A2\xF1A2\xF1B3", 61862), string.Format(global::a.a("\xEE09\xEE42\xEE0F\xEE52\xEE09\xEE43\xEE0F", 60976), (object)address.get_Address1(), (object)address.get_Address2()).Trim());
                requestArraySmall.Add(global::a.a("\xE999\xE992\xE997\xE997\xE98F\xE994\xE998\xE992\xE98F\xE982", 59730), address.get_City());
                StateProvince provinceByAddress = this.c.GetStateProvinceByAddress(address);
                if (provinceByAddress != null)
                    requestArraySmall.Add(global::a.a("\xE2A5\xE2AE\xE2AB\xE2AB\xE2B3\xE2A8\xE2B4\xE2B3\xE2A6\xE2B3\xE2A2", 58022), provinceByAddress?.get_Abbreviation());
                requestArraySmall.Add(global::a.a("\xE399\xE392\xE397\xE397\xE38F\xE394\xE381\xE392\xE38B", 58194), address.get_ZipPostalCode());
                Country countryByAddress = this.b.GetCountryByAddress(address);
                if (countryByAddress != null)
                    requestArraySmall.Add(global::a.a("\xF7B7\xF7BC\xF7B9\xF7B9\xF7A1\xF7BA\xF7B6\xF7BA\xF7A0\xF7BB\xF7A1\xF7A7\xF7AC", 63444), countryByAddress?.get_TwoLetterIsoCode());
                string email = address.get_Email();
                if (string.IsNullOrWhiteSpace(email))
                    email = this.g.get_CurrentCustomer().get_Email();
                requestArraySmall.Add(global::a.a("\xE6B7\xE6BC\xE6B9\xE6B9\xE6A1\xE6BA\xE6B0\xE6B8\xE6B4\xE6BC\xE6B9\xE6AE\xE6C4\xE6CD\xE6A8", 58901), email);
            }
            Address customerShippingAddress = this.a.GetCustomerShippingAddress(this.g.get_CurrentCustomer());
            if (customerShippingAddress != null)
            {
                Address address = customerShippingAddress;
                requestArraySmall.Add(global::a.a("\xE1A6\xE1BD\xE1BC\xE1A5\xE1A1\xE1BA\xE1B3\xE1BC\xE1A7\xE1A6\xE1A1\xE1BB\xE1B4\xE1B8\xE1B0", 57716), address.get_FirstName());
                requestArraySmall.Add(global::a.a("\xE60C\xE617\xE616\xE60F\xE60B\xE610\xE613\xE61E\xE60C\xE60B\xE611\xE61E\xE612\xE61A", 58973), address.get_LastName());
                requestArraySmall.Add(global::a.a("\xE7BC\xE7A7\xE7A6\xE7BF\xE7BB\xE7A0\xE7BC\xE7BB\xE7BD\xE7AA\xE7AA\xE7BB", 59374), string.Format(global::a.a("\xEE09\xEE42\xEE0F\xEE52\xEE09\xEE43\xEE0F", 60976), (object)address.get_Address1(), (object)address.get_Address2()).Trim());
                requestArraySmall.Add(global::a.a("\xEEA8\xEEB3\xEEB2\xEEAB\xEEAF\xEEB4\xEEB8\xEEB2\xEEAF\xEEA2", 61035), address.get_City());
                StateProvince provinceByAddress = this.c.GetStateProvinceByAddress(address);
                if (provinceByAddress != null)
                    requestArraySmall.Add(global::a.a("\xF06D\xF076\xF077\xF06E\xF06A\xF071\xF06D\xF06A\xF07F\xF06A\xF07B", 61498), provinceByAddress?.get_Abbreviation());
                requestArraySmall.Add(global::a.a("\xE6A8\xE6B3\xE6B2\xE6AB\xE6AF\xE6B4\xE6A1\xE6B2\xE6AB", 59131), address.get_ZipPostalCode());
                Country countryByAddress = this.b.GetCountryByAddress(address);
                if (countryByAddress != null)
                    requestArraySmall.Add(global::a.a("\xE3A6\xE3BD\xE3BC\xE3A5\xE3A1\xE3BA\xE3B6\xE3BA\xE3A0\xE3BB\xE3A1\xE3A7\xE3AC", 58244), countryByAddress?.get_TwoLetterIsoCode());
                string email = address.get_Email();
                if (string.IsNullOrWhiteSpace(email))
                    email = this.g.get_CurrentCustomer().get_Email();
                requestArraySmall.Add(global::a.a("\xF52C\xF537\xF536\xF52F\xF52B\xF530\xF53A\xF532\xF53E\xF536\xF533\xF524\xF54E\xF547\xF522", 62783), email);
            }
            if (this.f.ShoppingCartIsRecurring(shoppingCart))
                requestArraySmall.Add(global::a.a("\xECAB\xECBC\xECBA\xECAC\xECAB\xECAB\xECB0\xECB7\xECBE", 60665), global::a.a("\xF192", 61762));
            PayflowProResponce payflowProResponce = PayflowProHelper.PayflowProCall(global::a.a("\xF292\xF2B5\xF2AD\xF2B4\xF2B0\xF2BE\xF2FB\xF2B6\xF2BE\xF2AF\xF2B3\xF2B4\xF2BF", 62027), requestArraySmall, this.l);
            if (string.IsNullOrWhiteSpace(payflowProResponce.Error) && (string.IsNullOrWhiteSpace(payflowProResponce.SECURETOKEN) || string.IsNullOrWhiteSpace(payflowProResponce.SECURETOKENID)))
                payflowProResponce.Error = global::a.a("\xE0AB\xE09A\xE082\xE09D\xE097\xE094\xE08C\xE0DB\xE098\xE09A\xE097\xE097\xE0DB\xE09D\xE09A\xE092\xE097\xE09E\xE09F", 57595);
            if (this.m)
                this.a(payflowProResponce.ToString(""));
            string str5 = "";
            if (!string.IsNullOrWhiteSpace(payflowProResponce.Error))
                paymentInfoModel.Warnings.Add(payflowProResponce.Error);
            else
                str5 = QueryHelpers.AddQueryString(PayflowProDefaults.PayFlowLink, (IDictionary<string, string>)new Dictionary<string, string>()
        {
          {
            global::a.a("\xE99C\xE98A\xE98C\xE99A\xE99D\xE98A\xE99B\xE980\xE984\xE98A\xE981", 59791),
            payflowProResponce.SECURETOKEN
          },
          {
            global::a.a("\xF1E8\xF1FE\xF1F8\xF1EE\xF1E9\xF1FE\xF1EF\xF1F4\xF1F0\xF1FE\xF1F5\xF1F2\xF1FF", 61755),
            payflowProResponce.SECURETOKENID
          },
          {
            global::a.a("\xE7D2\xE7D0\xE7DB\xE7DA", 59294),
            this.l.UseSandbox ? global::a.a("\xEEA3\xEEB2\xEEA4\xEEA3", 61159) : global::a.a("\xE1B5\xE1B0\xE1AF\xE1BC", 57833)
          }
        });
            paymentInfoModel.CheckOutUrl = str5;
            return (IViewComponentResult)this.View<PaymentInfoModel>(this.GetViewName(global::a.a("\xE25E\xE26F\xE277\xE263\xE26B\xE260\xE27A\xE247\xE260\xE268\xE261", 57866)), (M0)paymentInfoModel);
        }
    }*/
    }
}
