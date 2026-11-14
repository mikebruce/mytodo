using System;
using System.Collections.Generic;
using System.Text;

namespace MyToDo.Shared.Dtos
{
    public class LoginUserDto : BaseDto
    {
        private string account;

        private string password;

        public string Account
        {
            get { return account; }
            set { account = value; OnPropertyChanged(); }
        }

        public string PassWord
        {
            get { return password; }
            set { password = value; OnPropertyChanged(); }
        }
    }
}