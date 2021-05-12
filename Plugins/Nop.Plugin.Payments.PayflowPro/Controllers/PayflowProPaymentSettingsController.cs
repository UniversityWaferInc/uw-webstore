using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.UWPayflowPro.Controllers
{
    [AuthorizeAdmin(false)]
    [Area("Admin")]
    public class PayflowProPaymentSettingsController : BasePluginController
    {
        private readonly ISettingService a;
        private readonly IPermissionService b;
        private readonly ILocalizationService c;
        private readonly INotificationService d;
        private readonly IStoreContext e;
        private readonly IStoreService f;
        private readonly IWebHelper g;
        private readonly TestDataPlugin h;
        private bool i;
        private readonly FNSLogger j;

        public PayflowProPaymentSettingsController(
          ISettingService settingService,
          IPermissionService permissionService,
          ILocalizationService localizationService,
          INotificationService notificationService,
          IStoreContext storeContext,
          IStoreService storeService,
          IWebHelper webHelper,
          PayflowProSettings payflowProSettings)
        {
            this.\u002Ector();
            this.a = settingService;
            this.b = permissionService;
            this.c = localizationService;
            this.d = notificationService;
            this.e = storeContext;
            this.f = storeService;
            this.g = webHelper;
            this.h = new TestDataPlugin();
            this.i = false;
            this.j = new FNSLogger(this.i);
        }

        private void a(string message)
        {
            if (!this.i || this.j == null)
                return;
            this.j.LogMessage(message);
        }

        private void b(string message, bool encode = true)
        {
            this.d.ErrorNotification(message, encode);
        }

        private void c(string message, bool encode = true)
        {
            this.d.SuccessNotification(message, encode);
        }

        private string d(string viewname)
        {
            return global::a.a("\xE28B\xE2DA\xE2A5\xE299\xE280\xE292\xE29C\xE29B\xE286\xE2DA\xE2B3\xE29A\xE28D\xE2BB\xE290\xE281\xE2A6\xE29A\xE293\xE281\xE2DB\xE2A5\xE294\xE28C\xE293\xE299\xE29A\xE282\xE2A5\xE287\xE29A\xE2DA\xE2A3\xE29C\xE290\xE282\xE286\xE2DA\xE2B6\xE29A\xE29B\xE293\xE29C\xE292\xE280\xE287\xE290\xE2DA", 57941) + viewname + global::a.a("\xEBC3\xEB8E\xEB9E\xEB85\xEB99\xEB80\xEB81", 60204);
        }

        private void e()
        {
            if (this.h.IsExpired)
            {
                this.b(this.c.GetResource(global::a.a("\xEE1E\xEE3B\xEE32\xEE36\xEE31\xEE71\xEE19\xEE30\xEE27\xEE11\xEE3A\xEE2B\xEE0C\xEE30\xEE39\xEE2B\xEE71\xEE0F\xEE33\xEE2A\xEE38\xEE36\xEE31\xEE71\xEE0F\xEE3E\xEE26\xEE32\xEE3A\xEE31\xEE2B\xEE2C\xEE71\xEE0F\xEE3E\xEE26\xEE39\xEE33\xEE30\xEE28\xEE0F\xEE2D\xEE30\xEE71\xEE16\xEE2C\xEE1A\xEE27\xEE2F\xEE36\xEE2D\xEE3A\xEE3B", 60957)), true);
            }
            else
            {
                if (this.h.IsRegisted)
                    return;
                this.b(this.c.GetResource(global::a.a("\xEEDA\xEEFF\xEEF6\xEEF2\xEEF5\xEEB5\xEEDD\xEEF4\xEEE3\xEED5\xEEFE\xEEEF\xEEC8\xEEF4\xEEFD\xEEEF\xEEB5\xEECB\xEEF7\xEEEE\xEEFC\xEEF2\xEEF5\xEEB5\xEECB\xEEFA\xEEE2\xEEF6\xEEFE\xEEF5\xEEEF\xEEE8\xEEB5\xEECB\xEEFA\xEEE2\xEEFD\xEEF7\xEEF4\xEEEC\xEECB\xEEE9\xEEF4\xEEB5\xEED2\xEEE8\xEED5\xEEF4\xEEC9\xEEFE\xEEFC\xEEF2\xEEE8\xEEEF\xEEFE\xEEFF", 60939)), true);
            }
        }

        public IActionResult Configure()
        {
            if (!this.b.Authorize((PermissionRecord)StandardPermissionProvider.ManagePaymentMethods))
                return ((BaseController)this).AccessDeniedView();
            new InstallLocaleResources(global::a.a("\xF540\xF511\xF56E\xF552\xF54B\xF559\xF557\xF550\xF54D\xF511\xF578\xF551\xF546\xF570\xF55B\xF54A\xF56D\xF551\xF558\xF54A\xF510\xF56E\xF55F\xF547\xF558\xF552\xF551\xF549\xF56E\xF54C\xF551\xF511\xF56C\xF55B\xF54D\xF551\xF54B\xF54C\xF55D\xF55B\xF54D\xF511", 62778), false).Update();
            int scopeConfiguration = this.e.get_ActiveStoreScopeConfiguration();
            PayflowProSettings payflowProSettings1 = (PayflowProSettings)this.a.LoadSetting<PayflowProSettings>(scopeConfiguration);
            string str = this.g.GetStoreLocation(new bool?());
            if (scopeConfiguration > 0)
                str = this.f.GetStoreById(scopeConfiguration).get_Url().Trim();
            ConfigurationModel configurationModel = new ConfigurationModel()
            {
                ActiveStoreScopeConfiguration = scopeConfiguration,
                Partner = payflowProSettings1.Partner,
                Vendor = payflowProSettings1.Vendor,
                User = payflowProSettings1.User,
                Password = payflowProSettings1.Password,
                UseSandbox = payflowProSettings1.UseSandbox,
                TransactModeId = (int)payflowProSettings1.TransactMode,
                TransactModeValues = Extensions.ToSelectList<TransactMode>((M0)payflowProSettings1.TransactMode, true, (int[])null, true),
                CreateOrderModeId = (int)payflowProSettings1.CreateOrderMode,
                CreateOrderModeValues = Extensions.ToSelectList<CreateOrderMode>((M0)payflowProSettings1.CreateOrderMode, true, (int[])null, true),
                SupportMultiCurrency = payflowProSettings1.SupportMultiCurrency,
                AdditionalFee = payflowProSettings1.AdditionalFee,
                AdditionalFeePercentage = payflowProSettings1.AdditionalFeePercentage,
                SkipPaymentInfo = payflowProSettings1.SkipPaymentInfo,
                ReturnUrl = string.Format(global::a.a("\xE086\xE0CD\xE080\xE08D\xE091\xE088\xE09A\xE094\xE093\xE08E\xE0D2\xE08D\xE09C\xE084\xE09B\xE091\xE092\xE08A\xE08D\xE08F\xE092\xE08D\xE09C\xE084\xE090\xE098\xE093\xE089\xE0D2\xE08F\xE098\xE089\xE088\xE08F\xE093", 57532), (object)str).ToLower(),
                showDebugInfo = payflowProSettings1.showDebugInfo,
                SerialNumber = payflowProSettings1.SerialNumber,
                IsRegisted = this.h.IsRegisted,
                StoreUrl = this.h.StoreUrl
            };
            if (scopeConfiguration > 0)
            {
                configurationModel.Partner_OverrideForStore = (this.a.SettingExists<PayflowProSettings, string>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.Partner), scopeConfiguration) ? 1 : 0) != 0;
                configurationModel.Vendor_OverrideForStore = (this.a.SettingExists<PayflowProSettings, string>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.Vendor), scopeConfiguration) ? 1 : 0) != 0;
                configurationModel.User_OverrideForStore = (this.a.SettingExists<PayflowProSettings, string>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.User), scopeConfiguration) ? 1 : 0) != 0;
                configurationModel.Password_OverrideForStore = (this.a.SettingExists<PayflowProSettings, string>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.Password), scopeConfiguration) ? 1 : 0) != 0;
                configurationModel.UseSandbox_OverrideForStore = (this.a.SettingExists<PayflowProSettings, bool>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.UseSandbox), scopeConfiguration) ? 1 : 0) != 0;
                configurationModel.TransactModeId_OverrideForStore = (this.a.SettingExists<PayflowProSettings, TransactMode>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.TransactMode), scopeConfiguration) ? 1 : 0) != 0;
                configurationModel.CreateOrderModeId_OverrideForStore = (this.a.SettingExists<PayflowProSettings, CreateOrderMode>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.CreateOrderMode), scopeConfiguration) ? 1 : 0) != 0;
                configurationModel.SkipPaymentInfo_OverrideForStore = (this.a.SettingExists<PayflowProSettings, bool>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.SkipPaymentInfo), scopeConfiguration) ? 1 : 0) != 0;
                configurationModel.SupportMultiCurrency_OverrideForStore = (this.a.SettingExists<PayflowProSettings, bool>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.SupportMultiCurrency), scopeConfiguration) ? 1 : 0) != 0;
                configurationModel.AdditionalFee_OverrideForStore = (this.a.SettingExists<PayflowProSettings, Decimal>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.AdditionalFee), scopeConfiguration) ? 1 : 0) != 0;
                configurationModel.AdditionalFeePercentage_OverrideForStore = (this.a.SettingExists<PayflowProSettings, bool>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.AdditionalFeePercentage), scopeConfiguration) ? 1 : 0) != 0;
            }
            this.e();
            return (IActionResult)((Controller)this).View(this.d(global::a.a("\xF1DD\xF1F1\xF1F0\xF1F8\xF1F7\xF1F9\xF1EB\xF1EC\xF1FB", 61850)), (object)configurationModel);
        }

        [HttpPost]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!this.b.Authorize((PermissionRecord)StandardPermissionProvider.ManagePaymentMethods))
                return ((BaseController)this).AccessDeniedView();
            if (!((ControllerBase)this).get_ModelState().get_IsValid())
                return this.Configure();
            int scopeConfiguration = this.e.get_ActiveStoreScopeConfiguration();
            PayflowProSettings payflowProSettings1 = (PayflowProSettings)this.a.LoadSetting<PayflowProSettings>(scopeConfiguration);
            this.g.GetStoreLocation(new bool?());
            if (scopeConfiguration > 0)
                this.f.GetStoreById(scopeConfiguration).get_Url().Trim();
            payflowProSettings1.Partner = model.Partner;
            payflowProSettings1.Vendor = model.Vendor;
            payflowProSettings1.User = model.User;
            payflowProSettings1.Password = model.Password;
            payflowProSettings1.UseSandbox = model.UseSandbox;
            payflowProSettings1.TransactMode = (TransactMode)model.TransactModeId;
            payflowProSettings1.CreateOrderMode = (CreateOrderMode)model.CreateOrderModeId;
            payflowProSettings1.SupportMultiCurrency = model.SupportMultiCurrency;
            payflowProSettings1.SkipPaymentInfo = model.SkipPaymentInfo;
            payflowProSettings1.AdditionalFee = model.AdditionalFee;
            payflowProSettings1.AdditionalFeePercentage = model.AdditionalFeePercentage;
            payflowProSettings1.showDebugInfo = model.showDebugInfo;
            payflowProSettings1.SerialNumber = string.IsNullOrEmpty(model.SerialNumber) ? "" : model.SerialNumber.Trim();
            this.a.SaveSetting<PayflowProSettings, bool>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.showDebugInfo), 0, false);
            this.a.SaveSetting<PayflowProSettings, string>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.SerialNumber), 0, false);
            this.a.SaveSettingOverridablePerStore<PayflowProSettings, string>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.Partner), (model.Partner_OverrideForStore ? 1 : 0) != 0, scopeConfiguration, false);
            this.a.SaveSettingOverridablePerStore<PayflowProSettings, string>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.Vendor), (model.Vendor_OverrideForStore ? 1 : 0) != 0, scopeConfiguration, false);
            this.a.SaveSettingOverridablePerStore<PayflowProSettings, string>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.User), (model.User_OverrideForStore ? 1 : 0) != 0, scopeConfiguration, false);
            this.a.SaveSettingOverridablePerStore<PayflowProSettings, string>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.Password), (model.Password_OverrideForStore ? 1 : 0) != 0, scopeConfiguration, false);
            this.a.SaveSettingOverridablePerStore<PayflowProSettings, bool>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.UseSandbox), (model.UseSandbox_OverrideForStore ? 1 : 0) != 0, scopeConfiguration, false);
            this.a.SaveSettingOverridablePerStore<PayflowProSettings, TransactMode>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.TransactMode), (model.TransactModeId_OverrideForStore ? 1 : 0) != 0, scopeConfiguration, false);
            this.a.SaveSettingOverridablePerStore<PayflowProSettings, CreateOrderMode>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.CreateOrderMode), (model.CreateOrderModeId_OverrideForStore ? 1 : 0) != 0, scopeConfiguration, false);
            this.a.SaveSettingOverridablePerStore<PayflowProSettings, bool>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.SupportMultiCurrency), (model.SupportMultiCurrency_OverrideForStore ? 1 : 0) != 0, scopeConfiguration, false);
            this.a.SaveSettingOverridablePerStore<PayflowProSettings, Decimal>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.AdditionalFee), (model.AdditionalFee_OverrideForStore ? 1 : 0) != 0, scopeConfiguration, false);
            this.a.SaveSettingOverridablePerStore<PayflowProSettings, bool>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.AdditionalFeePercentage), (model.AdditionalFeePercentage_OverrideForStore ? 1 : 0) != 0, scopeConfiguration, false);
            this.a.SaveSettingOverridablePerStore<PayflowProSettings, bool>((M0)payflowProSettings1, (Expression<Func<M0, M1>>)(payflowProSettings => payflowProSettings.SkipPaymentInfo), (model.SkipPaymentInfo_OverrideForStore ? 1 : 0) != 0, scopeConfiguration, false);
            this.a.ClearCache();
            this.h.RefreshRegister();
            this.c(this.c.GetResource(global::a.a("\xF6DA\xF6FF\xF6F6\xF6F2\xF6F5\xF6B5\xF6CB\xF6F7\xF6EE\xF6FC\xF6F2\xF6F5\xF6E8\xF6B5\xF6C8\xF6FA\xF6ED\xF6FE\xF6FF", 63115)), true);
            return this.Configure();
        }

        public IActionResult ClearLogFile()
        {
            if (!this.b.Authorize((PermissionRecord)StandardPermissionProvider.ManagePaymentMethods))
                return ((BaseController)this).AccessDeniedView();
            this.j.ClearLogFile();
            return (IActionResult)((ControllerBase)this).RedirectToAction(global::a.a("\xECBD\xEC91\xEC90\xEC98\xEC97\xEC99\xEC8B\xEC8C\xEC9B", 60666), global::a.a("\xF7E3\xF7D2\xF7CA\xF7D5\xF7DF\xF7DC\xF7C4\xF7E3\xF7C1\xF7DC\xF7E3\xF7D2\xF7CA\xF7DE\xF7D6\xF7DD\xF7C7\xF7E0\xF7D6\xF7C7\xF7C7\xF7DA\xF7DD\xF7D4\xF7C0", 63411), (object)new
            {
                area = global::a.a("\xE3FA\xE3DF\xE3D6\xE3D2\xE3D5", 58299)
            });
        }

        public IActionResult GetLogFile()
        {
            if (!this.b.Authorize((PermissionRecord)StandardPermissionProvider.ManagePaymentMethods))
                return ((BaseController)this).AccessDeniedView();
            string logFilePath = this.j.GetLogFilePath();
            return File.Exists(logFilePath) ? (IActionResult)((ControllerBase)this).File(File.ReadAllBytes(logFilePath), global::a.a("\xF30E\xF31F\xF31F\xF303\xF306\xF30C\xF30E\xF31B\xF306\xF300\xF301\xF340\xF300\xF30C\xF31B\xF30A\xF31B\xF342\xF31C\xF31B\xF31D\xF30A\xF30E\xF302", 62255), Path.GetFileName(logFilePath)) : (IActionResult)((ControllerBase)this).RedirectToAction(global::a.a("\xE5BA\xE596\xE597\xE59F\xE590\xE59E\xE58C\xE58B\xE59C", 58681), global::a.a("\xE39F\xE3AE\xE3B6\xE3A9\xE3A3\xE3A0\xE3B8\xE39F\xE3BD\xE3A0\xE39F\xE3AE\xE3B6\xE3A2\xE3AA\xE3A1\xE3BB\xE39C\xE3AA\xE3BB\xE3BB\xE3A6\xE3A1\xE3A8\xE3BC", 58253), (object)new
            {
                area = global::a.a("\xE2BA\xE29F\xE296\xE292\xE295", 57963)
            });
        }
    }
}
