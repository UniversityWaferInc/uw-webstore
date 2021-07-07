using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.UWPayflowPro.Domains
{
    public enum TransactMode
    {
        Authorize = 0,
        AuthorizeAndCapture = 2,
    }
}
