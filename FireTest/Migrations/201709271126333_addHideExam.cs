namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addHideExam : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Examinations", "Hide", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Examinations", "Hide");
        }
    }
}
