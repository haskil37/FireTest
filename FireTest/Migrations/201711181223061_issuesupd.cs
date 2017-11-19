namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class issuesupd : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Issues", "SubjectId", c => c.Int(nullable: false));
            AddColumn("dbo.Issues", "TeacherId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Issues", "TeacherId");
            DropColumn("dbo.Issues", "SubjectId");
        }
    }
}
