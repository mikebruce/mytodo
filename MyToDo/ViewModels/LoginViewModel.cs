using MyToDo.Common;
using MyToDo.Extensions;
using MyToDo.Service;
using MyToDo.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using static MyToDo.Common.SnackBar;

namespace MyToDo.ViewModels
{
    public class LoginViewModel : BindableBase, IDialogAware
    {
        public string Title { get; set; } = "记一下";
        public DialogCloseListener RequestClose { get; }
        public ILoginService LoginService { get; }

        public readonly IEventAggregator eventAggregator;

        public LoginViewModel(ILoginService loginService, IEventAggregator aggregator)
        {
            RegisterUserDto = new RegisterUserDto();
            ExecuteCommand = new DelegateCommand<string>(Execute);
            LoginService = loginService;
            eventAggregator = aggregator;
        }

        private int selectedIndex;

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { selectedIndex = value; RaisePropertyChanged(); }
        }

        private void Execute(string obj)
        {
            switch (obj)
            {
                case "Login":
                    Login();
                    break;

                case "LoginOut":
                    LoginOut();
                    break;

                case "GoRegister":
                    SelectedIndex = 1;
                    break;

                case "Return":
                    SelectedIndex = 0;
                    break;

                case "Register":
                    Register();
                    break;

                case "Forget":
                    SelectedIndex = 2;
                    break;

                case "Reset":
                    Reset();
                    break;
            }
        }

        public async void Reset()
        {
            if (string.IsNullOrWhiteSpace(RegisterUserDto.Account) || string.IsNullOrWhiteSpace(RegisterUserDto.PassWord))
            {
                await NotifyAsync("账号，密码都不能为空！", SnackBarEnum.Warning);
                return;
            }
            if (RegisterUserDto.PassWord != RegisterUserDto.NewPassWord)
            {
                await NotifyAsync("两次输入的密码不一致", SnackBarEnum.Warning);
                return;
            }
            var registerResult = await LoginService.ResetPwdAsync(new UserDto
            {
                Account = RegisterUserDto.Account,
                PassWord = RegisterUserDto.PassWord,
                UserName = "",
            });
            if (registerResult.IsSuccess)
            {
                await NotifyAsync("重置成功,你现在可以登录了", SnackBarEnum.Success);
                SelectedIndex = 0;
                return;
            }
            else
            {
                eventAggregator.SendMessage(registerResult.Message);
                return;
            }
        }

        private async void Register()
        {
            if (string.IsNullOrWhiteSpace(RegisterUserDto.Account) || string.IsNullOrWhiteSpace(RegisterUserDto.PassWord) || string.IsNullOrWhiteSpace(RegisterUserDto.UserName))
            {
                await NotifyAsync("账号、姓名、密码都不能为空！", SnackBarEnum.Warning);
                return;
            }
            if (RegisterUserDto.PassWord != RegisterUserDto.NewPassWord)
            {
                await NotifyAsync("两次输入的密码不一致", SnackBarEnum.Warning);
                return;
            }
            var registerResult = await LoginService.RegisterAsync(new UserDto
            {
                Account = RegisterUserDto.Account,
                PassWord = RegisterUserDto.PassWord,
                UserName = RegisterUserDto.UserName,
            });
            if (registerResult.IsSuccess)
            {
                await NotifyAsync("注册成功，现在你可以登录了！", SnackBarEnum.Success);
                SelectedIndex = 0;
                return;
            }
            else
            {
                eventAggregator.SendMessage(registerResult.Message);
                return;
            }
        }

        private RegisterUserDto registerUserDto;

        public RegisterUserDto RegisterUserDto
        {
            get { return registerUserDto; }
            set { registerUserDto = value; RaisePropertyChanged(); }
        }

        private void LoginOut()
        {
            AppSession.Clear();
            RequestClose.Invoke(new DialogResult(ButtonResult.No));
        }

        private async void Login()
        {
            if (string.IsNullOrWhiteSpace(Account) || string.IsNullOrWhiteSpace(PassWord))
            {
                await NotifyAsync("账号与密码都不能为空", SnackBarEnum.Warning);
                return;
            }
            var loginResult = await LoginService.LoginAsync(new LoginUserDto()
            {
                Account = Account,
                PassWord = PassWord
            });
            if (loginResult.IsSuccess)
            {
                AppSession.UserName = loginResult.Result.Account;
                AppSession.Token = loginResult.Token!;
                AppSession.RefreshToken = loginResult.RefreshToken!;
                await NotifyAsync("登录成功", SnackBarEnum.Success);
                await Task.Delay(500);
                RequestClose.Invoke(new DialogResult(ButtonResult.OK));
            }
            else
            {
                await NotifyAsync("登录失败", SnackBarEnum.Error);
                return;
            }
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        private string account;

        public string Account
        {
            get { return account; }
            set { account = value; RaisePropertyChanged(); }
        }

        public DelegateCommand<string> ExecuteCommand { get; private set; }

        private string password;

        public string PassWord
        {
            get { return password; }
            set { password = value; RaisePropertyChanged(); }
        }

        private SnackBarEnum snackBarKind = SnackBarEnum.Default;

        public SnackBarEnum SnackBarKind
        {
            get { return snackBarKind; }
            set { snackBarKind = value; RaisePropertyChanged(); }
        }

        private async Task NotifyAsync(string message, SnackBarEnum kind)
        {
            SnackBarKind = kind;
            await Task.Delay(100);
            eventAggregator.SendMessage(message, "Login");
        }
    }
}