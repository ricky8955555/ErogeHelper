using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ErogeHelper.Model.Entity.Response
{
    public class MojiFetchResponse
    {
        [JsonPropertyName("result")]
        public ResultClass Result { get; set; } = new();

        public class ResultClass
        {
            [JsonPropertyName("word")]
            public Word Word { get; set; } = new();

            // details[0].title aka Shinhi，可能有多组词性
            [JsonPropertyName("details")]
            public List<Detail> Details { get; set; } = new();

            [JsonPropertyName("subdetails")]
            public List<SubDetail> Subdetails { get; set; } = new();

            // 与subdetails相对应，可能null
            [JsonPropertyName("examples")]
            public List<Example> Examples { get; set; } = new();
        }

        public class Word
        {
            [JsonPropertyName("objectId")]
            public string ObjectId { get; set; } = string.Empty;

            // details[0].title + subdetails[0]
            public string Excerpt { get; set; } = string.Empty;

            /// <summary>
            /// Surface
            /// </summary>
            [JsonPropertyName("spell")]
            public string Spell { get; set; } = string.Empty;
            public string Accent { get; set; } = string.Empty;

            /// <summary>
            /// Hiragana
            /// </summary>
            [JsonPropertyName("pron")]
            public string Pron { get; set; } = string.Empty;
            public string Romaji { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public string UpdatedBy { get; set; } = string.Empty;
        }

        public class Detail
        {
            public string WordId { get; set; } = string.Empty;

            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty; // 自动#一类#感
            public int Index { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public string UpdatedBy { get; set; } = string.Empty;
            public bool Converted { get; set; }
            public string ObjectId { get; set; } = string.Empty;
        }

        public class SubDetail
        {

            [JsonPropertyName("objectId")]
            public string ObjectId { get; set; } = string.Empty; // 536 通过这个来找，对应 examples-subdetailsId

            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty;
            public int Index { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public string WordId { get; set; } = string.Empty;
            public string DetailsId { get; set; } = string.Empty;
            public string UpdatedBy { get; set; } = string.Empty;
            public bool Converted { get; set; }
        }

        public class Example
        {
            [JsonPropertyName("subdetailsId")]
            public string SubdetailsId { get; set; } = string.Empty;

            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty;
            public int Index { get; set; }

            [JsonPropertyName("trans")]
            public string Trans { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public string WordId { get; set; } = string.Empty;
            public string UpdatedBy { get; set; } = string.Empty;
            public bool Converted { get; set; }
            public string ObjectId { get; set; } = string.Empty;
        }
    }
}