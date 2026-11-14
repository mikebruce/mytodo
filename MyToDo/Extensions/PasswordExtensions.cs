using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MyToDo.Extensions
{
    public class PasswordExtensions
    {
        public static string GetPassword(DependencyObject obj)
        {
            return (string)obj.GetValue(PasswordProperty);
        }

        public static void SetPassword(DependencyObject obj, string value)
        {
            obj.SetValue(PasswordProperty, value);
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordExtensions), new PropertyMetadata(string.Empty, OnPasswordPropertyChanged));

        private static void OnPasswordPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is not PasswordBox passwordBox) return;
            var passWord = obj as PasswordBox;
            string password = (string)e.NewValue ?? string.Empty;
            if (passWord != null && passWord.Password != password)
            {
                passWord.Password = password;
            }
        }
    }

    public class PasswordBehavior : Behavior<PasswordBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PasswordChanged += AssociatedObject_PasswordChanged;
        }

        private void AssociatedObject_PasswordChanged(object sender, RoutedEventArgs e)
        {
            //PasswordBox passwordBox = sender as PasswordBox;
            //string passWord = PasswordExtensions.GetPassword(passwordBox);
            //if (passwordBox != null && passwordBox.Password != passWord)
            //{
            //    PasswordExtensions.SetPassword(passwordBox, passWord);
            //}
            if (sender is not PasswordBox passwordBox) return;

            // 把控件里的新密码，回写到附加属性（进而通过绑定同步到 VM）
            if (PasswordExtensions.GetPassword(passwordBox) != passwordBox.Password)
            {
                PasswordExtensions.SetPassword(passwordBox, passwordBox.Password);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PasswordChanged -= AssociatedObject_PasswordChanged;
        }
    }
}