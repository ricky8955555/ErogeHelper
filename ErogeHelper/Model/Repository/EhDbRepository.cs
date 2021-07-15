using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ErogeHelper.Model.Entity.Table;
using Microsoft.Data.Sqlite;


namespace ErogeHelper.Model.Repository
{
    public class EhDbRepository : IDisposable
    {
        public EhDbRepository(string connStr)
        {
            _connection = new SqliteConnection(connStr);
        }

        private readonly SqliteConnection _connection;

        public string Md5 { get; set; } = string.Empty;

        public GameInfoTable? GetGameInfo() =>
            _connection.QuerySingleOrDefault<GameInfoTable>($"SELECT * FROM GameInfo WHERE Md5='{Md5}'");

        public async Task<GameInfoTable?> GetGameInfoAsync() =>
            await _connection
                .QuerySingleOrDefaultAsync<GameInfoTable>($"SELECT * FROM GameInfo WHERE Md5='{Md5}'")
                .ConfigureAwait(false);

        public void SetGameInfo(GameInfoTable gameInfoTable)
        {
            string query = "INSERT INTO GameInfo VALUES (@Md5, @GameIdList, @RegExp, @TextractorSettingJson, @IsLoseFocus, @IsEnableTouchToMouse)";
            _connection.Execute(query, gameInfoTable);
        }

        public async Task SetGameInfoAsync(GameInfoTable gameInfoTable)
        {
            if (gameInfoTable.Md5 == string.Empty)
                throw new ArgumentException("GameInfoTable Md5 can not be empty!");

            string query = "INSERT INTO GameInfo VALUES (@Md5, @GameIdList, @RegExp, @TextractorSettingJson, @IsLoseFocus, @IsEnableTouchToMouse)";
            await _connection.ExecuteAsync(query, gameInfoTable).ConfigureAwait(false);
        }

        public void UpdateGameInfo(GameInfoTable gameInfoTable)
        {
            if (gameInfoTable.Md5 == string.Empty)
                throw new ArgumentException("GameInfoTable Md5 can not be empty!");

            string query = @"
UPDATE GameInfo 
SET GameIdList=@GameIdList, RegExp=@RegExp, TextractorSettingJson=@TextractorSettingJson, IsLoseFocus=@IsLoseFocus, IsEnableTouchToMouse=@IsEnableTouchToMouse
WHERE Md5 = @Md5";
            _connection.Execute(query, gameInfoTable);
        }

        public async Task UpdateGameInfoAsync(GameInfoTable gameInfoTable)
        {
            if (gameInfoTable.Md5 == string.Empty)
                throw new ArgumentException("GameInfoTable Md5 can not be empty!");

            string query = @"
UPDATE GameInfo 
SET GameIdList=@GameIdList, RegExp=@RegExp, TextractorSettingJson=@TextractorSettingJson, IsLoseFocus=@IsLoseFocus, IsEnableTouchToMouse=@IsEnableTouchToMouse
WHERE Md5 = @Md5";
            await _connection.ExecuteAsync(query, gameInfoTable).ConfigureAwait(false);
        }

        public async Task DeleteGameInfoAsync()
        {
            string query =
                "DELETE FROM GameInfo WHERE Md5=@Md5";
            await _connection.ExecuteAsync(query, new { Md5 }).ConfigureAwait(false);
        }

        public List<UserTermTable> GetUserTerms() =>
            _connection.Query<UserTermTable>("SELECT * FROM UserTerm").ToList();

        public async Task<List<UserTermTable>> GetUserTermsAsync() =>
            (await _connection.QueryAsync<UserTermTable>("SELECT * FROM UserTerm").ConfigureAwait(false)).ToList();

        public void AddUserTerm(UserTermTable userTermTable)
        {
            string query = "INSERT INTO UserTerm VALUES (@From, @To)";
            _connection.Execute(query, userTermTable);
        }

        public async Task DeleteUserTermAsync(UserTermTable userTermTable)
        {
            const string? sqlStatement = "DELETE FROM UserTerm WHERE `From` = @From";
            await _connection.ExecuteAsync(sqlStatement, new { userTermTable.From }).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}