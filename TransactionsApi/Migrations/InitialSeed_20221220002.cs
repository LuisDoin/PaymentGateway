using FluentMigrator;
using ServiceIntegrationLibrary.Models;
using ServiceIntegrationLibrary.Utils;

namespace TransactionsApi.Migrations
{
    [Migration(202212220002)]
    public class InitialSeed_20221220002 : Migration
    {
        public override void Down()
        {
            Delete.FromTable("Payments")
                .Row(new PaymentDetails
                {
                    PaymentId = new Guid("b44340d8-9ab8-4714-8d86-b2a89daad9eb"),
                    MerchantId = 1,
                    CreditCardNumber = "XXXX XXXX XXXX 8737",
                    ExpirationDate = "05/2027",
                    Cvv = "***",
                    Currency = "EUR",
                    Amount = 60,
                    CreatedAt = new DateTime(2022, 12, 21, 17, 32, 57),
                    Status = PaymentStatus.Successful
                });
            Delete.FromTable("Payments")
                .Row(new PaymentDetails
                {
                    PaymentId = new Guid("7428a96a-83ef-4f12-b1d8-647897c966c2"),
                    MerchantId = 1,
                    CreditCardNumber = "XXXX XXXX XXXX 8967",
                    ExpirationDate = "03/2024",
                    Cvv = "***",
                    Currency = "CAD",
                    Amount = 87,
                    CreatedAt = new DateTime(2022, 12, 19, 19, 47, 21),
                    Status = PaymentStatus.Successful
                });
            Delete.FromTable("Payments")
                .Row(new PaymentDetails
                {
                    PaymentId = new Guid("8a61e27c-07d3-48b6-b348-6b6b9f909aa2"),
                    MerchantId = 1,
                    CreditCardNumber = "XXXX XXXX XXXX 8732",
                    ExpirationDate = "02/2027",
                    Cvv = "***",
                    Currency = "USD",
                    Amount = 92,
                    CreatedAt = new DateTime(2022, 12, 21, 03, 24, 23),
                    Status = PaymentStatus.Unsuccessful
                });
            Delete.FromTable("Payments")
                .Row(new PaymentDetails
                {
                    PaymentId = new Guid("3077380a-09f0-4c2f-bdce-d858a705c429"),
                    MerchantId = 1,
                    CreditCardNumber = "XXXX XXXX XXXX 4587",
                    ExpirationDate = "04/2029",
                    Cvv = "***",
                    Currency = "USD",
                    Amount = 27,
                    CreatedAt = new DateTime(2022, 12, 20, 22, 34, 12),
                    Status = PaymentStatus.Successful
                });
            Delete.FromTable("Payments")
                .Row(new PaymentDetails
                {
                    PaymentId = new Guid("387725dc-2cf4-48d7-99a5-87ddf01bcffd"),
                    MerchantId = 1,
                    CreditCardNumber = "XXXX XXXX XXXX 8639",
                    ExpirationDate = "07/2029",
                    Cvv = "***",
                    Currency = "JPY",
                    Amount = 23,
                    CreatedAt = new DateTime(2022, 12, 20, 05, 38, 52),
                    Status = PaymentStatus.Successful
                });
            Delete.FromTable("Payments")
                .Row(new PaymentDetails
                {
                    PaymentId = new Guid("bb3a81d9-18da-4df4-8141-b4d30e9511be"),
                    MerchantId = 1,
                    CreditCardNumber = "XXXX XXXX XXXX 4378",
                    ExpirationDate = "06/2028",
                    Cvv = "***",
                    Currency = "GBP",
                    Amount = 80,
                    CreatedAt = new DateTime(2022, 12, 19, 05, 25, 17),
                    Status = PaymentStatus.Successful
                });
            Delete.FromTable("Payments")
                .Row(new PaymentDetails
                {
                    PaymentId = new Guid("ffcf900d-11b5-44be-817e-7f5a6438e41c"),
                    MerchantId = 1,
                    CreditCardNumber = "XXXX XXXX XXXX 2176",
                    ExpirationDate = "05/2025",
                    Cvv = "***",
                    Currency = "USD",
                    Amount = 20,
                    CreatedAt = new DateTime(2022, 12, 22, 13, 56, 02),
                    Status = PaymentStatus.Unsuccessful
                });
            Delete.FromTable("Payments")
                .Row(new PaymentDetails
                {
                    PaymentId = new Guid("67fbac34-1ee1-4697-b916-1748861dd275"),
                    MerchantId = 1,
                    CreditCardNumber = "XXXX XXXX XXXX 7289",
                    ExpirationDate = "02/2027",
                    Cvv = "***",
                    Currency = "USD",
                    Amount = 60,
                    CreatedAt = new DateTime(2022, 12, 21, 18, 14, 30),
                    Status = PaymentStatus.Successful
                });
        }
        public override void Up()
        {
            Insert.IntoTable("Payments")
                .Row(new PaymentDetails
                {
                    PaymentId = new Guid("67fbac34-1ee1-4697-b916-1748861dd275"),
                    MerchantId = 1,
                    CreditCardNumber = "XXXX XXXX XXXX 7289",
                    ExpirationDate = "02/2027",
                    Cvv = "***",
                    Currency = "USD",
                    Amount = 60,
                    CreatedAt = new DateTime(2022, 12, 21, 18, 14, 30),
                    Status = PaymentStatus.Successful
                }); 
            Insert.IntoTable("Payments")
                .Row(new PaymentDetails
                {
                    PaymentId = new Guid("ffcf900d-11b5-44be-817e-7f5a6438e41c"),
                    MerchantId = 1,
                    CreditCardNumber = "XXXX XXXX XXXX 2176",
                    ExpirationDate = "05/2025",
                    Cvv = "***",
                    Currency = "USD",
                    Amount = 20,
                    CreatedAt = new DateTime(2022, 12, 22, 13, 56, 12),
                    Status = PaymentStatus.Unsuccessful
                });
            Insert.IntoTable("Payments")
                .Row(new PaymentDetails
                {
                    PaymentId = new Guid("bb3a81d9-18da-4df4-8141-b4d30e9511be"),
                    MerchantId = 1,
                    CreditCardNumber = "XXXX XXXX XXXX 4378",
                    ExpirationDate = "06/2028",
                    Cvv = "***",
                    Currency = "GBP",
                    Amount = 80,
                    CreatedAt = new DateTime(2022, 12, 19, 13, 25, 17),
                    Status = PaymentStatus.Successful
                });

            Insert.IntoTable("Payments")
                .Row(new PaymentDetails
                {
                    PaymentId = new Guid("387725dc-2cf4-48d7-99a5-87ddf01bcffd"),
                    MerchantId = 1,
                    CreditCardNumber = "XXXX XXXX XXXX 8639",
                    ExpirationDate = "07/2029",
                    Cvv = "***",
                    Currency = "JPY",
                    Amount = 23,
                    CreatedAt = new DateTime(2022, 12, 20, 05, 38, 52),
                    Status = PaymentStatus.Successful
                });

            Insert.IntoTable("Payments")
                .Row(new PaymentDetails
                {
                    PaymentId = new Guid("3077380a-09f0-4c2f-bdce-d858a705c429"),
                    MerchantId = 1,
                    CreditCardNumber = "XXXX XXXX XXXX 4587",
                    ExpirationDate = "04/2029",
                    Cvv = "***",
                    Currency = "USD",
                    Amount = 27,
                    CreatedAt = new DateTime(2022, 12, 20, 22, 34, 12),
                    Status = PaymentStatus.Successful
                });

            Insert.IntoTable("Payments")
                .Row(new PaymentDetails
                {
                    PaymentId = new Guid("8a61e27c-07d3-48b6-b348-6b6b9f909aa2"),
                    MerchantId = 1,
                    CreditCardNumber = "XXXX XXXX XXXX 8732",
                    ExpirationDate = "02/2027",
                    Cvv = "***",
                    Currency = "USD",
                    Amount = 92,
                    CreatedAt = new DateTime(2022, 12, 21, 03, 24, 23),
                    Status = PaymentStatus.Unsuccessful
                });

            Insert.IntoTable("Payments")
                .Row(new PaymentDetails
                {
                    PaymentId = new Guid("7428a96a-83ef-4f12-b1d8-647897c966c2"),
                    MerchantId = 1,
                    CreditCardNumber = "XXXX XXXX XXXX 8967",
                    ExpirationDate = "03/2024",
                    Cvv = "***",
                    Currency = "CAD",
                    Amount = 87,
                    CreatedAt = new DateTime(2022, 12, 19, 19, 47, 21),
                    Status = PaymentStatus.Successful
                });

            Insert.IntoTable("Payments")
                .Row(new PaymentDetails
                {
                    PaymentId = new Guid("b44340d8-9ab8-4714-8d86-b2a89daad9eb"),
                    MerchantId = 1,
                    CreditCardNumber = "XXXX XXXX XXXX 8737",
                    ExpirationDate = "05/2027",
                    Cvv = "***",
                    Currency = "EUR",
                    Amount = 60,
                    CreatedAt = new DateTime(2022, 12, 21, 17, 32, 57),
                    Status = PaymentStatus.Successful
                });
        }
    }
}
