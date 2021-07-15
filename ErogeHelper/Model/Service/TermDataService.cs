using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;
using Caliburn.Micro;
using ErogeHelper.Model.Entity.Table;
using ErogeHelper.Model.Repository;
using ErogeHelper.Model.Service.Interface;
using ErogeHelper.ViewModel.Entity.NotifyItem;

namespace ErogeHelper.Model.Service
{
    public class TermDataService : ITermDataService
    {
        public TermDataService(EhDbRepository ehDbRepository)
        {
            _ehDbRepository = ehDbRepository;

            var userTerm = _ehDbRepository.GetUserTerms();
            userTerm.ForEach(term => _termDictionary.Add(term.From, term.To));
        }

        private readonly EhDbRepository _ehDbRepository;

        private readonly Dictionary<string, string> _termDictionary = new ();

        public BindableCollection<TermItem> GetBindableTermItems()
        {
            var result = new BindableCollection<TermItem>();
            foreach (var (from, to) in _termDictionary)
            {
                result.Add(new TermItem(from, to));
            }
            return result;
        }

        public void AddTermToDatabase(string from, string to)
        {
            _termDictionary.Add(from, to);
            _ehDbRepository.AddUserTerm(new UserTermTable(from, to));
        }

        public Dictionary<string, string> GetDictionary() => _termDictionary;

        public Task DeleteTermInDatabaseAsync(TermItem termItem) => 
            _ehDbRepository.DeleteUserTermAsync(new UserTermTable(termItem.SourceWord, termItem.TargetWord));

        //private int _count = 1234;
        //private Dictionary<int, string> countSourceWordDic = new();

        public string ProcessText(string originalText)
        {
            throw new NotImplementedException();
            //var tmpStr = originalText;
            //var termDatas = GetDictionary().ToList();
            //termDatas
            //    .Where(term => tmpStr.Contains(term.SourceWord)).ToList()
            //    .ForEach(term => 
            //    {
            //        tmpStr = tmpStr.Replace(term.SourceWord, $"{{{count}}}");
            //        countSourceWordDic.Add(count, term.SourceWord);
            //        count--;
            //    });
            
            //var tmpStrTranslatedResult = await _translatorFactory.GetTranslator(SelectedTranslator.TranslatorName)
            //    .TranslateAsync(tmpStr, _sourceLanguage, _targetLanguage);
            
            //return originalText;
        }

        public string FinalText(string translatedResult)
        {
            throw new NotImplementedException();
            //var finalResult = string.Empty;
            //try
            //{
            //    for(var i = 0; i < countSourceWordDic.Count; i++)
            //    {
            //        translatedResult = translatedResult.Replace(
            //            $"{{{++count}}}", 
            //            GetDictionary()[countSourceWordDic[count]]);
            //    }
            //    finalResult = translatedResult;
            //}
            //catch(Exception ex)
            //{
            //    Log.Error(ex);
            //}
            //finally
            //{
            //    count = 1234;
            //    countSourceWordDic.Clear();
            //}

            //return finalResult;
        }
    }
}