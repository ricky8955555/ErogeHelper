using System;
using ErogeHelper.Common.Enum;

namespace ErogeHelper.Common.Messenger
{
    public class ViewActionMessage
    {
        /// <summary>
        /// </summary>
        /// <param name="viewModelType">Sub name `ViewModel` mapping to `View`</param>
        /// <param name="viewType"></param>
        /// <param name="action"></param>
        /// <param name="dialogType"></param>
        /// <param name="context"></param>
        /// <param name="extraInfo"></param>
        public ViewActionMessage(
            Type viewModelType,
            ViewAction action,
            ModernDialog? dialogType = null,
            object? context = null,
            ViewType viewType = ViewType.Window,
            string extraInfo = "")
        {
            var viewName = string.Empty;

            if (viewType == ViewType.Window)
            {
                viewName = context is null ?
                    viewModelType.ToString().Replace("Model", string.Empty) :
                    viewModelType.ToString().Replace("Model", string.Empty)[..^4] + '.' + context;
            }
            else if (viewType == ViewType.Page)
            {
                viewName = viewModelType.ToString().Replace("Model", string.Empty)[..^4] + "Page";
            }

            WindowType = Type.GetType(viewName) ?? throw new InvalidCastException(viewName);
            Action = action;
            DialogType = dialogType;
            ExtraInfo = extraInfo;
        }

        public Type WindowType { get; set; }

        public ViewAction Action { get; set; }

        public ModernDialog? DialogType { get; set; }

        public string ExtraInfo { get; set; }
    }
}