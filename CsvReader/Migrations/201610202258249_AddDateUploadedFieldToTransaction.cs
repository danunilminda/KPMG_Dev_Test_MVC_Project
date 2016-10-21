namespace CsvReader.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddDateUploadedFieldToTransaction : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Transactions", "DateUploaded", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Transactions", "DateUploaded");
        }
    }
}
