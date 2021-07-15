using System.Text.Json.Serialization;
using RestSharp;

namespace ErogeHelper.Model.Entity.Response
{
    public class JishoResponse
    {
        [JsonIgnore]
        public ResponseStatus StatusCode { get; init; }

        [JsonPropertyName("meta")]
        public MetaData? Meta { get; init; }

        [JsonPropertyName("data")]
        public Data[]? DataList { get; init; }

        public class MetaData
        {
            [JsonPropertyName("status")]
            public int Status { get; init; }
        }

        public class Data
        {
            [JsonPropertyName("slug")]
            public string? Slug { get; init; }


            [JsonPropertyName("is_common")]
            public bool IsCommon { get; init; }

            [JsonPropertyName("tags")]
            public string[]? Tags { get; init; }


            [JsonPropertyName("jlpt")]
            public string[]? Jlpt { get; init; }


            [JsonPropertyName("japanese")]
            public Japanese[]? JapaneseList { get; init; }

            [JsonPropertyName("senses")]
            public Sense[]? Senses { get; init; }

            [JsonPropertyName("attribution")]
            public Attribution? Attribution { get; init; }
        }

        public class Japanese
        {
            [JsonPropertyName("word")]
            public string? Word { get; init; }

            [JsonPropertyName("reading")]
            public string? Reading { get; init; }
        }

        public class Sense
        {
            [JsonPropertyName("english_definitions")]
            public string[]? EnglishDefinitions { get; init; }

            [JsonPropertyName("parts_of_speech")]
            public string[]? PartsOfSpeech { get; init; }

            [JsonPropertyName("links")]
            public Link[]? Links { get; init; }

            [JsonPropertyName("tags")]
            public object[]? Tags { get; init; }

            [JsonPropertyName("restrictions")]
            public object[]? Restrictions { get; init; }

            [JsonPropertyName("see_also")]
            public object[]? SeeAlso { get; init; }

            [JsonPropertyName("antonyms")]
            public object[]? Antonyms { get; init; }

            [JsonPropertyName("source")]
            public object[]? Source { get; init; }

            [JsonPropertyName("info")]
            public string[]? Info { get; init; }

            [JsonPropertyName("sentences")]
            public object[]? Sentences { get; init; }
        }

        public class Link
        {
            [JsonPropertyName("text")]
            public string? Text { get; init; }

            [JsonPropertyName("url")]
            public string? Url { get; init; }
        }

        public class Attribution
        {
            [JsonPropertyName("jmdict")]
            public bool Jmdict { get; init; }

            [JsonPropertyName("jmnedict")]
            public bool Jmnedict { get; init; }

            // Dbpedia url or "False"
            [JsonPropertyName("dbpedia")]
            public object? Dbpedia { get; init; }
        }
    }
}