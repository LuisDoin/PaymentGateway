using PaymentGateway.Models;
using PaymentGateway.Services.Interfaces;
using PaymentGateway.Utils;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PaymentGateway.Services
{
    public class PaymentService : IPaymentService
    {
        public void validatePayment(PaymentDetails paymentDetails)
        {
            if (paymentDetails == null) throw new ArgumentException("PurchaseDetails cannot be null.");

            if (!IsValidEmail(paymentDetails.Email.Trim())) throw new ArgumentException("Invalid email adress.");

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

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (Exception ex) when (ex is ArgumentException || ex is RegexMatchTimeoutException)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
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

            return Regex.IsMatch(month, @"^(0[1-9]|1[0-2])$") && Regex.IsMatch(year, @"^20[2-9][0-9]$"); 
        }
    }
}
