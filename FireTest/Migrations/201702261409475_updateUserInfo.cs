namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateUserInfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Age", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "Sex", c => c.Boolean(nullable: false));
            AddColumn("dbo.Users", "Region", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Region");
            DropColumn("dbo.Users", "Sex");
            DropColumn("dbo.Users", "Age");
        }
    }
}
