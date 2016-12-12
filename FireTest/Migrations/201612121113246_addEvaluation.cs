namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addEvaluation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Examinations", "Eval5", c => c.Int(nullable: false));
            AddColumn("dbo.Examinations", "Eval4", c => c.Int(nullable: false));
            AddColumn("dbo.Examinations", "Eval3", c => c.Int(nullable: false));
            AddColumn("dbo.Examinations", "Time", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Examinations", "Time");
            DropColumn("dbo.Examinations", "Eval3");
            DropColumn("dbo.Examinations", "Eval4");
            DropColumn("dbo.Examinations", "Eval5");
        }
    }
}
