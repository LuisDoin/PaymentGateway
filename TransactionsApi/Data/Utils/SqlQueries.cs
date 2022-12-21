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
                                                                        AND createdAt >= @from
                                                                        AND createdAt <= @to
                                                                        ORDER BY createdAt DESC";

        public static readonly string PostTransaction = @"INSERT INTO PaymentsDb.dbo.Payments
                                                          VALUES (@PaymentId, @MerchantId, @CreditCardNumber, @ExpirationDate, @Cvv, @Currency, @Amount, @CreatedAt, @Status);";

        public static readonly string UpdateIfExistsElseInsert = @"IF EXISTS (SELECT 1 FROM PaymentsDb.dbo.Payments WHERE paymentId = @paymentId)
                                                                      BEGIN
                                                                         UPDATE PaymentsDb.dbo.Payments 
                                                                         SET status = @Status, createdAt = CreatedAt
                                                                         WHERE paymentId = @paymentId;
                                                                      END
                                                                   ELSE
                                                                      BEGIN
                                                                         INSERT INTO PaymentsDb.dbo.Payments
                                                                         VALUES (@PaymentId, @MerchantId, @CreditCardNumber, @ExpirationDate, @Cvv, @Currency, @Amount, @CreatedAt, @Status);
                                                                      END";

        
    }
}
