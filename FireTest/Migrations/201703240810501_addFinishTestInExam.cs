namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addFinishTestInExam : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Examinations", "FinishTest", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Examinations", "FinishTest");
        }
    }
}
