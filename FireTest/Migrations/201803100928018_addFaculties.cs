namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addFaculties : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Faculties",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        LevelsName = c.String(),
                        LevelsPictures = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Questions", "Faculties", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Questions", "Faculties");
            DropTable("dbo.Faculties");
        }
    }
}
