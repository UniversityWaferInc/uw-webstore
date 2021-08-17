using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.UWPayflowPro.Models
{
    public class PaymentInfoModel : BaseNopModel
    {
        public PaymentInfoModel()
        {
            this.Warnings = (IList<string>)new List<string>();
        }

        public string CheckOutUrl { get; set; }

        public IList<string> Warnings { get; set; }
    }
}
