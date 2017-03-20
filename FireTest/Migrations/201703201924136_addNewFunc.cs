namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addNewFunc : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Faculty", c => c.String());
            AddColumn("dbo.Users", "QualificationPoint", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "QualificationPoint");
            DropColumn("dbo.Users", "Faculty");
        }
    }
}
