namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addFinishQuaTest : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FinishTestQualifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdExamination = c.Int(nullable: false),
                        IdUser = c.String(),
                        Questions = c.String(),
                        Answers = c.String(),
                        RightOrWrong = c.String(),
                        TimeStart = c.DateTime(nullable: false),
                        Score = c.Int(nullable: false),
                        End = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.FinishTestQualificationAccesses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdExamination = c.Int(nullable: false),
                        IdUsers = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TeacherFinishTests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TeacherId = c.String(),
                        NameTest = c.String(),
                        Questions = c.String(),
                        Eval5 = c.Int(nullable: false),
                        Eval4 = c.Int(nullable: false),
                        Eval3 = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TeacherFinishTests");
            DropTable("dbo.FinishTestQualificationAccesses");
            DropTable("dbo.FinishTestQualifications");
        }
    }
}
