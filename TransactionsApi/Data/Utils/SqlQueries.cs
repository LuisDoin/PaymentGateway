namespace TransactionsApi.Data.Utils
{
    public static class SqlQueries
    {
        public static readonly string GetTransaction = @"SELECT *
                                                         FROM PaymentsDb.dbo.Payments 
                                                         WHERE paymentId = @paymentId";

        public static readonly string GetTransactionsFromMerchant = @"SELECT *
                                                                      FROM PaymentsDb.dbo.Payments 
                                                                      WHERE merchantId = @merchantId 
                                                                        AND processedAt >= @from
                                                                        AND processedAt <= @to";

        public static readonly string PostTransaction = @"INSERT INTO PaymentsDb.dbo.Payments
                                                          VALUES (@PaymentId, @MerchantId, @CreditCardNumber, @ExpirationDate, 
                                                                  @Cvv, @Currency, @Amount, @ProcessedAt, @Status);";

        
    }
}
