using Arch.EntityFrameworkCore.UnitOfWork.Collections;
using MyToDo.Common.Models;
using MyToDo.Shared;
using MyToDo.Shared.Dtos;
using MyToDo.Shared.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyToDo.Service
{
    public class MemoService : BaseService<MemoDto>, IMemoService
    {
        public HttpRestClient client;
        public MemoService(HttpRestClient client) : base(client, "Memo")
        {
            this.client = client;
        }        
    }
}
