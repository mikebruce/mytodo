using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace MyToDo.Extensions
{
    public class DialogWindow : Window, IDialogWindow
    {
        public DialogWindow()
        {
            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        public IDialogResult Result { get; set; }
    }
}