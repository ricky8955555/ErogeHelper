using ErogeHelper.Common;
using ErogeHelper.Common.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ErogeHelper.Common.Constraint;
using System.Threading;
using System.Threading.Tasks;
using ErogeHelper.Common.Entity;
using System.Text.Json.Serialization;

namespace ErogeHelper.Model.Repository
{
    public class EhConfigRepository
    {
        public string AppDataDir { get; }

        /// <summary>
        /// We generate file "ErogeHelper\EhSettings.json" to the specified directory
        /// </summary>
        /// <param name="rootDir"></param>
        public EhConfigRepository(string rootDir)
        {
            AppDataDir = Path.Combine(rootDir, "ErogeHelper");

            _configFilePath = Path.Combine(AppDataDir, "EhSettings.json");
            _localSetting = ReadLocalSetting(_configFilePath);
        }

        public void ClearConfig()
        {
            _localSetting.Clear();

            File.WriteAllText(_configFilePath, JsonSerializer.Serialize(_localSetting));
        }

        #region Private Methods

        private readonly string _configFilePath;
        private readonly Dictionary<string, object?> _localSetting;

        private static Dictionary<string, object?> ReadLocalSetting(string settingPath)
        {
            if (!File.Exists(settingPath))
            {
                FileInfo file = new(settingPath);
                // If the directory already exists, this method does nothing.
                file.Directory?.Create();

                File.WriteAllText(file.FullName, JsonSerializer.Serialize(new Dictionary<string, object?>()));
            }

            var options = new JsonSerializerOptions();
            options.Converters.Add(new DictionaryJsonConverter());

            string rawText = File.ReadAllText(settingPath);
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(rawText, options) ?? new Dictionary<string, object?>();
        }

        private T GetValue<T>(T defaultValue, [CallerMemberName] string propertyName = "")
        {
            if (_localSetting.TryGetValue(propertyName, out object? value))
            {
                try
                {
                    if (value is not null)
                    {
                        return (T)value;
                    }
                }
                catch (InvalidCastException)
                {
                }
            }

            return defaultValue;
        }

        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        private void SetValue<T>(T value, [CallerMemberName] string propertyName = "")
        {
            if (_localSetting.TryGetValue(propertyName, out object? outValue) && value!.Equals(outValue))
            {
                return;
            }

            _localSetting[propertyName] = value!;
            Log.Debug($"{propertyName} changed to {value}");
            _ = Task.Run(async () =>
              {
                  await _semaphoreSlim.WaitAsync();
                  try
                  {
                      await File.WriteAllTextAsync(
                          _configFilePath,
                          JsonSerializer.Serialize(_localSetting, new JsonSerializerOptions { WriteIndented = true }));
                  }
                  catch (Exception ex)
                  {
                      Log.Error(ex);
                  }
                  finally
                  {
                      _semaphoreSlim.Release();
                  }
              });
        }

        private class DictionaryJsonConverter : JsonConverter<Dictionary<string, object?>>
        {
            public override Dictionary<string, object?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var newOptions = new JsonSerializerOptions(options);
                newOptions.Converters.Add(this);

                var properties = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(ref reader, options);
                var dictionary = new Dictionary<string, object?>();

                foreach (var property in properties!)
                {
                    var value = property.Value;

                    object? newValue = value.ValueKind switch
                    {
                        JsonValueKind.Undefined => null,
                        JsonValueKind.Object => JsonSerializer.Deserialize<Dictionary<string, object>>(value.GetRawText(), newOptions),
                        JsonValueKind.Array => JsonSerializer.Deserialize<IEnumerable<object>>(value.GetRawText(), newOptions),
                        JsonValueKind.String => value.GetString(),
                        JsonValueKind.Number => value.GetInt32(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Null => null,
                        _ => throw new NotSupportedException()
                    };

                    dictionary.Add(property.Key, newValue);
                }

                return dictionary;
            }

            public override void Write(Utf8JsonWriter writer, Dictionary<string, object?> value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, value, options);
            }
        }

        #endregion

        #region Local Properties

        public string EhServerBaseUrl
        {
            get => GetValue(DefaultConfigValuesStore.EhServerUrl);
            set => SetValue(value);
        }

        public bool MonitorClipboard
            {
            get => GetValue(DefaultConfigValuesStore.MonitorClipboard);
            set => SetValue(value);
        }

        public int MaxAcceptTextLength
        {
            get => GetValue(DefaultConfigValuesStore.MaxAcceptTextLength);
            set => SetValue(value);
        }

        public string AssistiveTouchPostion
        {
            get => GetValue(DefaultConfigValuesStore.AssistiveTouchPostion);
            set => SetValue(value);

        }

        public double FontSize
        {
            get => GetValue(DefaultConfigValuesStore.FontSize);
            set => SetValue(value);
        }

        public bool EnableMeCab
        {
            get => GetValue(DefaultConfigValuesStore.EnableMeCab);
            set => SetValue(value);
        }

