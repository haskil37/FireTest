namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class delTwoTable : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.FinishTestQualifications");
            DropTable("dbo.FinishTestQualificationAccesses");
        }
        
        public override void Down()
        {
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
            
        }
    }
}
