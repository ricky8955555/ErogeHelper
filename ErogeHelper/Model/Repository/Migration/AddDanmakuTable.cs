using FluentMigrator;

namespace ErogeHelper.Model.Repository.Migration
{
    [Migration(20210503223300)]
    class AddDanmakuTable : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("Danmaku")
                .WithColumn("?").AsString();
        }

        public override void Down()
        {
            Delete.Table("Danmaku");
        }
    }
}
