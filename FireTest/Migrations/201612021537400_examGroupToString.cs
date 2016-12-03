namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class examGroupToString : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Examinations", "Group", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Examinations", "Group", c => c.Int(nullable: false));
        }
    }
}
