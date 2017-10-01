namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addCountTestQuestions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Questions", "CountTest", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Questions", "CountTest");
        }
    }
}
