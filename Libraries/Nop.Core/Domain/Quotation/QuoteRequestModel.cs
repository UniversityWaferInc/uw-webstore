using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Models.Quotation
{
    public class QuoteRequestModel
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        public int? CountryId { get; set; }
        public int? StateProvinceId { get; set; }
        public string CountryName { get; set; }
        public string City { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string ZipPostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? OrderTotal { get; set; }
        public decimal? ShippingCharge { get; set; }
        public bool IsShippingAddressSame { get; set; }
        public int? CountryIdDSA { get; set; }
        public string CountryNameDSA { get; set; }
        public string CustomerCurrencyCode { get; set; }
        public string CurrencyRate { get; set; }
        public string FirstNameDSA { get; set; }
        public string LastNameDSA { get; set; }
        public string CompanyNameDSA { get; set; }
        public string Address1DSA { get; set; }
        public string Address2DSA { get; set; }
        public string CityDSA { get; set; }
        public int? StateIdDSA { get; set; }
        public string ZipPostalCodeDSA { get; set; }
        public string PhoneNumberDSA { get; set; }
    }
}
