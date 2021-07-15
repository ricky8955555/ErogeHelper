namespace ErogeHelper.Model.Entity.Table
{
    public class GameInfoTable
    {
        public string? Md5 { get; set; }

        // Saved as "186,143,123"
        public string? GameIdList { get; set; }

        public string? RegExp { get; set; }

        public string? TextractorSettingJson { get; set; }

        public bool IsLoseFocus { get; set; }

        public bool IsEnableTouchToMouse { get; set; }
    }
}