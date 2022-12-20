﻿using ServiceIntegrationLibrary.Models;

namespace TransactionsApi.Services
{
    public interface IPaymentServices
    {
        public Task ProcessCompletedTransaction(PaymentDetails paymentDetails);
    }
}
