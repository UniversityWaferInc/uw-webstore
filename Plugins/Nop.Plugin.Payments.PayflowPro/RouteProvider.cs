using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.UWPayflowPro
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            ControllerEndpointRouteBuilderExtensions.MapControllerRoute(endpointRouteBuilder, PayflowProDefaults.ReturnUrlRoute, "plugins/payflowpropayment/return", (object)new
            {
                controller = "PayflowProPayment",
                action = "ReturnHandler"
            }, null, null);
        }

        public int Priority
        {
            get
            {
                return 100;
            }
        }
    }
}
