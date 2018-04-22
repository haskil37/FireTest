namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addMaster : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Faculties", "Bachelor", c => c.Int(nullable: false));
            AddColumn("dbo.Faculties", "Master", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "Master", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Master");
            DropColumn("dbo.Faculties", "Master");
            DropColumn("dbo.Faculties", "Bachelor");
        }
    }
}
