using Arch.EntityFrameworkCore.UnitOfWork.Collections;
using MyToDo.Shared;
using MyToDo.Shared.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyToDo.Service
{
    public interface IBaseService<TEntity> where TEntity:class
    {
        Task<ApiResponse<TEntity>> AddAsync(TEntity entity);
        Task<ApiResponse<TEntity>> UpdateAsync(TEntity entity);
        Task<ApiResponse> DeleteAsync(int Id);
        Task<ApiResponse<TEntity>> GetFirstOrDefaultAsync(int Id);
        Task<ApiResponse<PagedList<TEntity>>> GetAllAsync(QueryParameter query); 


    }
}
