using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.UWPayflowPro.Domains
{
    public class PayflowProResponce
    {
        public string SECURETOKEN { get; set; }

        public string SECURETOKENID { get; set; }

        public string Error { get; set; }

        public string UrlEndpoint { get; set; }

        public NameValueCollection RequestValueCollection { get; set; }

        public NameValueCollection ResponseValueCollection { get; set; }

        public string ToString(string separator = "")
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(global::a.a("\xF7AF\xF79E\xF786\xF799\xF793\xF790\xF788\xF7AF\xF78D\xF790\xF7DF\xF7AA\xF78D\xF793\xF7BA\xF791\xF79B\xF78F\xF790\xF796\xF791\xF78B\xF7C5", 63480) + this.UrlEndpoint + separator);
            if (!string.IsNullOrWhiteSpace(this.Error))
                stringBuilder.AppendLine(global::a.a("\xE51A\xE52D\xE52D\xE530\xE52D\xE565", 58696) + this.Error + separator);
            if (this.RequestValueCollection != null)
            {
                stringBuilder.AppendLine(global::a.a("\xE62D\xE61A\xE60E\xE60A\xE61A\xE60C\xE60B\xE645", 59006) + separator);
                foreach (string requestValue1 in (NameObjectCollectionBase)this.RequestValueCollection)
                {
                    string requestValue2 = this.RequestValueCollection[requestValue1];
                    stringBuilder.AppendLine(global::a.a("\xE28E\xE28E\xE28E\xE28E\xE28E", 58026) + requestValue1 + global::a.a("\xE942", 59752) + requestValue2 + separator);
                }
            }
            if (this.ResponseValueCollection != null)
            {
                stringBuilder.AppendLine(global::a.a("\xE09F\xE0A8\xE0BE\xE0BD\xE0A2\xE0A3\xE0AE\xE0A8\xE0F7", 57548) + separator);
                foreach (string responseValue1 in (NameObjectCollectionBase)this.ResponseValueCollection)
                {
                    string responseValue2 = this.ResponseValueCollection[responseValue1];
                    stringBuilder.AppendLine(global::a.a("\xE28E\xE28E\xE28E\xE28E\xE28E", 58026) + responseValue1 + global::a.a("\xE942", 59752) + responseValue2 + separator);
                }
            }
            return stringBuilder.ToString();
        }
    }
}
