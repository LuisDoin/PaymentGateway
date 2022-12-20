using NUnit.Framework;
using ServiceIntegrationLibrary.Models;
using ServiceIntegrationLibrary.ModelValidationServices;

namespace ServicesIntegrationLibrary.UnitTests.ValidationServices
{
    [TestFixture]
    class PaymentValidationServicesTests
    {
        private PaymentValidationService _paymentValidationService;

        [SetUp]
        public void SetUp()
        {
            _paymentValidationService = new PaymentValidationService();
        }

        [Test]
        //Invalid credit card number only
        [TestCase("", 1, "01/2030", "000", "USD", 1, "Invalid credit card number")] //Empty credit card number
        [TestCase("0", 1, "01/2030", "000", "USD", 1, "Invalid credit card number")] //Passing the sum check but not passing the minLenth check
        [TestCase("0000000", 1, "01/2030", "000", "USD", 1, "Invalid credit card number")] //Passing the sum check but not passing the minLenth check
        [TestCase("00000000000000000000", 1, "01/2030", "000", "USD", 1, "Invalid credit card number")] //Passing the sum check but not passing the maxLenth check
        [TestCase("0000000000000000000000000", 1, "01/2030", "000", "USD", 1, "Invalid credit card number")] //Passing the sum check but not passing the maxLenth check
        [TestCase("111111111111", 1, "01/2030", "000", "USD", 1, "Invalid credit card number")] //Passing the length check but not passing sum check
        //Invalid merchant name only
        [TestCase("4324781866717289", 0, "01/2030", "000", "USD", 1, "Invalid merchantId")] //Invalid merchantId
        //Invalid expiration date only
        [TestCase("4324781866717289", 1, "", "000", "USD", 1, "Invalid expiration date")] //Empty expiration date
        [TestCase("4324781866717289", 1, "00/2030", "000", "USD", 1, "Invalid expiration date")] //Incorrect month
        [TestCase("4324781866717289", 1, "13/2030", "000", "USD", 1, "Invalid expiration date")] //Incorrect month
        [TestCase("4324781866717289", 1, "1/2030", "000", "USD", 1, "Invalid expiration date")] //Incorrect month
        [TestCase("4324781866717289", 1, "111/2030", "000", "USD", 1, "Invalid expiration date")] //Incorrect month
        [TestCase("4324781866717289", 1, "01/2000", "000", "USD", 1, "Invalid expiration date")] //Incorrect year
        [TestCase("4324781866717289", 1, "01/3000", "000", "USD", 1, "Invalid expiration date")] //Incorrect year
        [TestCase("4324781866717289", 1, "01/30", "000", "USD", 1, "Invalid expiration date")] //Incorrect year
        [TestCase("4324781866717289", 1, "01/20301", " 000", "USD", 1, "Invalid expiration date")] //Incorrect year
        [TestCase("4324781866717289", 1, "01*2030", "000", "USD", 1, "Invalid expiration date")] //Incorrect separator
        [TestCase("4324781866717289", 1, "012030", "000", "USD", 1, "Invalid expiration date")] //Incorrect separator
        //Invalid cvv only  
        [TestCase("4324781866717289", 1, "01/2030", "", "USD", 1, "Invalid cvv")] //Empty cvv
        [TestCase("4324781866717289", 1, "01/2030", "0", "USD", 1, "Invalid cvv")] //Not enough digits
        [TestCase("4324781866717289", 1, "01/2030", "00", "USD", 1, "Invalid cvv")] //Not enough digits
        [TestCase("4324781866717289", 1, "01/2030", "00000", "USD", 1, "Invalid cvv")] //Exceeding maximum digits allowed
        [TestCase("4324781866717289", 1, "01/2030", "000000", "USD", 1, "Invalid cvv")] //Exceeding maximum digits allowed
        //Invalid currency only
        [TestCase("4324781866717289", 1, "01/2030", "000", "AAA", 1, "Invalid or unsupported currency")] //Empty currency
        [TestCase("4324781866717289", 1, "01/2030", "000", "AAA", 1, "Invalid or unsupported currency")] //Inexistent currency
        [TestCase("4324781866717289", 1, "01/2030", "000", "BRL", 1, "Invalid or unsupported currency")] //Unsupported currency
        //Invalid amount only
        [TestCase("4324781866717289", 1, "01/2030", "000", "USD", -1, "Invalid amount")] //Negative amount
        public void ValidatePayment_InvalidParameters_ThrowsArgumentException(string creditCardNumber, long merchantId, string expirationDate, string cvv, string currency, decimal amount, string expectedErrorMessage)
        {
            Assert.That(() => _paymentValidationService.ValidatePayment(new PaymentDetails(creditCardNumber, merchantId, expirationDate, cvv, currency, amount)),
                Throws.TypeOf<ArgumentException>()
                    .With.Message.Contains(expectedErrorMessage));
        }

        [TestCase("4324781866717289", 1, "01/2030", "000", "USD", 1)]
        public void ValidatePayment_ValidParameters_DoesNotThrowException(string creditCardNumber, long merchantId, string expirationDate, string cvv, string currency, decimal amount)
        {
            Assert.DoesNotThrow(() => _paymentValidationService.ValidatePayment(new PaymentDetails(creditCardNumber, merchantId, expirationDate, cvv, currency, amount)));
        }
    }
}
