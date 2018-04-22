namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBoolQualification : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Questions", "Qualification", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Questions", "Qualification");
        }
    }
}