        public bool UseDanmaku
        {
            get => GetValue(DefaultConfigValuesStore.UseDanmaku);
            set => SetValue(value);
        }

        public bool UseMoveableTextControl
        {
            get => GetValue(DefaultConfigValuesStore.UseOutsideWindow);
            set => SetValue(value);
        }

        public bool ShowAppendText
        {
            get => GetValue(DefaultConfigValuesStore.ShowAppendText);
            set => SetValue(value);
        }

        public bool PasteToDeepL
        {
            get => GetValue(DefaultConfigValuesStore.PasteToDeepL);
            set => SetValue(value);
        }

        public TextTemplateType TextTemplateConfig
        {
            get => GetValue(DefaultConfigValuesStore.TextTemplate);
            set => SetValue(value);
        }

        public bool KanaDefault
        {
            get => GetValue(DefaultConfigValuesStore.KanaDefault);
            set => SetValue(value);
        }

        public bool KanaTop
        {
            get => GetValue(DefaultConfigValuesStore.KanaTop);
            set => SetValue(value);
        }

        public bool KanaBottom
        {
            get => GetValue(DefaultConfigValuesStore.KanaBottom);
            set => SetValue(value);
        }

        public bool Romaji
        {
            get => GetValue(DefaultConfigValuesStore.Romaji);
            set => SetValue(value);
        }

        public bool Hiragana
        {
            get => GetValue(DefaultConfigValuesStore.Hiragana);
            set => SetValue(value);
        }

        public bool Katakana
        {
            get => GetValue(DefaultConfigValuesStore.Katakana);
            set => SetValue(value);
        }

        public bool MojiDictEnable
        {
            get => GetValue(DefaultConfigValuesStore.MojiDictEnable);
            set => SetValue(value);
        }

        public string MojiSessionToken => GetValue(DefaultConfigValuesStore.MojiSessionToken);

        public bool JishoDictEnable
        {
            get => GetValue(DefaultConfigValuesStore.JishoDictEnable);
            set => SetValue(value);
        }

        public bool UseTermTable
        {
            get => GetValue(DefaultConfigValuesStore.UseTermTable);
            set => SetValue(value);
        }
        public TransLanguage SrcTransLanguage
        {
            get => GetValue(DefaultConfigValuesStore.TransSrcLanguage);
            set => SetValue(value);
        }
        public TransLanguage TargetTransLanguage
        {
            get => GetValue(DefaultConfigValuesStore.TransTargetLanguage);
            set => SetValue(value);
        }

        public bool BaiduApiEnable
        {
            get => GetValue(DefaultConfigValuesStore.BaiduApiEnable);
            set => SetValue(value);
        }
        public string BaiduApiAppid
        {
            get => GetValue(DefaultConfigValuesStore.BaiduApiAppid);
            set => SetValue(value);
        }
        public string BaiduApiSecretKey
        {
            get => GetValue(DefaultConfigValuesStore.BaiduApiSecretKey);
            set => SetValue(value);
        }

        public bool YeekitEnable
        {
            get => GetValue(DefaultConfigValuesStore.YeekitEnable);
            set => SetValue(value);
        }

        public bool BaiduWebEnable
        {
            get => GetValue(DefaultConfigValuesStore.BaiduWebEnable);
            set => SetValue(value);
        }

        public bool CaiyunEnable
        {
            get => GetValue(DefaultConfigValuesStore.CaiyunEnable);
            set => SetValue(value);
        }
        public string CaiyunToken
        {
            get => GetValue(DefaultConfigValuesStore.CaiyunDefaultToken);
            set => SetValue(value);
        }

        public bool AlapiEnable
        {
            get => GetValue(DefaultConfigValuesStore.AlapiEnable);
            set => SetValue(value);
        }

        public bool YoudaoEnable
        {
            get => GetValue(DefaultConfigValuesStore.YoudaoEnable);
            set => SetValue(value);
        }

        public bool NiuTransEnable
        {
            get => GetValue(DefaultConfigValuesStore.NiuTransEnable);
            set => SetValue(value);
        }
        public string NiuTransApiKey
        {
            get => GetValue(DefaultConfigValuesStore.NiuTransApiKey);
            set => SetValue(value);
        }

        public bool GoogleCnEnable
        {
            get => GetValue(DefaultConfigValuesStore.GoogleCnEnable);
            set => SetValue(value);
        }

        public bool TencentMtEnable
        {
            get => GetValue(DefaultConfigValuesStore.TencentApiEnable);
            set => SetValue(value);
        }
        public string TencentMtSecretId
        {
            get => GetValue(DefaultConfigValuesStore.TencentApiSecretId);
            set => SetValue(value);
        }
        public string TencentMtSecretKey
        {
            get => GetValue(DefaultConfigValuesStore.TencentApiSecretKey);
            set => SetValue(value);
        }

        public bool CloudEnable
        {
            get => GetValue(DefaultConfigValuesStore.CloudEnable);
            set => SetValue(value);
        }

        #endregion
    }
}