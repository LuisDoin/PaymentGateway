
using ServiceIntegrationLibrary.Utils;
using ServiceIntegrationLibrary.Models;
using System;
using System.Text.RegularExpressions;

namespace ServiceIntegrationLibrary.ModelValidationServices
{
    public class PaymentValidationService : IPaymentValidationService
    {
        public void ValidatePayment(PaymentDetails paymentDetails)
        {
            if (paymentDetails == null) throw new ArgumentException("PurchaseDetails cannot be null.");

            if (paymentDetails.MerchantId == 0) throw new ArgumentException("Invalid merchantId.");

            if (!isValidCreditCard(paymentDetails.CreditCardNumber.Trim())) throw new ArgumentException("Invalid credit card number.");

            if (!isValidExpirationDate(paymentDetails.ExpirationDate.Trim())) throw new ArgumentException("Invalid expiration date.");

            if (!Regex.IsMatch(paymentDetails.Cvv.Trim(), "^[0-9]{3,4}$")) throw new ArgumentException("Invalid cvv");

            if (!Enum.IsDefined(typeof(SupportedCurrencies), paymentDetails.Currency.Trim())) throw new ArgumentException("Invalid or unsupported currency.");

            if (paymentDetails.Amount < 0) throw new ArgumentException("Invalid amount.");
        }

        private bool isValidCreditCard(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return false;

            var isValidLength = cardNumber.Length >= 8 && cardNumber.Length <= 19;
            var sum = sumOfDoubleEvenPlace(cardNumber) + sumOfOddPlace(cardNumber);
            var isValidSum = sum > 0 && sum % 10 == 0;

            return isValidLength && isValidSum;
        }

        // Get the result from Step 2
        private static int sumOfDoubleEvenPlace(string cardNumber)
        {
            int sum = 0;
            for (int i = cardNumber.Length - 2; i >= 0; i -= 2)
                sum += getDigit(int.Parse(cardNumber[i] + "") * 2);

            return sum;
        }

        // Returns this number if it is a single digit, otherwise, returns the sum of the two digits
        private static int getDigit(int num)
        {
            if (num < 9)
                return num;
            return num / 10 + num % 10;
        }

        // Return sum of odd-place digits in number
        private static int sumOfOddPlace(string cardNumber)
        {
            int sum = 0;
            for (int i = cardNumber.Length - 1; i >= 0; i -= 2)
                sum += int.Parse(cardNumber[i] + "");
            return sum;
        }

        private bool isValidExpirationDate(string expirationDate)
        {
            if (string.IsNullOrWhiteSpace(expirationDate))
                return false;

            var dateParts = expirationDate.Split('/');

            if (dateParts.Length != 2)
                return false;

            var month = dateParts[0];
            var year = dateParts[1];

            return Regex.IsMatch(month, @"^(0[1-9]|1[0-2])$") && Regex.IsMatch(year, @"^20[0-9][0-9]$") && !cardIsExpired(int.Parse(month), int.Parse(year));
        }

        private bool cardIsExpired(int expirationMonth, int expirationYear)
        {
            return expirationYear < DateTime.Now.Year || (expirationYear == DateTime.Now.Year && expirationMonth < DateTime.Now.Month);
        }
    }
}
