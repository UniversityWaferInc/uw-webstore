using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.UWPayflowPro.Models
{
    public class ErrorMessageModel : BaseNopModel
    {
        public ErrorMessageModel()
        {
            this.Warnings = (IList<string>)new List<string>();
        }

        public IList<string> Warnings { get; set; }
    }
}
