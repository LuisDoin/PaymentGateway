using NUnit.Framework;
using PaymentGateway.Models;
using PaymentGateway.Services;

namespace PaymentGateway.UnitTests.Services
{
    [TestFixture]
    class PaymentServiceTests
    {
        private PaymentService _paymentService;

        [SetUp]
        public void SetUp()
        {
            _paymentService = new PaymentService();
        }

        [Test]
        //Invalid email only
        [TestCase("invalidEmail", "4324781866717289", "01/2030", "000", "USD", 1, "Invalid email")] //Missing @
        [TestCase("invalidEmail@", "4324781866717289", "01/2030", "000", "USD", 1, "Invalid email")] //Missing domain
        [TestCase("@invalidEmail", "4324781866717289", "01/2030", "000", "USD", 1, "Invalid email")] //Missing name
        [TestCase("invalid@Email", "4324781866717289", "01/2030", "000", "USD", 1, "Invalid email")] //Missing top-level domain 
        //Invalid credit card number only
        [TestCase("valid@email.com", "0", "01/2030", "000", "USD", 1, "Invalid credit card number")] //Passing the sum check but not passing the minLenth check
        [TestCase("valid@email.com", "0000000", "01/2030", "000", "USD", 1, "Invalid credit card number")] //Passing the sum check but not passing the minLenth check
        [TestCase("valid@email.com", "00000000000000000000", "01/2030", "000", "USD", 1, "Invalid credit card number")] //Passing the sum check but not passing the maxLenth check
        [TestCase("valid@email.com", "0000000000000000000000000", "01/2030", "000", "USD", 1, "Invalid credit card number")] //Passing the sum check but not passing the maxLenth check
        [TestCase("valid@email.com", "111111111111", "01/2030", "000", "USD", 1, "Invalid credit card number")] //Passing the length check but not passing sum check
        //Invalid expiration date only
        [TestCase("valid@email.com", "4324781866717289", "00/2030", "000", "USD", 1, "Invalid expiration date")] //Incorrect month
        [TestCase("valid@email.com", "4324781866717289", "13/2030", "000", "USD", 1, "Invalid expiration date")] //Incorrect month
        [TestCase("valid@email.com", "4324781866717289", "1/2030", "000", "USD", 1, "Invalid expiration date")] //Incorrect month
        [TestCase("valid@email.com", "4324781866717289", "111/2030", "000", "USD", 1, "Invalid expiration date")] //Incorrect month
        [TestCase("valid@email.com", "4324781866717289", "01/2000", "000", "USD", 1, "Invalid expiration date")] //Incorrect year
        [TestCase("valid@email.com", "4324781866717289", "01/3000", "000", "USD", 1, "Invalid expiration date")] //Incorrect year
        [TestCase("valid@email.com", "4324781866717289", "01/30", "000", "USD", 1, "Invalid expiration date")] //Incorrect year
        [TestCase("valid@email.com", "4324781866717289", "01/20301", "000", "USD", 1, "Invalid expiration date")] //Incorrect year
        [TestCase("valid@email.com", "4324781866717289", "01*2030", "000", "USD", 1, "Invalid expiration date")] //Incorrect separator
        [TestCase("valid@email.com", "4324781866717289", "012030", "000", "USD", 1, "Invalid expiration date")] //Incorrect separator
        //Invalid cvv only
        [TestCase("valid@email.com", "4324781866717289", "01/2030", "0", "USD", 1, "Invalid cvv")] //Not enough digits
        [TestCase("valid@email.com", "4324781866717289", "01/2030", "00", "USD", 1, "Invalid cvv")] //Not enough digits
        [TestCase("valid@email.com", "4324781866717289", "01/2030", "00000", "USD", 1, "Invalid cvv")] //Exceeding maximum digits allowed
        [TestCase("valid@email.com", "4324781866717289", "01/2030", "000000", "USD", 1, "Invalid cvv")] //Exceeding maximum digits allowed
        //Invalid currency only
        [TestCase("valid@email.com", "4324781866717289", "01/2030", "000", "AAA", 1, "Invalid or unsupported currency")] //Inexistent currency
        [TestCase("valid@email.com", "4324781866717289", "01/2030", "000", "BRL", 1, "Invalid or unsupported currency")] //Unsupported currency
        //Invalid amount only
        [TestCase("valid@email.com", "4324781866717289", "01/2030", "000", "USD", -1, "Invalid amount")] //Negative amount
        public void ValidatePayment_InvalidParameters_ThrowArgumentException(string email, string creditCardNumber, string expirationDate, string cvv, string currency, decimal amount, string expectedErrorMessage)
        {
            Assert.That(() => _paymentService.validatePayment(new PaymentDetails(email, creditCardNumber, expirationDate, cvv, currency, amount)),
                Throws.TypeOf<ArgumentException>()
                    .With.Message.Contains(expectedErrorMessage));
        }
    }
}
