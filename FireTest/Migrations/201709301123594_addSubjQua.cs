namespace FireTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addSubjQua : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Examinations", "SubjQua", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Examinations", "SubjQua");
        }
    }
}
