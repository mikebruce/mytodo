using System;
using System.Collections.Generic;
using System.Text;

namespace MyToDo.Common
{
    public static class AppSession
    {
        public static string UserName { get; set; }

        public static string Token { get; set; }

        public static string RefreshToken { get; set; }

        public static void Clear()
        {
            Token = "";
            RefreshToken = "";
        }
    }
}