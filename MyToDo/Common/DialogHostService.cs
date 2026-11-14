using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace MyToDo.Common
{
    /// <summary>
    /// 对话主机服务
    /// </summary>
    public class DialogHostService : DialogService, IDialogHostService
    {
        public IContainerExtension containerExtension { get; }

        public DialogHostService(IContainerExtension container) : base(container)
        {
            this.containerExtension = container;
        }

        public async Task<IDialogResult> ShowDialog(string name, IDialogParameters pairs, string dialogHostName = "Root")
        {
            if (pairs == null)
                pairs = new DialogParameters();

            if (DialogHost.IsDialogOpen(dialogHostName))
            {
                return null; // 或者返回一个表示“已忽略”的结果
            }

            //从容器当中取出弹出窗口的实例
            var content = containerExtension.Resolve<object>(name);

            //验证实例的有效性
            if (!(content is FrameworkElement dialogContent))
                throw new NullReferenceException("对话框的内容必须是一个 FrameworkElement");

            if (dialogContent is FrameworkElement view && view.DataContext is null && ViewModelLocator.GetAutoWireViewModel(view) is null)
                ViewModelLocator.SetAutoWireViewModel(view, true);

            if (!(dialogContent.DataContext is IDialogHostAware viewModel))
                throw new NullReferenceException("对话框的 ViewModel 必须实现 IDialogAware 接口。");

            viewModel.DialogHostName = dialogHostName;

            DialogOpenedEventHandler eventHandler = (sender, eventArgs) =>
            {
                if (viewModel is IDialogHostAware aware)
                {
                    aware.OnDialogOpend(pairs);
                }
                eventArgs.Session.UpdateContent(content);
            };

            return (IDialogResult)await DialogHost.Show(dialogContent, viewModel.DialogHostName, eventHandler);
        }
    }
}