using System;
using System.Collections.Generic;
using System.Text;

namespace MyToDo.Events
{
    public class UpdateModel
    {
        public bool IsOpen { get; set; }
    }
    public class UpdateLoadingEvent : PubSubEvent<UpdateModel>
    {
    }
}
