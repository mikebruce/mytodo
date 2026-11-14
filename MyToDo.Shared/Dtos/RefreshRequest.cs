using System;
using System.Collections.Generic;
using System.Text;

namespace MyToDo.Shared.Dtos
{
    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}