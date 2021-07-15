using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ErogeHelper.Model.Entity.Response
{
    /*
    * Search搜索不到时result状态
    * 
    * originalSearchText: "ドバサ"
    * searchResults: [{searchText: "ドバサ", count: 1,…}] 包含一个其他网站的索引
    * words: []
    */
    // NOTE: 没有使用的属性没有用 JsonPropertyName Attribute 标注, 所以会解析失败
    public class MojiSearchResponse
    {
        // Property
        [JsonPropertyName("result")]
        public ResultClass Result { get; set; } = new();

        [JsonPropertyName("error")]
        public string Error { get; set; } = string.Empty;

        // Class
        public class ResultClass
        {
            // Properties
            [JsonPropertyName("originalSearchText")]
            public string OriginalSearchText { get; set; } = string.Empty;

            public List<SearchResult> SearchResults { get; set; } = new();

            [JsonPropertyName("words")]
            public List<Word> Words { get; set; } = new();

            // Class
            public class SearchResult
            {
                public string SearchText { get; set; } = string.Empty;
                public string TarId { get; set; } = string.Empty;    // may be an address
                public int Type { get; set; }
                public int Count { get; set; }
                public string Title { get; set; } = string.Empty;
                public DateTime CreatedAt { get; set; }
                public DateTime UpdatedAt { get; set; }
                public string ObjectId { get; set; } = string.Empty; // BoVutOlaPR
            }

            public class Word
            {
                [JsonPropertyName("objectId")]
                public string ObjectId { get; set; } = string.Empty; // 198991321

                public string Excerpt { get; set; } = string.Empty;  // [惯用语] 主动承担。（自分からすすんで引き受ける。） 

                [JsonPropertyName("spell")]
                public string Spell { get; set; } = string.Empty;    // 買う
                                                                     // ?
                [JsonPropertyName("accent")]
                public string Accent { get; set; } = string.Empty;

                [JsonPropertyName("pron")]
                public string Pron { get; set; } = string.Empty;     // かう
                public string Romaji { get; set; } = string.Empty;
                public DateTime CreatedAt { get; set; }
                public DateTime UpdatedAt { get; set; }
                public string UpdatedBy { get; set; } = string.Empty;
            }
        }
    }
}