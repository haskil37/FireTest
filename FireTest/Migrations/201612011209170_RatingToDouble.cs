namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RatingToDouble : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "Rating", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "Rating", c => c.Int(nullable: false));
        }
    }
}
