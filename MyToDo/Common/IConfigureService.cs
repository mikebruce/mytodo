using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyToDo.Common
{
    public interface IConfigureService
    {
        void Configure();
    }
}
