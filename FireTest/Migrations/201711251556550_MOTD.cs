namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MOTD : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MessageOfTheDays",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Group = c.String(),
                        Message = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.MessageOfTheDays");
        }
    }
}
