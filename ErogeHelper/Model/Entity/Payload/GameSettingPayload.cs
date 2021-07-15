using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace ErogeHelper.Model.Entity.Payload
{
    public class GameSettingPayload
    {
        public GameSettingPayload(string username, string password, string md5, Dictionary<string, string> names, string textSetting, string regExp)
        {
            Username = username;
            Password = password;
            Md5 = md5;
            Names = new List<GameNamePair>(names.Select(pair => new GameNamePair(pair.Key, pair.Value)));
            TextSetting = textSetting;
            RegExp = regExp;
        }

        public string Username { get; }

        public string Password { get; }

        public string Md5 { get; }

        public List<GameNamePair> Names { get; }

        public string TextSetting { get; }

        public string RegExp { get; }
    }

    public class GameNamePair
    {
        public GameNamePair(string type, string value)
        {
            Type = type;
            Value = value;
        }
        public string Type { get; } 
        public string Value { get; } 
    }
}