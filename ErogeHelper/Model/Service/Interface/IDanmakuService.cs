using System.Collections.Generic;

namespace ErogeHelper.Model.Service.Interface
{
    public interface IDanmakuService
    {
        // to danma ku struct
        List<string> QueryDanmaku(string sourceText);
    }
}
