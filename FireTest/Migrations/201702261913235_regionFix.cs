namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class regionFix : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "Region", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "Region", c => c.Int(nullable: false));
        }
    }
}
