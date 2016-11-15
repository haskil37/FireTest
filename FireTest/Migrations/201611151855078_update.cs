namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TeacherAccesses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TeacherId = c.String(),
                        TeacherSubjects = c.String(),
                        TeacherQualifications = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Questions", "Tag", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Questions", "Tag");
            DropTable("dbo.TeacherAccesses");
        }
    }
}
