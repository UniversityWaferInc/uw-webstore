using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;

namespace Nop.Plugin.Payments.UWPayflowPro.Models
{
    public class ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.UW.Plugin.Payments.PayflowPro.Partner")]
        public string Partner { get; set; }

        public bool Partner_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.UW.Plugin.Payments.PayflowPro.Vendor")]
        public string Vendor { get; set; }

        public bool Vendor_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.UW.Plugin.Payments.PayflowPro.User")]
        public string User { get; set; }

        public bool User_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.UW.Plugin.Payments.PayflowPro.Password")]
        public string Password { get; set; }

        public bool Password_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.UW.Plugin.Payments.PayflowPro.UseSandbox")]
        public bool UseSandbox { get; set; }

        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.UW.Plugin.Payments.PayflowPro.TransactMode")]
        public int TransactModeId { get; set; }

        public bool TransactModeId_OverrideForStore { get; set; }

        public SelectList TransactModeValues { get; set; }

        [NopResourceDisplayName("Admin.UW.Plugin.Payments.PayflowPro.SupportMultiCurrency")]
        public bool SupportMultiCurrency { get; set; }

        public bool SupportMultiCurrency_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.UW.Plugin.Payments.PayflowPro.ReturnUrl")]
        public string ReturnUrl { get; set; }

        [NopResourceDisplayName("Admin.UW.Plugin.Payments.PayflowPro.CreateOrderMode")]
        public int CreateOrderModeId { get; set; }

        public bool CreateOrderModeId_OverrideForStore { get; set; }

        public SelectList CreateOrderModeValues { get; set; }

        [NopResourceDisplayName("Admin.UW.Plugin.Payments.PayflowPro.SkipPaymentInfo")]
        public bool SkipPaymentInfo { get; set; }

        public bool SkipPaymentInfo_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.UW.Plugin.Payments.PayflowPro.AdditionalFee")]
        public Decimal AdditionalFee { get; set; }

        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.UW.Plugin.Payments.PayflowPro.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }

        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.UW.Plugin.Payments.PayflowPro.showDebugInfo")]
        public bool showDebugInfo { get; set; }

        [NopResourceDisplayName("Admin.UW.Plugin.Payments.PayflowPro.SerialNumber")]
        public string SerialNumber { get; set; }

        [NopResourceDisplayName("Admin.UW.Plugin.Payments.PayflowPro.StoreUrl")]
        public string StoreUrl { get; set; }

        public bool IsRegisted { get; set; }

        public ConfigurationModel()
        {

        }
    }
}
