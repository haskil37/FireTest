namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addSpec : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Speciality", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Speciality");
        }
    }
}
