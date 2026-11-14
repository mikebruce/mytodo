
using Arch.EntityFrameworkCore.UnitOfWork.Collections;
using MyToDo.Common.Models;
using MyToDo.Shared;
using MyToDo.Shared.Dtos;
using MyToDo.Shared.Parameters;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyToDo.Service
{
    public interface IToDoService:IBaseService<ToDoDto>
    {
        Task<ApiResponse<PagedList<ToDoDto>>> GetAllFilterAsync(ToDoParameter parameter);

        Task<ApiResponse<SummaryDto>> SummaryAsync();
    }
}
