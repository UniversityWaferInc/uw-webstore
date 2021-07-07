using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.UWPayflowPro.Domains
{
    [Serializable]
    public class PayfloProPaymentResult
    {
        private PaymentStatus _newPaymentStatus = (PaymentStatus)10;

        public PayfloProPaymentResult()
        {
            this.Errors = (IList<string>)new List<string>();
        }

        public bool Success
        {
            get
            {
                return !this.Errors.Any<string>();
            }
        }

        public void AddError(string error)
        {
            this.Errors.Add(error);
        }

        public IList<string> Errors { get; set; }

        public string TransactionId { get; set; }

        public Guid OrderGuid { get; set; }

        public Decimal OrderTotal { get; set; }

        public Decimal OrderSubTotal { get; set; }

        public string AvsResult { get; set; }

        public string AuthorizationTransactionId { get; set; }

        public string AuthorizationTransactionCode { get; set; }

        public string AuthorizationTransactionResult { get; set; }

        public string CaptureTransactionId { get; set; }

        public string CaptureTransactionResult { get; set; }

        public string SubscriptionTransactionId { get; set; }

        public string Token { get; set; }

        public PaymentStatus NewPaymentStatus
        {
            get
            {
                return this._newPaymentStatus;
            }
            set
            {
                this._newPaymentStatus = value;
            }
        }
    }
}
