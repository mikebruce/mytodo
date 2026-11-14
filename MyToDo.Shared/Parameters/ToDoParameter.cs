using System;
using System.Collections.Generic;
using System.Text;

namespace MyToDo.Shared.Parameters
{
    public class ToDoParameter:QueryParameter
    {
        public int? Status { get; set; }
    }
}
