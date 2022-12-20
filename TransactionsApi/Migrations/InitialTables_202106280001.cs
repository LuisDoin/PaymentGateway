using FluentMigrator;

namespace TransactionsApi.Migrations
{
    [Migration(202106280001)]
    public class InitialTables_202106280001 : Migration
    {
        public override void Down()
        {
            Delete.Table("Transactions");
        }
        public override void Up()
        {
            Create.Table("Transactions")
                .WithColumn("PaymentId").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("MerchantId").AsInt64().NotNullable()
                .WithColumn("CreditCard").AsString(40).NotNullable()
                .WithColumn("ExpirationDate").AsString(10).NotNullable()
                .WithColumn("Cvv").AsString(10).NotNullable()
                .WithColumn("Currency").AsString(10).NotNullable()
                .WithColumn("Amount").AsDecimal().NotNullable()
                .WithColumn("CreationDate").AsDate().NotNullable()
                .WithColumn("Status").AsString(20).NotNullable();
        }
    }
}
