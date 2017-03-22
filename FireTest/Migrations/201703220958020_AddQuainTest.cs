namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddQuainTest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TeacherFinishTests", "IdQualification", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TeacherFinishTests", "IdQualification");
        }
    }
}
