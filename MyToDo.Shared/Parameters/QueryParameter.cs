using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyToDo.Shared.Parameters
{
    public class QueryParameter
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string? Search { get; set; } = null;

        public int UserId { get; set; }
    }
}