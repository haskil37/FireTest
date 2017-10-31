namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTimeEnd : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TestQualifications", "TimeEnd", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TestQualifications", "TimeEnd");
        }
    }
}
