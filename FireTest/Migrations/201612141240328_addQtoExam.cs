namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addQtoExam : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TestQualifications", "Questions", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TestQualifications", "Questions");
        }
    }
}
