using FluentMigrator;

namespace TransactionsApi.Migrations
{
    [Migration(202212220001)]
    public class InitialTables_202212220001 : Migration
    {
        public override void Down()
        {
            Delete.Table("Payments");
        }
        public override void Up()
        {
            Create.Table("Payments")
                .WithColumn("paymentId").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("merchantId").AsInt64().NotNullable()
                .WithColumn("creditCardNumber").AsString(40).NotNullable()
                .WithColumn("expirationDate").AsString(10).NotNullable()
                .WithColumn("cvv").AsString(10).NotNullable()
                .WithColumn("currency").AsString(10).NotNullable()
                .WithColumn("amount").AsDecimal().NotNullable()
                .WithColumn("createdAt").AsDateTime().NotNullable()
                .WithColumn("status").AsString(20).NotNullable();
        }
    }
}
