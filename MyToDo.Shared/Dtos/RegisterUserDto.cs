using System;
using System.Collections.Generic;
using System.Text;

namespace MyToDo.Shared.Dtos
{
    public class RegisterUserDto : BaseDto
    {
        private string account;
        private string username;
        private string password;
        private string newPassWord;

        public string Account
        {
            get { return account; }
            set { account = value; OnPropertyChanged(); }
        }

        public string UserName
        {
            get { return username; }
            set { username = value; OnPropertyChanged(); }
        }

        public string PassWord
        {
            get { return password; }
            set { password = value; OnPropertyChanged(); }
        }

        public string NewPassWord
        {
            get { return newPassWord; }
            set { newPassWord = value; OnPropertyChanged(); }
        }
    }
}