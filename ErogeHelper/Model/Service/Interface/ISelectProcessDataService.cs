using System.Threading.Tasks;
using Caliburn.Micro;
using ErogeHelper.ViewModel.Entity.NotifyItem;

namespace ErogeHelper.Model.Service.Interface
{
    public interface ISelectProcessDataService
    {
        Task RefreshBindableProcComboBoxAsync(BindableCollection<ProcComboBoxItem> refData, bool allApp = false);
    }
}