﻿using Model.Utils;
using Newtonsoft.Json;
using System;

namespace Model
{
    public class PaymentDetails
    {
        public PaymentDetails(string creditCardNumber, string expirationDate, string cvv, string currency, decimal amount)
        {
            CreditCardNumber = creditCardNumber;
            ExpirationDate = expirationDate;
            Cvv = cvv;
            Currency = currency;
            Amount = amount;
        }

        public Guid PaymentId { get; set; }
        public string CreditCardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string Cvv { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
