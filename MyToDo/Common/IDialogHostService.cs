using System;
using System.Collections.Generic;
using System.Text;

namespace MyToDo.Common
{
    public interface IDialogHostService:IDialogService
    {
        Task<IDialogResult> ShowDialog(string name, IDialogParameters pairs, string dialogHostName = "Root");
    }
}
