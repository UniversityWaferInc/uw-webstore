using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.UWPayflowPro
{
    public class PayflowProSettings : ISettings
    {
        public string Partner { get; set; }

        public string Vendor { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public bool UseSandbox { get; set; }

        public CreateOrderMode CreateOrderMode { get; set; }

        public TransactMode TransactMode { get; set; }

        public bool SupportMultiCurrency { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public Decimal AdditionalFee { get; set; }

        public bool showDebugInfo { get; set; }

        public bool SkipPaymentInfo { get; set; }

        public string SerialNumber { get; set; }
    }
}
