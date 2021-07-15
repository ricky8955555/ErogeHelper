using ErogeHelper.ViewModel.Entity.NotifyItem;

namespace ErogeHelper.Common.Messenger
{
    public class NewTextThreadSelectedMessage
    {
        public NewTextThreadSelectedMessage(HookMapItem hookMapItem, bool status)
        {
            HookMapItem = hookMapItem;
            Status = status;
        }

        public HookMapItem HookMapItem { get; }
        public bool Status { get; }
    }
}
