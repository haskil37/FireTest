namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addEvaluation2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TeacherTests", "Eval5", c => c.Int(nullable: false));
            AddColumn("dbo.TeacherTests", "Eval4", c => c.Int(nullable: false));
            AddColumn("dbo.TeacherTests", "Eval3", c => c.Int(nullable: false));
            DropColumn("dbo.Examinations", "Eval5");
            DropColumn("dbo.Examinations", "Eval4");
            DropColumn("dbo.Examinations", "Eval3");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Examinations", "Eval3", c => c.Int(nullable: false));
            AddColumn("dbo.Examinations", "Eval4", c => c.Int(nullable: false));
            AddColumn("dbo.Examinations", "Eval5", c => c.Int(nullable: false));
            DropColumn("dbo.TeacherTests", "Eval3");
            DropColumn("dbo.TeacherTests", "Eval4");
            DropColumn("dbo.TeacherTests", "Eval5");
        }
    }
}
