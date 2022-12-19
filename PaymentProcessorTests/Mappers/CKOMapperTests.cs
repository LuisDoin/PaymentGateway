using AutoFixture;
using MassTransit;
using Model;
using Model.ModelValidationServices;
using NUnit.Framework;
using PaymentProcessor.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentProcessorTests.Mappers
{
    [TestFixture]
    class CKOMapperTests
    {
        private Fixture _fixture;
        private CKOMapper _ckoMapper;
        private PaymentDetails _paymentDetails;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _ckoMapper = new CKOMapper();
            _paymentDetails = new PaymentDetails(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<decimal>());
        }

        [Test]
        public void ToDto_WhenSuccessful_ReturnExpectedDTO()
        {
            var ckoPaymentInfoDTO = _ckoMapper.ToDto(_paymentDetails);

            Assert.That(ckoPaymentInfoDTO.PaymentId, Is.EqualTo(_paymentDetails.PaymentId));
            Assert.That(ckoPaymentInfoDTO.CreditCardNumber, Is.EqualTo(_paymentDetails.CreditCardNumber));
            Assert.That(ckoPaymentInfoDTO.ExpirationDate, Is.EqualTo(_paymentDetails.ExpirationDate));
            Assert.That(ckoPaymentInfoDTO.Cvv, Is.EqualTo(_paymentDetails.Cvv));
            Assert.That(ckoPaymentInfoDTO.Currency, Is.EqualTo(_paymentDetails.Currency));
            Assert.That(ckoPaymentInfoDTO.Amount, Is.EqualTo(_paymentDetails.Amount));
        }
    }
}
