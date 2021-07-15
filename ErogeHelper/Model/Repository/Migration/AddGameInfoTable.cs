using FluentMigrator;

namespace ErogeHelper.Model.Repository.Migration
{
    [Migration(20210325151400)]
    public class AddGameInfoTable : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("GameInfo")
                .WithColumn("Md5").AsString()
                .WithColumn("GameIdList").AsString()
                .WithColumn("RegExp").AsString().Nullable()
                .WithColumn("TextractorSettingJson").AsString()
                .WithColumn("IsLoseFocus").AsBoolean()
                .WithColumn("IsEnableTouchToMouse").AsBoolean();
        }

        public override void Down()
        {
            Delete.Table("GameInfo");
        }
    }
}