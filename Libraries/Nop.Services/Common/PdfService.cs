// RTL Support provided by Credo inc (www.credo.co.il  ||   info@credo.co.il)

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Html;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.CustomCode;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Nop.Services.Vendors;

namespace Nop.Services.Common
{
    /// <summary>
    /// PDF service
    /// </summary>
    public partial class PdfService : IPdfService
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IGiftCardService _giftCardService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IMeasureService _measureService;
        private readonly INopFileProvider _fileProvider;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IRewardPointService _rewardPointService;
        private readonly ISettingService _settingService;
        private readonly IShipmentService _shipmentService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly MeasureSettings _measureSettings;
        private readonly PdfSettings _pdfSettings;
        private readonly TaxSettings _taxSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly IDateRangeService _dateRangeService;

        #endregion

        #region Ctor

        public PdfService(AddressSettings addressSettings,
            CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IAddressAttributeFormatter addressAttributeFormatter,
            IAddressService addressService,
            ICountryService countryService,
            ICurrencyService currencyService,
            IDateTimeHelper dateTimeHelper,
            IGiftCardService giftCardService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IMeasureService measureService,
            INopFileProvider fileProvider,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IRewardPointService rewardPointService,
            ISettingService settingService,
            IShipmentService shipmentService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreService storeService,
            IVendorService vendorService,
            IWorkContext workContext,
            MeasureSettings measureSettings,
            PdfSettings pdfSettings,
            TaxSettings taxSettings,
            VendorSettings vendorSettings,
            IDateRangeService dateRangeService)
        {
            _addressSettings = addressSettings;
            _addressService = addressService;
            _catalogSettings = catalogSettings;
            _countryService = countryService;
            _currencySettings = currencySettings;
            _addressAttributeFormatter = addressAttributeFormatter;
            _currencyService = currencyService;
            _dateTimeHelper = dateTimeHelper;
            _giftCardService = giftCardService;
            _languageService = languageService;
            _localizationService = localizationService;
            _measureService = measureService;
            _fileProvider = fileProvider;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _rewardPointService = rewardPointService;
            _settingService = settingService;
            _shipmentService = shipmentService;
            _storeContext = storeContext;
            _stateProvinceService = stateProvinceService;
            _storeService = storeService;
            _vendorService = vendorService;
            _workContext = workContext;
            _measureSettings = measureSettings;
            _pdfSettings = pdfSettings;
            _taxSettings = taxSettings;
            _vendorSettings = vendorSettings;
            _dateRangeService = dateRangeService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get font
        /// </summary>
        /// <returns>Font</returns>
        protected virtual Font GetFont()
        {
            //nopCommerce supports Unicode characters
            //nopCommerce uses Free Serif font by default (~/App_Data/Pdf/FreeSerif.ttf file)
            //It was downloaded from http://savannah.gnu.org/projects/freefont
            return GetFont(_pdfSettings.FontFileName);
        }

        /// <summary>
        /// Get font
        /// </summary>
        /// <param name="fontFileName">Font file name</param>
        /// <returns>Font</returns>
        protected virtual Font GetFont(string fontFileName)
        {
            if (fontFileName == null)
                throw new ArgumentNullException(nameof(fontFileName));

            var fontPath = _fileProvider.Combine(_fileProvider.MapPath("~/App_Data/Pdf/"), fontFileName);
            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new Font(baseFont, 10, Font.NORMAL);
            return font;
        }

        /// <summary>
        /// Get font direction
        /// </summary>
        /// <param name="lang">Language</param>
        /// <returns>Font direction</returns>
        protected virtual int GetDirection(Language lang)
        {
            return lang.Rtl ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR;
        }

        /// <summary>
        /// Get element alignment
        /// </summary>
        /// <param name="lang">Language</param>
        /// <param name="isOpposite">Is opposite?</param>
        /// <returns>Element alignment</returns>
        protected virtual int GetAlignment(Language lang, bool isOpposite = false)
        {
            //if we need the element to be opposite, like logo etc`.
            if (!isOpposite)
                return lang.Rtl ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT;

            return lang.Rtl ? Element.ALIGN_LEFT : Element.ALIGN_RIGHT;
        }

        /// <summary>
        /// Get PDF cell
        /// </summary>
        /// <param name="resourceKey">Locale</param>
        /// <param name="lang">Language</param>
        /// <param name="font">Font</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the pDF cell
        /// </returns>
        protected virtual async Task<PdfPCell> GetPdfCellAsync(string resourceKey, Language lang, Font font,bool needCapital = false)
        {
            return new PdfPCell(new Phrase(await _localizationService.GetResourceAsync(resourceKey, lang.Id,needCapital:needCapital), font));
        }

        /// <summary>
        /// Get PDF cell
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="font">Font</param>
        /// <returns>PDF cell</returns>
        protected virtual PdfPCell GetPdfCell(object text, Font font)
        {
            return new PdfPCell(new Phrase(text.ToString(), font));
        }

        /// <summary>
        /// Get paragraph
        /// </summary>
        /// <param name="resourceKey">Locale</param>
        /// <param name="lang">Language</param>
        /// <param name="font">Font</param>
        /// <param name="args">Locale arguments</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the paragraph
        /// </returns>
        protected virtual async Task<Paragraph> GetParagraphAsync(string resourceKey, Language lang, Font font, params object[] args)
        {
            return await GetParagraphAsync(resourceKey, string.Empty, lang, font, args);
        }

        /// <summary>
        /// Get paragraph
        /// </summary>
        /// <param name="resourceKey">Locale</param>
        /// <param name="indent">Indent</param>
        /// <param name="lang">Language</param>
        /// <param name="font">Font</param>
        /// <param name="args">Locale arguments</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the paragraph
        /// </returns>
        protected virtual async Task<Paragraph> GetParagraphAsync(string resourceKey, string indent, Language lang, Font font, params object[] args)
        {
            var formatText = await _localizationService.GetResourceAsync(resourceKey, lang.Id);
            return new Paragraph(indent + (args.Any() ? string.Format(formatText, args) : formatText), font);
        }

        /// <summary>
        /// Print footer
        /// </summary>
        /// <param name="pdfSettingsByStore">PDF settings</param>
        /// <param name="pdfWriter">PDF writer</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="lang">Language</param>
        /// <param name="font">Font</param>
        protected virtual void PrintFooter(PdfSettings pdfSettingsByStore, PdfWriter pdfWriter, Rectangle pageSize, Language lang, Font font)
        {
            if (string.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn1) && string.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn2))
                return;

