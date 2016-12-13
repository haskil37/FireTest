namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Examination : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TestQualifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdExamination = c.Int(nullable: false),
                        IdUser = c.String(),
                        Answers = c.String(),
                        RightOrWrong = c.String(),
                        TimeStart = c.DateTime(nullable: false),
                        Score = c.Int(nullable: false),
                        End = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TestQualificationAccesses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdExamination = c.Int(nullable: false),
                        IdUsers = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TestQualificationAccesses");
            DropTable("dbo.TestQualifications");
        }
    }
}
