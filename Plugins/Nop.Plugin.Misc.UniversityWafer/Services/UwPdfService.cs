using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.UniversityWafer.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Orders;
using Nop.Services.Shipping.Date;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nop.Plugin.Misc.UniversityWafer.Services
{
    public class UWPdfService
    {
        private readonly INopDataProvider _nopDataProvider;
        private readonly IPdfService _pdfService;
        private readonly IOrderService _orderService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IDateRangeService _dateRangeSerivce;

        public UWPdfService(INopDataProvider nopDataProvider, IPdfService pdfService, IOrderService orderService, ISpecificationAttributeService specificationAttributeService, IDateRangeService dateRangeSerivce)
        {
            _nopDataProvider = nopDataProvider;
            _pdfService = pdfService;
            _orderService = orderService;
            _specificationAttributeService = specificationAttributeService;
            _dateRangeSerivce = dateRangeSerivce;
        }

        public void GetPdfInvoice(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted) { }

            var orders = new List<Order>();
            orders.Add(order);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                //_pdfService.PrintOrdersToPdf(stream, orders, _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }
            //return File(bytes, MimeTypes.ApplicationPdf, $"order_{order.Id}.pdf");
        }
    }
}