            var column1Lines = string.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn1)
                ? new List<string>()
                : pdfSettingsByStore.InvoiceFooterTextColumn1
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            var column2Lines = string.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn2)
                ? new List<string>()
                : pdfSettingsByStore.InvoiceFooterTextColumn2
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

            if (!column1Lines.Any() && !column2Lines.Any())
                return;

            var totalLines = Math.Max(column1Lines.Count, column2Lines.Count);
            const float margin = 43;

            //if you have really a lot of lines in the footer, then replace 9 with 10 or 11
            var footerHeight = totalLines * 9;
            var directContent = pdfWriter.DirectContent;
            directContent.MoveTo(pageSize.GetLeft(margin), pageSize.GetBottom(margin) + footerHeight);
            directContent.LineTo(pageSize.GetRight(margin), pageSize.GetBottom(margin) + footerHeight);
            directContent.Stroke();

            var footerTable = new PdfPTable(2)
            {
                WidthPercentage = 100f,
                RunDirection = GetDirection(lang)
            };
            footerTable.SetTotalWidth(new float[] { 250, 250 });

            //column 1
            if (column1Lines.Any())
            {
                var column1 = new PdfPCell(new Phrase())
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };

                foreach (var footerLine in column1Lines)
                {
                    column1.Phrase.Add(new Phrase(footerLine, font));
                    column1.Phrase.Add(new Phrase(Environment.NewLine));
                }

                footerTable.AddCell(column1);
            }
            else
            {
                var column = new PdfPCell(new Phrase(" ")) { Border = Rectangle.NO_BORDER };
                footerTable.AddCell(column);
            }

            //column 2
            if (column2Lines.Any())
            {
                var column2 = new PdfPCell(new Phrase())
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };

                foreach (var footerLine in column2Lines)
                {
                    column2.Phrase.Add(new Phrase(footerLine, font));
                    column2.Phrase.Add(new Phrase(Environment.NewLine));
                }

                footerTable.AddCell(column2);
            }
            else
            {
                var column = new PdfPCell(new Phrase(" ")) { Border = Rectangle.NO_BORDER };
                footerTable.AddCell(column);
            }

            footerTable.WriteSelectedRows(0, totalLines, pageSize.GetLeft(margin), pageSize.GetBottom(margin) + footerHeight, directContent);
        }

        /// <summary>
        /// Print order notes
        /// </summary>
        /// <param name="pdfSettingsByStore">PDF settings</param>
        /// <param name="order">Order</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">Document</param>
        /// <param name="font">Font</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrintOrderNotesAsync(PdfSettings pdfSettingsByStore, Order order, Language lang, Font titleFont, Document doc, Font font)
        {
            if (!pdfSettingsByStore.RenderOrderNotes)
                return;

            var orderNotes = (await _orderService.GetOrderNotesByOrderIdAsync(order.Id, true))
                .OrderByDescending(on => on.CreatedOnUtc)
                .ToList();

            if (!orderNotes.Any())
                return;

            var notesHeader = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var cellOrderNote = await GetPdfCellAsync("PDFInvoice.OrderNotes", lang, titleFont);
            cellOrderNote.Border = Rectangle.NO_BORDER;
            notesHeader.AddCell(cellOrderNote);
            doc.Add(notesHeader);
            doc.Add(new Paragraph(" "));

            var notesTable = new PdfPTable(2)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            notesTable.SetWidths(lang.Rtl ? new[] { 70, 30 } : new[] { 30, 70 });

            //created on
            cellOrderNote = await GetPdfCellAsync("PDFInvoice.OrderNotes.CreatedOn", lang, font);
            cellOrderNote.BackgroundColor = BaseColor.LightGray;
            cellOrderNote.HorizontalAlignment = Element.ALIGN_CENTER;
            notesTable.AddCell(cellOrderNote);

            //note
            cellOrderNote = await GetPdfCellAsync("PDFInvoice.OrderNotes.Note", lang, font);
            cellOrderNote.BackgroundColor = BaseColor.LightGray;
            cellOrderNote.HorizontalAlignment = Element.ALIGN_CENTER;
            notesTable.AddCell(cellOrderNote);

            foreach (var orderNote in orderNotes)
            {
                cellOrderNote = GetPdfCell(await _dateTimeHelper.ConvertToUserTimeAsync(orderNote.CreatedOnUtc, DateTimeKind.Utc), font);
                cellOrderNote.HorizontalAlignment = Element.ALIGN_LEFT;
                notesTable.AddCell(cellOrderNote);

                cellOrderNote = GetPdfCell(HtmlHelper.ConvertHtmlToPlainText(_orderService.FormatOrderNoteText(orderNote), true, true), font);
                cellOrderNote.HorizontalAlignment = Element.ALIGN_LEFT;
                notesTable.AddCell(cellOrderNote);

                //should we display a link to downloadable files here?
                //I think, no. Anyway, PDFs are printable documents and links (files) are useful here
            }

            doc.Add(notesTable);
        }

        /// <summary>
        /// Print totals
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">PDF document</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrintTotalsAsync(int vendorId, Language lang, Order order, Font font, Font titleFont, Document doc)
        {
            //vendors cannot see totals
            if (vendorId != 0)
                return;

            //subtotal
            var totalsTable = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            totalsTable.DefaultCell.Border = Rectangle.NO_BORDER;

            var languageId = lang.Id;

            //order subtotal
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax &&
                !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                //including tax

                var orderSubtotalInclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                var orderSubtotalInclTaxStr = await _priceFormatter.FormatPriceAsync(orderSubtotalInclTaxInCustomerCurrency, true,
                    order.CustomerCurrencyCode, languageId, true);

                var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Sub-Total", languageId)} {orderSubtotalInclTaxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }
            else
            {
                //excluding tax

                var orderSubtotalExclTaxInCustomerCurrency = 
                    _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                var orderSubtotalExclTaxStr = await _priceFormatter.FormatPriceAsync(orderSubtotalExclTaxInCustomerCurrency, true,
                    order.CustomerCurrencyCode, languageId, false);

                var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Sub-Total", languageId)} {orderSubtotalExclTaxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //discount (applied to order subtotal)
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
            {
                //order subtotal
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax &&
                    !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
                {
                    //including tax

                    var orderSubTotalDiscountInclTaxInCustomerCurrency =  
                        _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                    var orderSubTotalDiscountInCustomerCurrencyStr = await _priceFormatter.FormatPriceAsync(
                        -orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);

                    var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Discount", languageId)} {orderSubTotalDiscountInCustomerCurrencyStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
                else
                {
                    //excluding tax

                    var orderSubTotalDiscountExclTaxInCustomerCurrency =  
                        _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                    var orderSubTotalDiscountInCustomerCurrencyStr = await _priceFormatter.FormatPriceAsync(
                        -orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);

                    var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Discount", languageId)} {orderSubTotalDiscountInCustomerCurrencyStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            //shipping
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var orderShippingInclTaxInCustomerCurrency =  
                        _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                    var orderShippingInclTaxStr = await _priceFormatter.FormatShippingPriceAsync(
                        orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);

                    var p = GetPdfCell($"{ await _localizationService.GetResourceAsync("PDFInvoice.Shipping", languageId)} {orderShippingInclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
                else
                {
                    //excluding tax
                    var orderShippingExclTaxInCustomerCurrency =  
                        _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                    var orderShippingExclTaxStr =  await _priceFormatter.FormatShippingPriceAsync(
                        orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);

                    var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Shipping", languageId)} {orderShippingExclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            //payment fee
            if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
            {
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var paymentMethodAdditionalFeeInclTaxInCustomerCurrency =  
                        _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                    var paymentMethodAdditionalFeeInclTaxStr = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(
                        paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);

                    var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.PaymentMethodAdditionalFee", languageId)} {paymentMethodAdditionalFeeInclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
                else
                {
                    //excluding tax
                    var paymentMethodAdditionalFeeExclTaxInCustomerCurrency =  
                        _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                    var paymentMethodAdditionalFeeExclTaxStr =  await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(
                        paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);

                    var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.PaymentMethodAdditionalFee", languageId)} {paymentMethodAdditionalFeeExclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            //tax
            var taxStr = string.Empty;
            var taxRates = new SortedDictionary<decimal, decimal>();
            bool displayTax;
            var displayTaxRates = true;
            if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
            }
            else
            {
                if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    taxRates = _orderService.ParseTaxRates(order, order.TaxRates);

                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    taxStr = await _priceFormatter.FormatPriceAsync(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        false, languageId);
                }
            }

            if (displayTax)
            {
                var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Tax", languageId)} {taxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            if (displayTaxRates)
            {
                foreach (var item in taxRates)
                {
                    var taxRate = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.TaxRate", languageId),
                        _priceFormatter.FormatTaxRate(item.Key));
                    var taxValue = await _priceFormatter.FormatPriceAsync(
                        _currencyService.ConvertCurrency(item.Value, order.CurrencyRate), true, order.CustomerCurrencyCode,
                        false, languageId);

                    var p = GetPdfCell($"{taxRate} {taxValue}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            //discount (applied to order total)
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency =  
                    _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                var orderDiscountInCustomerCurrencyStr = await _priceFormatter.FormatPriceAsync(-orderDiscountInCustomerCurrency,
                    true, order.CustomerCurrencyCode, false, languageId);

                var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Discount", languageId)} {orderDiscountInCustomerCurrencyStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //gift cards
            foreach (var gcuh in await _giftCardService.GetGiftCardUsageHistoryAsync(order))
            {
                var gcTitle = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.GiftCardInfo", languageId),
                    (await _giftCardService.GetGiftCardByIdAsync(gcuh.GiftCardId))?.GiftCardCouponCode);
                var gcAmountStr = await _priceFormatter.FormatPriceAsync(
                    -_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate), true,
                    order.CustomerCurrencyCode, false, languageId);

                var p = GetPdfCell($"{gcTitle} {gcAmountStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //reward points
            if (order.RedeemedRewardPointsEntryId.HasValue && await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(order.RedeemedRewardPointsEntryId.Value) is RewardPointsHistory redeemedRewardPointsEntry)
            {
                var rpTitle = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.RewardPoints", languageId),
                    -redeemedRewardPointsEntry.Points);
                var rpAmount = await _priceFormatter.FormatPriceAsync(
                    -_currencyService.ConvertCurrency(redeemedRewardPointsEntry.UsedAmount, order.CurrencyRate),
                    true, order.CustomerCurrencyCode, false, languageId);

                var p = GetPdfCell($"{rpTitle} {rpAmount}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //order total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            var orderTotalStr = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);

            var pTotal = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.OrderTotal", languageId)} {orderTotalStr}", titleFont);
            pTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
            pTotal.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(pTotal);

            doc.Add(totalsTable);
        }

        /// <summary>
        /// Print checkout attributes
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="order">Order</param>
        /// <param name="doc">Document</param>
        /// <param name="lang">Language</param>
        /// <param name="font">Font</param>
        protected virtual void PrintCheckoutAttributes(int vendorId, Order order, Document doc, Language lang, Font font)
        {
            //vendors cannot see checkout attributes
            if (vendorId != 0 || string.IsNullOrEmpty(order.CheckoutAttributeDescription))
                return;

            doc.Add(new Paragraph(" "));
            var attribTable = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var cCheckoutAttributes = GetPdfCell(HtmlHelper.ConvertHtmlToPlainText(order.CheckoutAttributeDescription, true, true), font);
            cCheckoutAttributes.Border = Rectangle.NO_BORDER;
            cCheckoutAttributes.HorizontalAlignment = Element.ALIGN_RIGHT;
            attribTable.AddCell(cCheckoutAttributes);
            doc.Add(attribTable);
        }

        /// <summary>
        /// Print products
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">Document</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="attributesFont">Product attributes font</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrintProductsAsync(int vendorId, Language lang, Font titleFont, Document doc, Order order, Font font, Font attributesFont)
        {
            var productsHeader = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            var cellProducts = await GetPdfCellAsync("PDFInvoice.Product(s)", lang, titleFont);
            cellProducts.Border = Rectangle.NO_BORDER;
            productsHeader.AddCell(cellProducts);
            doc.Add(productsHeader);
            doc.Add(new Paragraph(" "));

            //a vendor should have access only to products
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, vendorId: vendorId);

            var count = 4 + (_catalogSettings.ShowSkuOnProductDetailsPage ? 1 : 0)
                        + (_vendorSettings.ShowVendorOnOrderDetailsPage ? 1 : 0);

            var productsTable = new PdfPTable(count)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { 4, new[] { 50, 20, 10, 20 } },
                { 5, new[] { 45, 15, 15, 10, 15 } },
                { 6, new[] { 40, 13, 13, 12, 10, 12 } }
            };

            productsTable.SetWidths(lang.Rtl ? widths[count].Reverse().ToArray() : widths[count]);

            //product name
            var cellProductItem = await GetPdfCellAsync("PDFInvoice.ProductName", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //SKU
            if (_catalogSettings.ShowSkuOnProductDetailsPage)
            {
                cellProductItem = await GetPdfCellAsync("PDFInvoice.SKU", lang, font);
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
            }

            //Vendor name
            if (_vendorSettings.ShowVendorOnOrderDetailsPage)
            {
                cellProductItem = await GetPdfCellAsync("PDFInvoice.VendorName", lang, font);
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
            }

            //price
            cellProductItem = await GetPdfCellAsync("PDFInvoice.ProductPrice", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //qty
            cellProductItem = await GetPdfCellAsync("PDFInvoice.ProductQuantity", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //total
            cellProductItem = await GetPdfCellAsync("PDFInvoice.ProductTotal", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            var vendors = _vendorSettings.ShowVendorOnOrderDetailsPage ? await _vendorService.GetVendorsByProductIdsAsync(orderItems.Select(item => item.ProductId).ToArray()) : new List<Vendor>();

            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                var pAttribTable = new PdfPTable(1) { RunDirection = GetDirection(lang) };
                pAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

                //product name
                var name = await _localizationService.GetLocalizedAsync(product, x => x.Name, lang.Id);
                pAttribTable.AddCell(new Paragraph(name, font));
                cellProductItem.AddElement(new Paragraph(name, font));
                //attributes
                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    var attributesParagraph =
                        new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true),
                            attributesFont);
                    pAttribTable.AddCell(attributesParagraph);
                }

                //rental info
                if (product.IsRental)
                {
                    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalStartDateUtc.Value)
                        : string.Empty;
                    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalEndDateUtc.Value)
                        : string.Empty;
                    var rentalInfo = string.Format(await _localizationService.GetResourceAsync("Order.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);

                    var rentalInfoParagraph = new Paragraph(rentalInfo, attributesFont);
                    pAttribTable.AddCell(rentalInfoParagraph);
                }

                productsTable.AddCell(pAttribTable);

                //SKU
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                {
                    var sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml);
                    cellProductItem = GetPdfCell(sku ?? string.Empty, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cellProductItem);
                }

                //Vendor name
                if (_vendorSettings.ShowVendorOnOrderDetailsPage)
                {
                    var vendorName = vendors.FirstOrDefault(v => v.Id == product.VendorId)?.Name ?? string.Empty;
                    cellProductItem = GetPdfCell(vendorName, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cellProductItem);
                }

                //price
                string unitPrice;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency =  
                        _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                    unitPrice = await _priceFormatter.FormatPriceAsync(unitPriceInclTaxInCustomerCurrency, true,
                        order.CustomerCurrencyCode, lang.Id, true);
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency =  
                        _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                    unitPrice = await _priceFormatter.FormatPriceAsync(unitPriceExclTaxInCustomerCurrency, true,
                        order.CustomerCurrencyCode, lang.Id, false);
                }

                cellProductItem = GetPdfCell(unitPrice, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);

                //qty
                cellProductItem = GetPdfCell(orderItem.Quantity, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);

                //total
                string subTotal;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var priceInclTaxInCustomerCurrency =  
                        _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                    subTotal = await _priceFormatter.FormatPriceAsync(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        lang.Id, true);
                }
                else
                {
                    //excluding tax
                    var priceExclTaxInCustomerCurrency =  
                        _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                    subTotal = await _priceFormatter.FormatPriceAsync(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        lang.Id, false);
                }

                cellProductItem = GetPdfCell(subTotal, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);
            }

            doc.Add(productsTable);
        }

        /// <summary>
        /// Print addresses
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="doc">Document</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrintAddressesAsync(int vendorId, Language lang, Font titleFont, Order order, Font font, Document doc)
        {
            var addressTable = new PdfPTable(2) { RunDirection = GetDirection(lang) };
            addressTable.DefaultCell.Border = Rectangle.NO_BORDER;
            addressTable.WidthPercentage = 100f;
            addressTable.SetWidths(new[] { 50, 50 });

            //billing info
            await PrintBillingInfoAsync(vendorId, lang, titleFont, order, font, addressTable);

            //shipping info
            await PrintShippingInfoAsync(lang, order, titleFont, font, addressTable);

            doc.Add(addressTable);
            doc.Add(new Paragraph(" "));
        }

        /// <summary>
        /// Print shipping info
        /// </summary>
        /// <param name="lang">Language</param>
        /// <param name="order">Order</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="font">Text font</param>
        /// <param name="addressTable">PDF table for address</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrintShippingInfoAsync(Language lang, Order order, Font titleFont, Font font, PdfPTable addressTable)
        {
            var shippingAddressPdf = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang)
            };
            shippingAddressPdf.DefaultCell.Border = Rectangle.NO_BORDER;

            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                //cell = new PdfPCell();
                //cell.Border = Rectangle.NO_BORDER;
                const string indent = "   ";

                if (!order.PickupInStore)
                {
                    if (order.ShippingAddressId == null || await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value) is not Address shippingAddress)
                        throw new NopException($"Shipping is required, but address is not available. Order ID = {order.Id}");

                    shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.ShippingInformation", lang, titleFont));
                    if (!string.IsNullOrEmpty(shippingAddress.Company))
                        shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Company", indent, lang, font, shippingAddress.Company));
                    shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Name", indent, lang, font, shippingAddress.FirstName + " " + shippingAddress.LastName));
                    if (_addressSettings.PhoneEnabled)
                        shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Phone", indent, lang, font, shippingAddress.PhoneNumber));
                    if (_addressSettings.FaxEnabled && !string.IsNullOrEmpty(shippingAddress.FaxNumber))
                        shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Fax", indent, lang, font, shippingAddress.FaxNumber));
                    if (_addressSettings.StreetAddressEnabled)
                        shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Address", indent, lang, font, shippingAddress.Address1));
                    if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(shippingAddress.Address2))
                        shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Address2", indent, lang, font, shippingAddress.Address2));
                    if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                        _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
                    {
                        var addressLine = $"{indent}{shippingAddress.City}, " +
                            $"{(!string.IsNullOrEmpty(shippingAddress.County) ? $"{shippingAddress.County}, " : string.Empty)}" +
                            $"{(await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                            $"{shippingAddress.ZipPostalCode}";
                        shippingAddressPdf.AddCell(new Paragraph(addressLine, font));
                    }

                    if (_addressSettings.CountryEnabled &&  await _countryService.GetCountryByAddressAsync(shippingAddress) is Country country)
                    {
                        shippingAddressPdf.AddCell(
                            new Paragraph(indent + await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id), font));
                    }
                    //custom attributes
                    var customShippingAddressAttributes = await _addressAttributeFormatter
                        .FormatAttributesAsync(shippingAddress.CustomAttributes, $"<br />{indent}");
                    if (!string.IsNullOrEmpty(customShippingAddressAttributes))
                    {
                        var text = HtmlHelper.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true);
                        shippingAddressPdf.AddCell(new Paragraph(indent + text, font));
                    }

                    shippingAddressPdf.AddCell(new Paragraph(" "));
                }
                else if (order.PickupAddressId.HasValue && await _addressService.GetAddressByIdAsync(order.PickupAddressId.Value) is Address pickupAddress)
                {
                    shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Pickup", lang, titleFont));

                    if (!string.IsNullOrEmpty(pickupAddress.Address1))
                        shippingAddressPdf.AddCell(new Paragraph(
                            $"{indent}{string.Format(await _localizationService.GetResourceAsync("PDFInvoice.Address", lang.Id), pickupAddress.Address1)}",
                            font));

                    if (!string.IsNullOrEmpty(pickupAddress.City))
                        shippingAddressPdf.AddCell(new Paragraph($"{indent}{pickupAddress.City}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.County))
                        shippingAddressPdf.AddCell(new Paragraph($"{indent}{pickupAddress.County}", font));

                    if (await _countryService.GetCountryByAddressAsync(pickupAddress) is Country country)
                        shippingAddressPdf.AddCell(
                            new Paragraph($"{indent}{await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id)}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.ZipPostalCode))
                        shippingAddressPdf.AddCell(new Paragraph($"{indent}{pickupAddress.ZipPostalCode}", font));

                    shippingAddressPdf.AddCell(new Paragraph(" "));
                }

                shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.ShippingMethod", indent, lang, font, order.ShippingMethod));
                shippingAddressPdf.AddCell(new Paragraph());

                addressTable.AddCell(shippingAddressPdf);
            }
            else
            {
                shippingAddressPdf.AddCell(new Paragraph());
                addressTable.AddCell(shippingAddressPdf);
            }
        }

        /// <summary>
        /// Print billing info
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="addressTable">Address PDF table</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrintBillingInfoAsync(int vendorId, Language lang, Font titleFont, Order order, Font font, PdfPTable addressTable)
        {
            const string indent = "   ";
            var billingAddressPdf = new PdfPTable(1) { RunDirection = GetDirection(lang) };
            billingAddressPdf.DefaultCell.Border = Rectangle.NO_BORDER;

            billingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.BillingInformation", lang, titleFont));

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

            if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(billingAddress.Company))
                billingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Company", indent, lang, font, billingAddress.Company));

            billingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Name", indent, lang, font, billingAddress.FirstName + " " + billingAddress.LastName));

            if (_addressSettings.PhoneEnabled)
                billingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Phone", indent, lang, font, billingAddress.PhoneNumber));

            if (_addressSettings.FaxEnabled && !string.IsNullOrEmpty(billingAddress.FaxNumber))
                billingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Fax", indent, lang, font, billingAddress.FaxNumber));

            if (_addressSettings.StreetAddressEnabled)
                billingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Address", indent, lang, font, billingAddress.Address1));

            if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(billingAddress.Address2))
                billingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Address2", indent, lang, font, billingAddress.Address2));

            if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
            {
                var addressLine = $"{indent}{billingAddress.City}, " +
                    $"{(!string.IsNullOrEmpty(billingAddress.County) ? $"{billingAddress.County}, " : string.Empty)}" +
                    $"{(await _stateProvinceService.GetStateProvinceByAddressAsync(billingAddress) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                    $"{billingAddress.ZipPostalCode}";
                billingAddressPdf.AddCell(new Paragraph(addressLine, font));
            }

            if (_addressSettings.CountryEnabled && await _countryService.GetCountryByAddressAsync(billingAddress) is Country country)
                billingAddressPdf.AddCell(new Paragraph(indent + await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id), font));

            //VAT number
            if (!string.IsNullOrEmpty(order.VatNumber))
                billingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.VATNumber", indent, lang, font, order.VatNumber));

            //custom attributes
            var customBillingAddressAttributes = await _addressAttributeFormatter
                .FormatAttributesAsync(billingAddress.CustomAttributes, $"<br />{indent}");
            if (!string.IsNullOrEmpty(customBillingAddressAttributes))
            {
                var text = HtmlHelper.ConvertHtmlToPlainText(customBillingAddressAttributes, true, true);
                billingAddressPdf.AddCell(new Paragraph(indent + text, font));
            }

            //vendors payment details
            if (vendorId == 0)
            {
                //payment method
                var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(order.PaymentMethodSystemName);
                var paymentMethodStr = paymentMethod != null
                    ? await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, lang.Id)
                    : order.PaymentMethodSystemName;
                if (!string.IsNullOrEmpty(paymentMethodStr))
                {
                    billingAddressPdf.AddCell(new Paragraph(" "));
                    billingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.PaymentMethod", indent, lang, font, paymentMethodStr));
                    billingAddressPdf.AddCell(new Paragraph());
                }

                //custom values
                var customValues = _paymentService.DeserializeCustomValues(order);
                if (customValues != null)
                {
                    foreach (var item in customValues)
                    {
                        billingAddressPdf.AddCell(new Paragraph(" "));
                        billingAddressPdf.AddCell(new Paragraph(indent + item.Key + ": " + item.Value, font));
                        billingAddressPdf.AddCell(new Paragraph());
                    }
                }
            }

            addressTable.AddCell(billingAddressPdf);
        }

        /// <summary>
        /// Print header
        /// </summary>
        /// <param name="pdfSettingsByStore">PDF settings</param>
        /// <param name="lang">Language</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">Document</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrintHeaderAsync(PdfSettings pdfSettingsByStore, Language lang, Order order, Font font, Font titleFont, Document doc)
        {
            //logo
            var logoPicture = await _pictureService.GetPictureByIdAsync(pdfSettingsByStore.LogoPictureId);
            var logoExists = logoPicture != null;

            //header
            var headerTable = new PdfPTable(logoExists ? 2 : 1)
            {
                RunDirection = GetDirection(lang)
            };
            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;

            //store info
            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            var anchor = new Anchor(store.Url.Trim('/'), font)
            {
                Reference = store.Url
            };

            var cellHeader = GetPdfCell(string.Format(await _localizationService.GetResourceAsync("PDFInvoice.Order#", lang.Id), order.CustomOrderNumber), titleFont);
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(anchor));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(await GetParagraphAsync("PDFInvoice.OrderDate", lang, font, (await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc)).ToString("D", new CultureInfo(lang.LanguageCulture))));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;
            cellHeader.Border = Rectangle.NO_BORDER;

            headerTable.AddCell(cellHeader);

            if (logoExists)
                headerTable.SetWidths(lang.Rtl ? new[] { 0.2f, 0.8f } : new[] { 0.8f, 0.2f });
            headerTable.WidthPercentage = 100f;

            //logo               
            if (logoExists)
            {
                var logoFilePath = await _pictureService.GetThumbLocalPathAsync(logoPicture, 0, false);
                var logo = Image.GetInstance(logoFilePath);
                logo.Alignment = GetAlignment(lang, true);
                logo.ScaleToFit(65f, 65f);

                var cellLogo = new PdfPCell { Border = Rectangle.NO_BORDER };
                cellLogo.AddElement(logo);
                headerTable.AddCell(cellLogo);
            }

            doc.Add(headerTable);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Print an order to PDF
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        /// <param name="vendorId">Vendor identifier to limit products; 0 to print all products. If specified, then totals won't be printed</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains a path of generated file
        /// </returns>
        public virtual async Task<string> PrintOrderToPdfAsync(Order order, int languageId = 0, int vendorId = 0)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var fileName = $"order_{order.OrderGuid}_{CommonHelper.GenerateRandomDigitCode(4)}.pdf";
            var filePath = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/exportimport"), fileName);
            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                var orders = new List<Order> { order };
                await PrintOrdersToPdfAsync(fileStream, orders, languageId, vendorId);
            }

            return filePath;
        }

        /// <summary>
        /// Print orders to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="orders">Orders</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        /// <param name="vendorId">Vendor identifier to limit products; 0 to print all products. If specified, then totals won't be printed</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task PrintOrdersToPdfAsync(Stream stream, IList<Order> orders, int languageId = 0, int vendorId = 0)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (orders == null)
                throw new ArgumentNullException(nameof(orders));

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled) 
                pageSize = PageSize.Letter;

            var doc = new Document(pageSize);
            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            var ordCount = orders.Count;
            var ordNum = 0;

            foreach (var order in orders)
            {
                //by default _pdfSettings contains settings for the current active store
                //and we need PdfSettings for the store which was used to place an order
                //so let's load it based on a store of the current order
                var pdfSettingsByStore = await _settingService.LoadSettingAsync<PdfSettings>(order.StoreId);

                var lang = await _languageService.GetLanguageByIdAsync(languageId == 0 ? order.CustomerLanguageId : languageId);
                if (lang == null || !lang.Published)
                    lang = await _workContext.GetWorkingLanguageAsync();

                //header
                await PrintHeaderAsync(pdfSettingsByStore, lang, order, font, titleFont, doc);

                //addresses
                await PrintAddressesAsync(vendorId, lang, titleFont, order, font, doc);

                //products
                await PrintProductsAsync(vendorId, lang, titleFont, doc, order, font, attributesFont);

                //checkout attributes
                PrintCheckoutAttributes(vendorId, order, doc, lang, font);

                //totals
                await PrintTotalsAsync(vendorId, lang, order, font, titleFont, doc);

                //order notes
                await PrintOrderNotesAsync(pdfSettingsByStore, order, lang, titleFont, doc, font);

                //footer
                PrintFooter(pdfSettingsByStore, pdfWriter, pageSize, lang, font);

                ordNum++;
                if (ordNum < ordCount) 
                    doc.NewPage();
            }

            doc.Close();
        }

        /// <summary>
        /// Print packaging slips to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="shipments">Shipments</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task PrintPackagingSlipsToPdfAsync(Stream stream, IList<Shipment> shipments, int languageId = 0)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (shipments == null)
                throw new ArgumentNullException(nameof(shipments));

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            var shipmentCount = shipments.Count;
            var shipmentNum = 0;

            foreach (var shipment in shipments)
            {
                var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);

                var lang = await _languageService.GetLanguageByIdAsync(languageId == 0 ? order.CustomerLanguageId : languageId);
                if (lang == null || !lang.Published)
                    lang = await _workContext.GetWorkingLanguageAsync();

                var addressTable = new PdfPTable(1);
                if (lang.Rtl)
                    addressTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                addressTable.DefaultCell.Border = Rectangle.NO_BORDER;
                addressTable.WidthPercentage = 100f;

                addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Shipment", lang, titleFont, shipment.Id));
                addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Order", lang, titleFont, order.CustomOrderNumber));

                if (!order.PickupInStore)
                {
                    if (order.ShippingAddressId == null || await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value) is not Address shippingAddress)
                        throw new NopException($"Shipping is required, but address is not available. Order ID = {order.Id}");

                    if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(shippingAddress.Company))
                        addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Company", lang, font, shippingAddress.Company));

                    addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Name", lang, font, shippingAddress.FirstName + " " + shippingAddress.LastName));
                    if (_addressSettings.PhoneEnabled)
                        addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Phone", lang, font, shippingAddress.PhoneNumber));
                    if (_addressSettings.StreetAddressEnabled)
                        addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Address", lang, font, shippingAddress.Address1));

                    if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(shippingAddress.Address2))
                        addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Address2", lang, font, shippingAddress.Address2));

                    if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                        _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
                    {
                        var addressLine = $"{shippingAddress.City}, " +
                            $"{(!string.IsNullOrEmpty(shippingAddress.County) ? $"{shippingAddress.County}, " : string.Empty)}" +
                            $"{(await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                            $"{shippingAddress.ZipPostalCode}";
                        addressTable.AddCell(new Paragraph(addressLine, font));
                    }

                    if (_addressSettings.CountryEnabled && await _countryService.GetCountryByAddressAsync(shippingAddress) is Country country)
                        addressTable.AddCell(new Paragraph(await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id), font));

                    //custom attributes
                    var customShippingAddressAttributes = await _addressAttributeFormatter.FormatAttributesAsync(shippingAddress.CustomAttributes);
                    if (!string.IsNullOrEmpty(customShippingAddressAttributes))
                    {
                        addressTable.AddCell(new Paragraph(HtmlHelper.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true), font));
                    }
                }
                else
                    if (order.PickupAddressId.HasValue && await _addressService.GetAddressByIdAsync(order.PickupAddressId.Value) is Address pickupAddress)
                {
                    addressTable.AddCell(new Paragraph(await _localizationService.GetResourceAsync("PDFInvoice.Pickup", lang.Id), titleFont));

                    if (!string.IsNullOrEmpty(pickupAddress.Address1))
                        addressTable.AddCell(new Paragraph($"   {string.Format(await _localizationService.GetResourceAsync("PDFInvoice.Address", lang.Id), pickupAddress.Address1)}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.City))
                        addressTable.AddCell(new Paragraph($"   {pickupAddress.City}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.County))
                        addressTable.AddCell(new Paragraph($"   {pickupAddress.County}", font));

                    if (await _countryService.GetCountryByAddressAsync(pickupAddress) is Country country)
                        addressTable.AddCell(new Paragraph($"   {await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id)}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.ZipPostalCode))
                        addressTable.AddCell(new Paragraph($"   {pickupAddress.ZipPostalCode}", font));

                    addressTable.AddCell(new Paragraph(" "));
                }

                addressTable.AddCell(new Paragraph(" "));

                addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.ShippingMethod", lang, font, order.ShippingMethod));
                addressTable.AddCell(new Paragraph(" "));
                doc.Add(addressTable);

                var productsTable = new PdfPTable(3) { WidthPercentage = 100f };
                if (lang.Rtl)
                {
                    productsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    productsTable.SetWidths(new[] { 20, 20, 60 });
                }
                else
                {
                    productsTable.SetWidths(new[] { 60, 20, 20 });
                }

                //product name
                var cell = await GetPdfCellAsync("PDFPackagingSlip.ProductName", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                //SKU
                cell = await GetPdfCellAsync("PDFPackagingSlip.SKU", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                //qty
                cell = await GetPdfCellAsync("PDFPackagingSlip.QTY", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                foreach (var si in await _shipmentService.GetShipmentItemsByShipmentIdAsync(shipment.Id))
                {
                    var productAttribTable = new PdfPTable(1);
                    if (lang.Rtl)
                        productAttribTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    productAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

                    //product name
                    var orderItem = await _orderService.GetOrderItemByIdAsync(si.OrderItemId);
                    if (orderItem == null)
                        continue;

                    var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                    var name = await _localizationService.GetLocalizedAsync(product, x => x.Name, lang.Id);
                    productAttribTable.AddCell(new Paragraph(name, font));
                    //attributes
                    if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                    {
                        var attributesParagraph = new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true), attributesFont);
                        productAttribTable.AddCell(attributesParagraph);
                    }

                    //rental info
                    if (product.IsRental)
                    {
                        var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                            ? _productService.FormatRentalDate(product, orderItem.RentalStartDateUtc.Value) : string.Empty;
                        var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                            ? _productService.FormatRentalDate(product, orderItem.RentalEndDateUtc.Value) : string.Empty;
                        var rentalInfo = string.Format(await _localizationService.GetResourceAsync("Order.Rental.FormattedDate"),
                            rentalStartDate, rentalEndDate);

                        var rentalInfoParagraph = new Paragraph(rentalInfo, attributesFont);
                        productAttribTable.AddCell(rentalInfoParagraph);
                    }

                    productsTable.AddCell(productAttribTable);

                    //SKU
                    var sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml);
                    cell = GetPdfCell(sku ?? string.Empty, font);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);

                    //qty
                    cell = GetPdfCell(si.Quantity, font);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);
                }

                doc.Add(productsTable);

                shipmentNum++;
                if (shipmentNum < shipmentCount) 
                    doc.NewPage();
            }

            doc.Close();
        }

        /// <summary>
        /// Print products to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="products">Products</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task PrintProductsToPdfAsync(Stream stream, IList<Product> products)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (products == null)
                throw new ArgumentNullException(nameof(products));

            var lang = await _workContext.GetWorkingLanguageAsync();

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            var font = GetFont();

            var productNumber = 1;
            var prodCount = products.Count;

            foreach (var product in products)
            {
                var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name, lang.Id);
                var productDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription, lang.Id);

                var productTable = new PdfPTable(1) { WidthPercentage = 100f };
                productTable.DefaultCell.Border = Rectangle.NO_BORDER;
                if (lang.Rtl)
                {
                    productTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                }

                productTable.AddCell(new Paragraph($"{productNumber}. {productName}", titleFont));
                productTable.AddCell(new Paragraph(" "));
                productTable.AddCell(new Paragraph(HtmlHelper.StripTags(HtmlHelper.ConvertHtmlToPlainText(productDescription, decode: true)), font));
                productTable.AddCell(new Paragraph(" "));

                if (product.ProductType == ProductType.SimpleProduct)
                {
                    //simple product
                    //render its properties such as price, weight, etc
                    var priceStr = $"{product.Price:0.00} {(await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode}";
                    if (product.IsRental)
                        priceStr = await _priceFormatter.FormatRentalProductPeriodAsync(product, priceStr);
                    productTable.AddCell(new Paragraph($"{await _localizationService.GetResourceAsync("PDFProductCatalog.Price", lang.Id)}: {priceStr}", font));
                    productTable.AddCell(new Paragraph($"{await _localizationService.GetResourceAsync("PDFProductCatalog.SKU", lang.Id)}: {product.Sku}", font));

                    if (product.IsShipEnabled && product.Weight > decimal.Zero)
                        productTable.AddCell(new Paragraph($"{await _localizationService.GetResourceAsync("PDFProductCatalog.Weight", lang.Id)}: {product.Weight:0.00} {(await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId)).Name}", font));

                    if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                        productTable.AddCell(new Paragraph($"{await _localizationService.GetResourceAsync("PDFProductCatalog.StockQuantity", lang.Id)}: {await _productService.GetTotalStockQuantityAsync(product)}", font));

                    productTable.AddCell(new Paragraph(" "));
                }

                var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id);
                if (pictures.Any())
                {
                    var table = new PdfPTable(2) { WidthPercentage = 100f };
                    if (lang.Rtl) 
                        table.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    foreach (var pic in pictures)
                    {
                        var picBinary = await _pictureService.LoadPictureBinaryAsync(pic);
                        if (picBinary == null || picBinary.Length <= 0)
                            continue;

                        var pictureLocalPath = await _pictureService.GetThumbLocalPathAsync(pic, 200, false);
                        var cell = new PdfPCell(Image.GetInstance(pictureLocalPath))
                        {
                            HorizontalAlignment = Element.ALIGN_LEFT,
                            Border = Rectangle.NO_BORDER
                        };
                        table.AddCell(cell);
                    }

                    if (pictures.Count % 2 > 0)
                    {
                        var cell = new PdfPCell(new Phrase(" "))
                        {
                            Border = Rectangle.NO_BORDER
                        };
                        table.AddCell(cell);
                    }

                    productTable.AddCell(table);
                    productTable.AddCell(new Paragraph(" "));
                }

                if (product.ProductType == ProductType.GroupedProduct)
                {
                    //grouped product. render its associated products
                    var pvNum = 1;
                    foreach (var associatedProduct in await _productService.GetAssociatedProductsAsync(product.Id, showHidden: true))
                    {
                        productTable.AddCell(new Paragraph($"{productNumber}-{pvNum}. {await _localizationService.GetLocalizedAsync(associatedProduct, x => x.Name, lang.Id)}", font));
                        productTable.AddCell(new Paragraph(" "));

                        //uncomment to render associated product description
                        //string apDescription = associated_localizationService.GetLocalized(product, x => x.ShortDescription, lang.Id);
                        //if (!string.IsNullOrEmpty(apDescription))
                        //{
                        //    productTable.AddCell(new Paragraph(HtmlHelper.StripTags(HtmlHelper.ConvertHtmlToPlainText(apDescription)), font));
                        //    productTable.AddCell(new Paragraph(" "));
                        //}

                        //uncomment to render associated product picture
                        //var apPicture = _pictureService.GetPicturesByProductId(associatedProduct.Id).FirstOrDefault();
                        //if (apPicture != null)
                        //{
                        //    var picBinary = _pictureService.LoadPictureBinary(apPicture);
                        //    if (picBinary != null && picBinary.Length > 0)
                        //    {
                        //        var pictureLocalPath = _pictureService.GetThumbLocalPath(apPicture, 200, false);
                        //        productTable.AddCell(Image.GetInstance(pictureLocalPath));
                        //    }
                        //}

                        productTable.AddCell(new Paragraph($"{await _localizationService.GetResourceAsync("PDFProductCatalog.Price", lang.Id)}: {associatedProduct.Price:0.00} {(await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode}", font));
                        productTable.AddCell(new Paragraph($"{await _localizationService.GetResourceAsync("PDFProductCatalog.SKU", lang.Id)}: {associatedProduct.Sku}", font));

                        if (associatedProduct.IsShipEnabled && associatedProduct.Weight > decimal.Zero)
                            productTable.AddCell(new Paragraph($"{await _localizationService.GetResourceAsync("PDFProductCatalog.Weight", lang.Id)}: {associatedProduct.Weight:0.00} {(await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId)).Name}", font));

                        if (associatedProduct.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                            productTable.AddCell(new Paragraph($"{await _localizationService.GetResourceAsync("PDFProductCatalog.StockQuantity", lang.Id)}: {await _productService.GetTotalStockQuantityAsync(associatedProduct)}", font));

                        productTable.AddCell(new Paragraph(" "));

                        pvNum++;
                    }
                }

                doc.Add(productTable);

                productNumber++;

                if (productNumber <= prodCount)
                {
                    doc.NewPage();
                }
            }

            doc.Close();
        }

        #region QuoteRequestPdf

        /// <summary>
        /// Get paragraph
        /// </summary>
        /// <param name="resourceKey">Locale</param>
        /// <param name="indent">Indent</param>
        /// <param name="lang">Language</param>
        /// <param name="font">Font</param>
        /// <param name="args">Locale arguments</param>
        /// <returns>Paragraph</returns>
        protected virtual async Task<Paragraph> GetParagraphForQoute(string resourceKey, string indent, Language lang, Font font, params object[] args)
        {
            //var formatText = _localizationService.GetResource(resourceKey, lang.Id);
            var formatText =await _localizationService.GetResourceAsync(resourceKey, lang.Id);
            if (formatText == "email")
            {
                formatText = "email: {0} ";
            }

            return new Paragraph(indent + (args.Any() ? string.Format(formatText, args) : formatText), font);
        }


        /// <summary>
        /// Get element alignment
        /// </summary>
        /// <param name="lang">Language</param>
        /// <param name="isOpposite">Is opposite?</param>
        /// <returns>Element alignment</returns>
        protected virtual int GetAlignmentForQuote(Language lang, bool isOpposite = true)
        {
            //if we need the element to be opposite, like logo etc`.
            if (!isOpposite)
                return lang.Rtl ? Element.ALIGN_LEFT : Element.ALIGN_RIGHT;

            return lang.Rtl ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT;
        }

        /// <summary>
        /// Get font direction
        /// </summary>
        /// <param name="lang">Language</param>
        /// <returns>Font direction</returns>
        protected virtual int GetDirectionForQuote(Language lang)
        {
            return lang.Rtl ? PdfWriter.RUN_DIRECTION_LTR : PdfWriter.RUN_DIRECTION_RTL;
        }

        protected virtual int GetDirectiontable(Language lang)
        {
            return lang.Rtl ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR;
        }

        protected virtual async Task<Paragraph> GetParagraphdate(string resourceKey, Language lang, Font fontpre, params object[] args)
        {
            return await GetParagraphForQoute(resourceKey, string.Empty, lang, fontpre, args);
        }

        protected virtual async Task<Paragraph> GetParagraph1(string resourceKey, string indent, Language lang, Font fontpre, params object[] args)
        {
            //var formatText = _localizationService.GetResource(resourceKey, lang.Id);
            var formatText =await _localizationService.GetResourceAsync(resourceKey, lang.Id);
            if (formatText == "email")
            {
                formatText = "email: {0} ";
            }

            return new Paragraph(indent + (args.Any() ? string.Format(formatText, args) : formatText), fontpre);
        }

        /// <summary>
        /// Print quote billing info
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="addressTable">Address PDF table</param>
        //protected virtual void PrintQouteBillingInfo(int vendorId, Language lang, Font titleFont, Order order, Font font, PdfPTable addressTable, Font fontpre)
        protected virtual async Task PrintQouteBillingInfo(int vendorId, Language lang, Font titleFont, Order order, Font font, PdfPTable addressTable, Font fontpre)
        {
            const string indent = "   ";
            var billingAddressPdf = new PdfPTable(1) { RunDirection = 3 };
            billingAddressPdf.DefaultCell.Border = Rectangle.NO_BORDER;
            //var billingAddress = _addressService.GetAddressById(order.BillingAddressId);
            var billingAddress =await _addressService.GetAddressByIdAsync(order.BillingAddressId);

            if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(billingAddress.Company))
                billingAddressPdf.AddCell(await GetParagraphForQoute("PDFInvoice.Company", indent, lang, font, billingAddress.Company));
            if (_addressSettings.PhoneEnabled)
                billingAddressPdf.AddCell(await GetParagraphForQoute("PDFInvoice.Phone", indent, lang, font, billingAddress.PhoneNumber));

            if (_addressSettings.FaxEnabled && !string.IsNullOrEmpty(billingAddress.FaxNumber))
                billingAddressPdf.AddCell(await GetParagraphForQoute("PDFInvoice.Fax", indent, lang, font, billingAddress.FaxNumber));

            if (_addressSettings.StreetAddressEnabled)
                billingAddressPdf.AddCell(await GetParagraphForQoute("PDFInvoice.Address", indent, lang, font, billingAddress.Address1));

            if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(billingAddress.Address2))
                billingAddressPdf.AddCell(await GetParagraphForQoute("PDFInvoice.Address2", indent, lang, font, billingAddress.Address2));

            billingAddressPdf.AddCell(await GetParagraphForQoute("Email", indent, lang, font, billingAddress.Email));
            //if (_addressSettings.CountryEnabled && _countryService.GetCountryByAddress(billingAddress) is Country country)
            if (_addressSettings.CountryEnabled && await _countryService.GetCountryByAddressAsync(billingAddress) is Country country)
                //billingAddressPdf.AddCell(new Paragraph(indent + _localizationService.GetLocalized(country, x => x.Name, lang.Id), font));
                billingAddressPdf.AddCell(new Paragraph(indent +await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id), font));

            //VAT number
            if (!string.IsNullOrEmpty(order.VatNumber))
                billingAddressPdf.AddCell(await GetParagraphForQoute("", indent, lang, font, order.VatNumber));

            //custom attributes
            var customBillingAddressAttributes =await _addressAttributeFormatter
                //.FormatAttributes(billingAddress.CustomAttributes, $"<br />{indent}");
                .FormatAttributesAsync(billingAddress.CustomAttributes, $"<br />{indent}");
            if (!string.IsNullOrEmpty(customBillingAddressAttributes))
            {
                var text = HtmlHelper.ConvertHtmlToPlainText(customBillingAddressAttributes, true, true);
                billingAddressPdf.AddCell(new Paragraph(indent + text, font));
            }

            //vendors payment details
            if (vendorId == 0)
            {
                //payment method
                //var paymentMethod = _paymentPluginManager.LoadPluginBySystemName(order.PaymentMethodSystemName);
                var paymentMethod =await _paymentPluginManager.LoadPluginBySystemNameAsync(order.PaymentMethodSystemName);
                var paymentMethodStr = paymentMethod != null
                    //? _localizationService.GetLocalizedFriendlyName(paymentMethod, lang.Id)
                    ?await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, lang.Id)
                    : order.PaymentMethodSystemName;
                if (!string.IsNullOrEmpty(paymentMethodStr))
                {
                    billingAddressPdf.AddCell(new Paragraph(" "));
                    billingAddressPdf.AddCell(await GetParagraphForQoute("PDFInvoice.PaymentMethod", indent, lang, font, paymentMethodStr));
                    billingAddressPdf.AddCell(new Paragraph());
                }

                //custom values
                var customValues = _paymentService.DeserializeCustomValues(order);
                if (customValues != null)
                {
                    foreach (var item in customValues)
                    {
                        billingAddressPdf.AddCell(new Paragraph(" "));
                        billingAddressPdf.AddCell(new Paragraph(indent + item.Key + ": " + item.Value, font));
                        billingAddressPdf.AddCell(new Paragraph());
                    }
                }
            }

            addressTable.AddCell(billingAddressPdf);
        }

        /// <summary>
        /// Print quote shipping info
        /// </summary>
        /// <param name="lang">Language</param>
        /// <param name="order">Order</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="font">Text font</param>
        /// <param name="addressTable">PDF table for address</param>
        //protected virtual void PrintQuoteShippingInfo(Language lang, Order order, Font titleFont, Font font, Font fontpre, PdfPTable addressTable)
        protected virtual async Task PrintQuoteShippingInfo(Language lang, Order order, Font titleFont, Font font, Font fontpre, PdfPTable addressTable)
        {
            var shippingAddressPdf = new PdfPTable(1)
            {
                RunDirection = 2
            };
            shippingAddressPdf.DefaultCell.Border = Rectangle.NO_BORDER;

            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                const string indent = "   ";

                if (!order.PickupInStore)
                {
                    //if (order.ShippingAddressId == null || !(_addressService.GetAddressById(order.ShippingAddressId.Value) is Address shippingAddress))
                    if (order.ShippingAddressId == null || !(await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value) is Address shippingAddress))
                        throw new NopException($"Shipping is required, but address is not available. Order ID = {order.Id}");

                    if (!string.IsNullOrEmpty(shippingAddress.Company))
                        shippingAddressPdf.AddCell(await GetParagraphForQoute("PDFInvoice.Company", indent, lang, font, shippingAddress.Company));
                    shippingAddressPdf.AddCell(await GetParagraphForQoute("PDFInvoice.Name", indent, lang, font, shippingAddress.FirstName + " " + shippingAddress.LastName));
                    if (_addressSettings.PhoneEnabled)
                        shippingAddressPdf.AddCell(await GetParagraphForQoute("PDFInvoice.Phone", indent, lang, font, shippingAddress.PhoneNumber));
                    if (_addressSettings.FaxEnabled && !string.IsNullOrEmpty(shippingAddress.FaxNumber))
                        shippingAddressPdf.AddCell(await GetParagraphForQoute("PDFInvoice.Fax", indent, lang, font, shippingAddress.FaxNumber));
                    if (_addressSettings.StreetAddressEnabled)
                        shippingAddressPdf.AddCell(await GetParagraphForQoute("PDFInvoice.Address", indent, lang, font, shippingAddress.Address1));
                    if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(shippingAddress.Address2))
                        shippingAddressPdf.AddCell(await GetParagraphForQoute("PDFInvoice.Address2", indent, lang, font, shippingAddress.Address2));
                    shippingAddressPdf.AddCell(await GetParagraphForQoute("Email", indent, lang, font, shippingAddress.Email));

                    //if (_addressSettings.CountryEnabled && _countryService.GetCountryByAddress(shippingAddress) is Country country)
                    if (_addressSettings.CountryEnabled && await _countryService.GetCountryByAddressAsync(shippingAddress) is Country country)
                    {
                        shippingAddressPdf.AddCell(
                        //new Paragraph(indent + _localizationService.GetLocalized(country, x => x.Name, lang.Id), font));
                        new Paragraph(indent +await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id), font));
                    }
                    shippingAddressPdf.AddCell(await GetParagraphForQoute("", indent, lang, font, shippingAddress.Email));

                    //shippingAddressPdf.AddCell(await GetParagraphdate("PDFInvoice.OrderDate", lang, fontpre, _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc).ToString("D", new CultureInfo(lang.LanguageCulture))));
                    shippingAddressPdf.AddCell(await GetParagraphdate("PDFInvoice.OrderDate", lang, fontpre,(await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc)).ToString("D", new CultureInfo(lang.LanguageCulture))));

                    shippingAddressPdf.AddCell(await GetParagraph1("PDFInvoice.Order#", indent, lang, fontpre, order.CustomOrderNumber));

                    //custom attributes
                    var customShippingAddressAttributes =await _addressAttributeFormatter
                        //.FormatAttributes(shippingAddress.CustomAttributes, $"<br />{indent}");
                        .FormatAttributesAsync(shippingAddress.CustomAttributes, $"<br />{indent}");
                    if (!string.IsNullOrEmpty(customShippingAddressAttributes))
                    {
                        var text = HtmlHelper.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true);
                        shippingAddressPdf.AddCell(new Paragraph(indent + text, font));
                    }

                    shippingAddressPdf.AddCell(new Paragraph(" "));
                }
                //else if (order.PickupAddressId.HasValue && _addressService.GetAddressById(order.PickupAddressId.Value) is Address pickupAddress)
                else if (order.PickupAddressId.HasValue && await _addressService.GetAddressByIdAsync(order.PickupAddressId.Value) is Address pickupAddress)
                {
                    //shippingAddressPdf.AddCell(GetParagraph("PDFInvoice.Pickup", lang, titleFont));
                    shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Pickup", lang, titleFont));

                    if (!string.IsNullOrEmpty(pickupAddress.Address1))
                        shippingAddressPdf.AddCell(new Paragraph(
                            //$"{indent}{string.Format(_localizationService.GetResource("PDFInvoice.Address", lang.Id), pickupAddress.Address1)}",
                            $"{indent}{string.Format(await _localizationService.GetResourceAsync("PDFInvoice.Address", lang.Id), pickupAddress.Address1)}",
                            font));

                    if (!string.IsNullOrEmpty(pickupAddress.City))
                        shippingAddressPdf.AddCell(new Paragraph($"{indent}{pickupAddress.City}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.County))
                        shippingAddressPdf.AddCell(new Paragraph($"{indent}{pickupAddress.County}", font));

                    //if (_countryService.GetCountryByAddress(pickupAddress) is Country country)
                    if (await _countryService.GetCountryByAddressAsync(pickupAddress) is Country country)
                        shippingAddressPdf.AddCell(
                            //new Paragraph($"{indent}{_localizationService.GetLocalized(country, x => x.Name, lang.Id)}", font));
                            new Paragraph($"{indent}{await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id)}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.ZipPostalCode))
                        shippingAddressPdf.AddCell(new Paragraph($"{indent}{pickupAddress.ZipPostalCode}", font));


                    shippingAddressPdf.AddCell(new Paragraph(" "));
                }

                shippingAddressPdf.AddCell(new Paragraph());

                addressTable.AddCell(shippingAddressPdf);
            }
            else
            {
                shippingAddressPdf.AddCell(new Paragraph());
                addressTable.AddCell(shippingAddressPdf);
            }
        }

        //protected virtual void PrintInfo(int vendorId, Language lang, Font titleFont, Order order, Font font, PdfPTable addressTable)
        protected virtual async Task PrintInfo(int vendorId, Language lang, Font titleFont, Order order, Font font, PdfPTable addressTable)
        {
            const string indent = "   ";
            var billingAddressPdf = new PdfPTable(1) { RunDirection = 0 };
            billingAddressPdf.DefaultCell.Border = Rectangle.NO_BORDER;

            //billingAddressPdf.AddCell(GetParagraph("", lang, titleFont));
            billingAddressPdf.AddCell(await GetParagraphAsync("", lang, titleFont));

            //var billingAddress = _addressService.GetAddressById(order.BillingAddressId);
            var billingAddress =await _addressService.GetAddressByIdAsync(order.BillingAddressId);


            if (_addressSettings.PhoneEnabled)
                billingAddressPdf.AddCell(await GetParagraphForQoute("PDFInvoice.Phone", indent, lang, font, billingAddress.PhoneNumber));

            //  if (_addressSettings.FaxEnabled && !string.IsNullOrEmpty(billingAddress.FaxNumber))


            // if (_addressSettings.StreetAddressEnabled)
            billingAddressPdf.AddCell(await GetParagraphForQoute("Email", indent, lang, font, billingAddress.Email));


            addressTable.AddCell(billingAddressPdf);
        }

        //protected virtual void PrintQuoteHeader(PdfSettings pdfSettingsByStore, Language lang, Order order, Font font, Font titleFont, Document doc)
        protected virtual async Task PrintQuoteHeader(PdfSettings pdfSettingsByStore, Language lang, Order order, Font font, Font titleFont, Document doc)
        {
            //logo
            //var logoPicture = _pictureService.GetPictureById(pdfSettingsByStore.LogoPictureId);
            var logoPicture =await _pictureService.GetPictureByIdAsync(pdfSettingsByStore.LogoPictureId);
            var logoExists = logoPicture != null;



            //header
            var headerTable = new PdfPTable(logoExists ? 2 : 1)
            {
                RunDirection = GetDirectionForQuote(lang)
            };
            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;

            //store info
            //var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            var store =await _storeService.GetStoreByIdAsync(order.StoreId) ??await _storeContext.GetCurrentStoreAsync();
            var anchor = new Anchor(store.Url.Trim('/'), font)
            {
                Reference = store.Url
            };

            //var cellHeader = GetPdfCell(string.Format(_localizationService.GetResource("", lang.Id), order.CustomOrderNumber), titleFont);
            var cellHeader = GetPdfCell(string.Format(await _localizationService.GetResourceAsync("", lang.Id), order.CustomOrderNumber), titleFont);
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add("Quotation");
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.HorizontalAlignment = Element.ALIGN_RIGHT;
            cellHeader.Border = Rectangle.NO_BORDER;


            headerTable.AddCell(cellHeader);

            if (logoExists)
                headerTable.SetWidths(lang.Rtl ? new[] { 0.2f, 0.8f } : new[] { 0.8f, 0.2f });
            headerTable.WidthPercentage = 100f;

            //logo               
            if (logoExists)
            {
                //var logoFilePath = _pictureService.GetThumbLocalPath(logoPicture, 0, false);
                var logoFilePath =await _pictureService.GetThumbLocalPathAsync(logoPicture, 0, false);
                var logo = Image.GetInstance(logoFilePath);
                logo.Alignment = GetAlignmentForQuote(lang, true);
                logo.ScaleToFit(270f, 270f);

                var cellLogo = new PdfPCell { Border = Rectangle.NO_BORDER };
                cellLogo.AddElement(logo);
                headerTable.AddCell(cellLogo);
            }

            doc.Add(headerTable);
        }

        //protected virtual void PrintQuoteAddresses(int vendorId, Language lang, Font titleFont, Order order, Font font, Document doc, Font fontpre)
        protected virtual async Task PrintQuoteAddresses(int vendorId, Language lang, Font titleFont, Order order, Font font, Document doc, Font fontpre)
        {
            var addressTable = new PdfPTable(3) { RunDirection = GetDirectionForQuote(lang) };
            addressTable.DefaultCell.Border = Rectangle.NO_BORDER;

            addressTable.WidthPercentage = 104f;
            addressTable.SetWidths(new[] { 30, 30, 30 });

            //billing info
            await PrintQouteBillingInfo(vendorId, lang, titleFont, order, font, addressTable, fontpre);

            await PrintInfo(vendorId, lang, titleFont, order, font, addressTable);

            //shipping info
            await PrintQuoteShippingInfo(lang, order, titleFont, font, fontpre, addressTable);

            doc.Add(addressTable);
            doc.Add(new Paragraph(" "));
        }

        //public virtual void PrintQuoteRequestToPdf(Stream stream, IList<Order> orders, IList<UWProductDetailsModel> productDetailsList, int languageId = 0, int vendorId = 0)
        public virtual async Task PrintQuoteRequestToPdf(Stream stream, IList<Order> orders, IList<UWProductDetailsModel> productDetailsList, int languageId = 0, int vendorId = 0)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (orders == null)
                throw new ArgumentNullException(nameof(orders));

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            titleFont.Size = 25f;
            var fontpre = GetFont();
            fontpre.SetStyle(Font.BOLD);
            var firstfont = GetFont();
            firstfont.SetColor(160, 82, 45);
            var font = GetFont();
            var font1 = GetFont();
            font1.Size = 11f;
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            var ordCount = orders.Count;
            var ordNum = 0;

            foreach (var order in orders)
            {
                //by default _pdfSettings contains settings for the current active store
                //and we need PdfSettings for the store which was used to place an order
                //so let's load it based on a store of the current order
                //var pdfSettingsByStore = _settingService.LoadSetting<PdfSettings>(order.StoreId);
                var pdfSettingsByStore =await _settingService.LoadSettingAsync<PdfSettings>(order.StoreId);

                //var lang = _languageService.GetLanguageById(languageId == 0 ? order.CustomerLanguageId : languageId);
                var lang =await _languageService.GetLanguageByIdAsync(languageId == 0 ? order.CustomerLanguageId : languageId);
                //if (lang == null || !lang.Published)
                if (lang == null || !lang.Published)
                    //lang = _workContext.WorkingLanguage;
                    lang =await _workContext.GetWorkingLanguageAsync();

                //header
                await PrintQuoteHeader(pdfSettingsByStore, lang, order, font, titleFont, doc);

                //addresses
                await PrintQuoteAddresses(vendorId, lang, titleFont, order, font, doc, fontpre);
                //products
                await PrintQuoteRequestProducts(vendorId, lang, productDetailsList, titleFont, doc, order, font1, attributesFont, font);

                //checkout attributes
                PrintCheckoutAttributes(vendorId, order, doc, lang, font);

                //totals
                //order notes
                //PrintOrderNotes(pdfSettingsByStore, order, lang, titleFont, doc, font);
                await PrintOrderNotesAsync(pdfSettingsByStore, order, lang, titleFont, doc, font);

                await PrintQuoteRequestTotalsPdf(vendorId, lang, order, font, titleFont, doc, fontpre, firstfont);

                //footer
                PrintFooter(pdfSettingsByStore, pdfWriter, pageSize, lang, font);

                ordNum++;
                if (ordNum < ordCount)
                {
                    doc.NewPage();
                }
            }

            doc.Close();
        }

        //protected virtual void PrintQuoteRequestProducts(int vendorId, Language lang, IList<UWProductDetailsModel> productDetailsList, Font titleFont, Document doc, Order order, Font font1, Font attributesFont, Font font)
        protected virtual async Task PrintQuoteRequestProducts(int vendorId, Language lang, IList<UWProductDetailsModel> productDetailsList, Font titleFont, Document doc, Order order, Font font1, Font attributesFont, Font font)
        {

            var productsHeader = new PdfPTable(1)
            {

                RunDirection = 2,
                WidthPercentage = 100f
            };
            //var cellProducts = GetPdfCell("Please note quotes are only valid for 30 days from the email send data.we are not responsive for piece mistakes and response the right to cance order", lang, font1, true);
            var cellProducts =await GetPdfCellAsync("Please note quotes are only valid for 30 days from the email send data.we are not responsive for piece mistakes and response the right to cance order", lang, font1, true);
            cellProducts.Border = Rectangle.NO_BORDER;

            productsHeader.AddCell(cellProducts);
            doc.Add(productsHeader);
            doc.Add(new Paragraph(" "));
            doc.SetPageSize(PageSize.A4.Rotate());
            //a vendor should have access only to products
            //var orderItems = _orderService.GetOrderItems(order.Id, vendorId: vendorId);
            var orderItems =await _orderService.GetOrderItemsAsync(order.Id, vendorId: vendorId);

            var count = 13 + (_catalogSettings.ShowSkuOnProductDetailsPage ? 1 : 0)
                        + (_vendorSettings.ShowVendorOnOrderDetailsPage ? 1 : 0);

            var productsTable = new PdfPTable(count)
            {
                RunDirection = GetDirectiontable(lang),
                WidthPercentage = 100f
            };



            var widths = new Dictionary<int, int[]>
            {
                { 4, new[] { 50, 20, 10, 20 } },
                { 5, new[] { 45, 15, 15, 10, 15 } },
               { 6, new[] { 40, 13, 13, 12, 10, 12 } },
               { 13, new[] { 40, 5, 5, 5, 5, 5 ,5,5, 5, 5, 5, 5,5 } },
               { 14, new[] { 9, 7, 7, 7, 7, 7 ,7,7, 7, 7, 7, 7,7,7} },
               { 15, new[] { 30, 5, 5, 5, 5, 5 ,5,5, 5, 5, 5, 5,5,5,5} },
            };

            productsTable.SetWidths(lang.Rtl ? widths[count].Reverse().ToArray() : widths[count]);

            //product name or Category 1
            //var cellProductItem = GetPdfCell("PDFInvoice.ProductName", lang, font);
            var cellProductItem =await GetPdfCellAsync("PDFInvoice.ProductName", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //SKU 2
            cellProductItem = await GetPdfCellAsync("PDFInvoice.SKU", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //Vendor name
            if (_vendorSettings.ShowVendorOnOrderDetailsPage)
            {
                cellProductItem = await GetPdfCellAsync("PDFInvoice.VendorName", lang, font);
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
            }
            //Diameter 3
            cellProductItem = await GetPdfCellAsync("Diameter", lang, font, true);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //Type 4
            cellProductItem = await GetPdfCellAsync("Type", lang, font, true);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //Dopant 5
            cellProductItem = await GetPdfCellAsync("Dopant", lang, font, true);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //Orientation 6
            cellProductItem = await GetPdfCellAsync("Orientation", lang, font, true);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //Resistivity 7
            cellProductItem = await GetPdfCellAsync("Resistance", lang, font, true);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);


            //Thickness 8
            cellProductItem = await GetPdfCellAsync("Thickness", lang, font, true);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //Polish 9
            cellProductItem = await GetPdfCellAsync("Polish", lang, font, true);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);


            //Grade 10
            cellProductItem = await GetPdfCellAsync("Grade", lang, font, true);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);


            //LeadTime 11
            cellProductItem = await GetPdfCellAsync("LeadTime", lang, font, true);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //price
            cellProductItem = await GetPdfCellAsync("PDFInvoice.ProductPrice", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //qty
            cellProductItem = await GetPdfCellAsync("PDFInvoice.ProductQuantity", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //total
            cellProductItem = await GetPdfCellAsync("PDFInvoice.ProductTotal", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //var vendors = _vendorSettings.ShowVendorOnOrderDetailsPage ? _vendorService.GetVendorsByProductIds(orderItems.Select(item => item.ProductId).ToArray()) : new List<Vendor>();
            var vendors = _vendorSettings.ShowVendorOnOrderDetailsPage ?await _vendorService.GetVendorsByProductIdsAsync(orderItems.Select(item => item.ProductId).ToArray()) : new List<Vendor>();


            int i = 0;
            foreach (var orderItem in orderItems)
            {

                //var product = _productService.GetProductById(orderItem.ProductId);
                var product =await _productService.GetProductByIdAsync(orderItem.ProductId);
                //var leadTimes = _dateRangeService.GetAllProductAvailabilityRanges();
                var leadTimes =await _dateRangeService.GetAllProductAvailabilityRangesAsync();
                var leadTime = "";
                if (leadTimes.Any(lt => lt.Id.Equals(product.ProductAvailabilityRangeId)))
                {
                    leadTime = leadTimes.First(lt => lt.Id.Equals(product.ProductAvailabilityRangeId)).Name;
                }
                var pAttribTable = new PdfPTable(1) { RunDirection = GetDirectionForQuote(lang) };
                pAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

                //product name
                //var name = _localizationService.GetLocalized(product, x => x.Name, lang.Id);
                var name =await _localizationService.GetLocalizedAsync(product, x => x.Name, lang.Id);
                pAttribTable.AddCell(new Paragraph(name, font));
                cellProductItem.AddElement(new Paragraph(name, font));


                //attributes
                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    var attributesParagraph =
                        new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true),
                            attributesFont);
                    pAttribTable.AddCell(attributesParagraph);
                }

                //rental info
                if (product.IsRental)
                {
                    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalStartDateUtc.Value)
                        : string.Empty;
                    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalEndDateUtc.Value)
                        : string.Empty;
                    //var rentalInfo = string.Format(_localizationService.GetResource("Order.Rental.FormattedDate"),
                    var rentalInfo = string.Format(await _localizationService.GetResourceAsync("Order.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);

                    var rentalInfoParagraph = new Paragraph(rentalInfo, attributesFont);
                    pAttribTable.AddCell(rentalInfoParagraph);
                }

                productsTable.AddCell(pAttribTable);

                //SKU
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                {
                    //var sku = _productService.FormatSku(product, orderItem.AttributesXml);
                    var sku =await _productService.FormatSkuAsync(product, orderItem.AttributesXml);
                    cellProductItem = GetPdfCell(sku ?? string.Empty, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cellProductItem);
                }

                //Vendor name
                if (_vendorSettings.ShowVendorOnOrderDetailsPage)
                {
                    var vendorName = vendors.FirstOrDefault(v => v.Id == product.VendorId)?.Name ?? string.Empty;
                    cellProductItem = GetPdfCell(vendorName, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cellProductItem);
                }
                //diameter
                cellProductItem = GetPdfCell(productDetailsList[i].Diameter ?? string.Empty, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
                //type
                cellProductItem = GetPdfCell(productDetailsList[i].Type ?? string.Empty, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
                //dopant
                cellProductItem = GetPdfCell(productDetailsList[i].Dopant ?? string.Empty, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
                //Orientation
                cellProductItem = GetPdfCell(productDetailsList[i].Orientation ?? string.Empty, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
                //polish
                cellProductItem = GetPdfCell(productDetailsList[i].Polish ?? string.Empty, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
                //grade
                cellProductItem = GetPdfCell(productDetailsList[i].Grade ?? string.Empty, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
                //resistance
                cellProductItem = GetPdfCell((productDetailsList[i].ResistivityMin + "-" + productDetailsList[i].ResistivityMax) ?? string.Empty, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);

                //thickness
                cellProductItem = GetPdfCell(productDetailsList[i].Thickness ?? string.Empty, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);

                //leadTime
                cellProductItem = GetPdfCell(leadTime ?? string.Empty, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);

                // price
                string unitPrice;
                //if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                //{
                //    //including tax
                //    var unitPriceInclTaxInCustomerCurrency =
                //        _currencyService.ConvertCurrency(UnitPriceInclTax, order.CurrencyRate);
                //    unitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true,
                //        order.CustomerCurrencyCode, lang.Id, true);
                //}
                //else
                //{
                //excluding tax

                var unitPriceExclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                //unitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true,
                unitPrice =await _priceFormatter.FormatPriceAsync(unitPriceExclTaxInCustomerCurrency, true,
                    order.CustomerCurrencyCode, lang.Id, false);
                // unitPrice = Convert.ToString(UnitPriceExclTax);
                //  }

                cellProductItem = GetPdfCell(unitPrice, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);

                //qty
                cellProductItem = GetPdfCell(orderItem.Quantity, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);

                //total
                string subTotal;
                //if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                //{
                //    //including tax
                //    var priceInclTaxInCustomerCurrency =
                //        _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                //    subTotal = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                //        lang.Id, true);
                //}
                //else
                //{
                //excluding tax
                var priceExclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                //subTotal = _priceFormatter.FormatPrice(orderItem.PriceExclTax, true, order.CustomerCurrencyCode,
                subTotal =await _priceFormatter.FormatPriceAsync(orderItem.PriceExclTax, true, order.CustomerCurrencyCode,
                     lang.Id, false);


                cellProductItem = GetPdfCell(subTotal, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);

                i++;
            }

            doc.Add(productsTable);
        }

        //protected virtual void PrintQuoteRequestTotalsPdf(int vendorId, Language lang, Order order, Font font, Font titleFont, Document doc, Font fontpre, Font firstfont)
        protected virtual async Task PrintQuoteRequestTotalsPdf(int vendorId, Language lang, Order order, Font font, Font titleFont, Document doc, Font fontpre, Font firstfont)
        {
            //vendors cannot see totals
            if (vendorId != 0)
                return;

            //subtotal
            var totalsTable = new PdfPTable(1)
            {
                RunDirection = GetDirectionForQuote(lang),
                WidthPercentage = 100f
            };
            totalsTable.DefaultCell.Border = Rectangle.NO_BORDER;

            var languageId = lang.Id;

            //order subtotal
            //    //excluding tax

            var orderSubtotalExclTaxInCustomerCurrency =
                _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
            //var orderSubtotalExclTaxStr = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, true,
            var orderSubtotalExclTaxStr =await _priceFormatter.FormatPriceAsync(orderSubtotalExclTaxInCustomerCurrency, true,
                order.CustomerCurrencyCode, languageId, false);

            //var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Sub-Total", languageId)} {orderSubtotalExclTaxStr}", font);
            var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Sub-Total", languageId)} {orderSubtotalExclTaxStr}", font);
            p.HorizontalAlignment = Element.ALIGN_LEFT;
            p.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(p);
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
            {
                //    //excluding tax

                var orderSubTotalDiscountExclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                //var orderSubTotalDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(
                var orderSubTotalDiscountInCustomerCurrencyStr = _priceFormatter.FormatPriceAsync(
                    -orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);

                //p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Discount", languageId)} {orderSubTotalDiscountInCustomerCurrencyStr}", font);
                p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Discount", languageId)} {orderSubTotalDiscountInCustomerCurrencyStr}", font);
                p.HorizontalAlignment = Element.ALIGN_LEFT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //shipping
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                //    //excluding tax
                var orderShippingExclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                //var orderShippingExclTaxStr = _priceFormatter.FormatShippingPrice(
                var orderShippingExclTaxStr =await _priceFormatter.FormatShippingPriceAsync(
                    orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);

                //p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Shipping", languageId)} {orderShippingExclTaxStr}", font);
                p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Shipping", languageId)} {orderShippingExclTaxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_LEFT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
                //}

            }

            //payment fee
            if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
            {
                //    //excluding tax
                var paymentMethodAdditionalFeeExclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                //var paymentMethodAdditionalFeeExclTaxStr = _priceFormatter.FormatPaymentMethodAdditionalFee(
                var paymentMethodAdditionalFeeExclTaxStr =await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(
                    paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);

                //p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", languageId)} {paymentMethodAdditionalFeeExclTaxStr}", font);
                p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.PaymentMethodAdditionalFee", languageId)} {paymentMethodAdditionalFeeExclTaxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_LEFT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //tax
            var taxStr = string.Empty;
            var taxRates = new SortedDictionary<decimal, decimal>();
            bool displayTax;
            var displayTaxRates = true;
            if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
            }
            else
            {
                if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    taxRates = _orderService.ParseTaxRates(order, order.TaxRates);

                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    //taxStr = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                    taxStr = await _priceFormatter.FormatPriceAsync(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                       false, languageId);

                }
            }

            if (displayTax)
            {
                //p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Tax", languageId)} {taxStr}", font);
                p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Tax", languageId)} {taxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_LEFT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            if (displayTaxRates)
            {
                foreach (var item in taxRates)
                {
                    //var taxRate = string.Format(_localizationService.GetResource("PDFInvoice.TaxRate", languageId),
                    var taxRate = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.TaxRate", languageId),
                        _priceFormatter.FormatTaxRate(item.Key));
                    //var taxValue = _priceFormatter.FormatPrice(
                    var taxValue = _priceFormatter.FormatPriceAsync(
                        _currencyService.ConvertCurrency(item.Value, order.CurrencyRate), true, order.CustomerCurrencyCode,
                        false, languageId);

                    p = GetPdfCell($"{taxRate} {taxValue}", font);
                    p.HorizontalAlignment = Element.ALIGN_LEFT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            //discount (applied to order total)
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency =
                _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                //var orderDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency,
                var orderDiscountInCustomerCurrencyStr = _priceFormatter.FormatPriceAsync(-orderDiscountInCustomerCurrency,
                    true, order.CustomerCurrencyCode, false, languageId);

                //p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Discount", languageId)} {orderDiscountInCustomerCurrencyStr}", font);
                p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Discount", languageId)} {orderDiscountInCustomerCurrencyStr}", font);
                p.HorizontalAlignment = Element.ALIGN_LEFT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //gift cards
            //foreach (var gcuh in _giftCardService.GetGiftCardUsageHistory(order))
            foreach (var gcuh in await _giftCardService.GetGiftCardUsageHistoryAsync(order))
            {
                //var gcTitle = string.Format(_localizationService.GetResource("PDFInvoice.GiftCardInfo", languageId),
                var gcTitle = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.GiftCardInfo", languageId),
                     //_giftCardService.GetGiftCardById(gcuh.GiftCardId)?.GiftCardCouponCode);
                    (await _giftCardService.GetGiftCardByIdAsync(gcuh.GiftCardId))?.GiftCardCouponCode);
                //var gcAmountStr = _priceFormatter.FormatPrice(
                var gcAmountStr = _priceFormatter.FormatPriceAsync(
                    -_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate), true,
                    order.CustomerCurrencyCode, false, languageId);

                p = GetPdfCell($"{gcTitle} {gcAmountStr}", font);
                p.HorizontalAlignment = Element.ALIGN_LEFT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //reward points
            //if (order.RedeemedRewardPointsEntryId.HasValue && _rewardPointService.GetRewardPointsHistoryEntryById(order.RedeemedRewardPointsEntryId.Value) is RewardPointsHistory redeemedRewardPointsEntry)
            if (order.RedeemedRewardPointsEntryId.HasValue &&await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(order.RedeemedRewardPointsEntryId.Value) is RewardPointsHistory redeemedRewardPointsEntry)
            {
                //var rpTitle = string.Format(_localizationService.GetResource("PDFInvoice.RewardPoints", languageId),
                var rpTitle = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.RewardPoints", languageId),
                    -redeemedRewardPointsEntry.Points);
                //var rpAmount = _priceFormatter.FormatPrice(
                var rpAmount = _priceFormatter.FormatPriceAsync(
                   -_currencyService.ConvertCurrency(redeemedRewardPointsEntry.UsedAmount, order.CurrencyRate),
                    true, order.CustomerCurrencyCode, false, languageId);

                p = GetPdfCell($"{rpTitle} {rpAmount}", font);
                p.HorizontalAlignment = Element.ALIGN_LEFT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //order total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            //var orderTotalStr = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);
            var orderTotalStr =await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);
            //var orderTotalStr = order.OrderTotal;
            //var pTotal = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.OrderTotal", languageId)} {orderTotalStr}", fontpre/* font *//*firstfont*//* titleFont*/);
            var pTotal = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.OrderTotal", languageId)} {orderTotalStr}", fontpre/* font *//*firstfont*//* titleFont*/);
            pTotal.HorizontalAlignment = Element.ALIGN_LEFT;
            pTotal.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(pTotal);


            // Font font, Document doc, Font firstfont

            //p = GetPdfCell($"{_localizationService.GetResource("", languageId)} ", font);
            p = GetPdfCell($"{await _localizationService.GetResourceAsync("", languageId)} ", font);
            p.HorizontalAlignment = Element.ALIGN_RIGHT;
            p.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(p);


            p =await GetPdfCellAsync("Note: SSP = Single Side Polished, DSP = Double Side Polished, E = Etched, C = AsCut, L = Lapped,", lang, firstfont, true);
            p.HorizontalAlignment = Element.ALIGN_RIGHT;
            p.Border = Rectangle.NO_BORDER;

            totalsTable.AddCell(p);

            p = await GetPdfCellAsync("Material is CZ unless noted,Und=Undoped(intrinstic),", lang, firstfont, true);
            p.HorizontalAlignment = Element.ALIGN_RIGHT;
            p.Border = Rectangle.NO_BORDER;

            totalsTable.AddCell(p);

            p = await GetPdfCellAsync("All Prices USD only.One unit=one wafer,", lang, firstfont, true);
            p.HorizontalAlignment = Element.ALIGN_RIGHT;
            p.Border = Rectangle.NO_BORDER;

            totalsTable.AddCell(p);

            //p = GetPdfCell($"{_localizationService.GetResource("", languageId)} ", font);
            p = GetPdfCell($"{await _localizationService.GetResourceAsync("", languageId)} ", font);
            p.HorizontalAlignment = Element.ALIGN_RIGHT;
            p.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(p);
            p = await GetPdfCellAsync("Return Policy: There will be  15% restrocking fee for all retirns must bbe sent back withine 15 days of recieving the shipment. All duties domestic and international taxes,shipping and customer fees are paid by the buyer and are not refunded. Custom oreders are on a best effor basis with np refunds.Return Shipments is paid for by the buyer unless extenuating circumstances arise.", lang, font, true);
            p.HorizontalAlignment = Element.ALIGN_RIGHT;
            p.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(p);

            //p = GetPdfCell($"{_localizationService.GetResource("", languageId)} ", font);
            p = GetPdfCell($"{await _localizationService.GetResourceAsync("", languageId)} ", font);
            p.HorizontalAlignment = Element.ALIGN_RIGHT;
            p.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(p);

            p = await GetPdfCellAsync("Refund Policy: Please Allow the credit Card processor up to 7 days to credit back your credit card", lang, font, true);
            p.HorizontalAlignment = Element.ALIGN_RIGHT;
            p.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(p);


            p = await GetPdfCellAsync("Cancellation Policy: We Will make every attempts to cancel the order befor it ships, once shipped, the order cannot be canceled", lang, font, true);
            p.HorizontalAlignment = Element.ALIGN_RIGHT;
            p.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(p);

            p =await GetPdfCellAsync("Delivery/Shipping Policy: We do not ship to any countries not allowed by US customs. This includes but is not limited to Syria,North Korea,Sudan.Iran And Cuba.OVERNIGHT/Next Day orders recieved AFTER 3:30pm EASTERN Standard Time will Ship the  Next Business Day.", lang, font, true);
            p.HorizontalAlignment = Element.ALIGN_RIGHT;
            p.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(p);


            doc.Add(totalsTable);
            //}



        }

        #endregion
        #endregion
    }
}