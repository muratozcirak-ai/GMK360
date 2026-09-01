using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GMK360.Web.Services.Integration
{
    public class IyzicoPaymentService
    {
        private readonly Options _options;
        private readonly ILogger<IyzicoPaymentService> _logger;

        public IyzicoPaymentService(IConfiguration configuration, ILogger<IyzicoPaymentService> logger)
        {
            _logger = logger;
            _options = new Options
            {
                ApiKey = configuration["Iyzico:ApiKey"] ?? "sandbox-api-key",
                SecretKey = configuration["Iyzico:SecretKey"] ?? "sandbox-secret-key",
                BaseUrl = configuration["Iyzico:BaseUrl"] ?? "https://sandbox-api.iyzipay.com"
            };
        }

        /// <summary>
        /// İyzico Pazaryeri (Sub-Merchant) Kredi Kartı Ödemesi ve Komisyon Dağıtımı
        /// </summary>
        public async Task<bool> ProcessSplitPaymentAsync(decimal totalAmount, string subMerchantKey, string cardHolderName, string cardNumber, string expireMonth, string expireYear, string cvc, decimal platformCommissionRate = 0.10m)
        {
            try
            {
                // Komisyon hesabı
                decimal platformEarning = totalAmount * platformCommissionRate;
                decimal subMerchantEarning = totalAmount - platformEarning;

                // 1. Ödeme İsteği Hazırlama
                CreatePaymentRequest request = new CreatePaymentRequest
                {
                    Locale = Locale.TR.ToString(),
                    ConversationId = Guid.NewGuid().ToString(),
                    Price = totalAmount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                    PaidPrice = totalAmount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                    Currency = Currency.TRY.ToString(),
                    Installment = 1,
                    BasketId = "B67832",
                    PaymentChannel = PaymentChannel.WEB.ToString(),
                    PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                };

                // 2. Kredi Kartı Bilgileri
                PaymentCard paymentCard = new PaymentCard
                {
                    CardHolderName = cardHolderName,
                    CardNumber = cardNumber,
                    ExpireMonth = expireMonth,
                    ExpireYear = expireYear,
                    Cvc = cvc,
                    RegisterCard = 0
                };
                request.PaymentCard = paymentCard;

                // 3. Alıcı (Müşteri) Bilgileri
                Buyer buyer = new Buyer
                {
                    Id = "BY789",
                    Name = "John",
                    Surname = "Doe",
                    GsmNumber = "+905350000000",
                    Email = "email@email.com",
                    IdentityNumber = "74300864791",
                    LastLoginDate = "2015-10-05 12:43:35",
                    RegistrationDate = "2013-04-21 15:12:09",
                    RegistrationAddress = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1",
                    Ip = "85.34.78.112",
                    City = "Istanbul",
                    Country = "Turkey",
                    ZipCode = "34732"
                };
                request.Buyer = buyer;

                // 4. Kargo ve Fatura Adresi (Zorunlu)
                Address shippingAddress = new Address
                {
                    ContactName = "Jane Doe",
                    City = "Istanbul",
                    Country = "Turkey",
                    Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1",
                    ZipCode = "34742"
                };
                request.ShippingAddress = shippingAddress;
                request.BillingAddress = shippingAddress;

                // 5. Sepet Kalemleri (Pazaryeri Dağıtımı Buradan Yapılır)
                List<BasketItem> basketItems = new List<BasketItem>();
                
                BasketItem firstBasketItem = new BasketItem
                {
                    Id = "BI101",
                    Name = "Emlak Hizmet Bedeli",
                    Category1 = "Real Estate",
                    ItemType = BasketItemType.VIRTUAL.ToString(),
                    Price = totalAmount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                    SubMerchantKey = subMerchantKey, // Hangi emlakçı/uzman alacak
                    SubMerchantPrice = subMerchantEarning.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) // Uzmana gidecek net tutar
                };
                basketItems.Add(firstBasketItem);

                request.BasketItems = basketItems;

                // 6. Ödemeyi Gerçekleştir
                _logger.LogInformation($"[IYZICO] Pazaryeri ödemesi başlatılıyor. Toplam: {totalAmount}, Komisyon: {platformEarning}, SubMerchant: {subMerchantKey}");
                
                // Iyzico SDK'sı senkron çalışır, bu yüzden Task.Run içine alıyoruz.
                Payment payment = await Task.Run(() => Payment.Create(request, _options));

                if (payment.Status == "success")
                {
                    _logger.LogInformation($"[IYZICO] Ödeme Başarılı! İşlem ID: {payment.PaymentId}");
                    return true;
                }
                else
                {
                    _logger.LogError($"[IYZICO] Ödeme Hatası: {payment.ErrorMessage} (Kodu: {payment.ErrorCode})");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[IYZICO] Beklenmeyen ödeme hatası.");
                return false;
            }
        }
    }
}
