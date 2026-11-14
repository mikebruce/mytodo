using Arch.EntityFrameworkCore.UnitOfWork;
using AutoMapper;
using MyToDo.Api.Context;
using MyToDo.Shared;
using MyToDo.Shared.Dtos;
using MyToDo.Shared.Parameters;
using System.Collections.ObjectModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MyToDo.Api.Service.implementation
{
    /// <summary>
    /// 待办事项
    /// </summary>
    public class ToDoService : IToDoService
    {
        public IUnitOfWork work;
        private readonly IMapper mapper;

        public ToDoService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            work = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<ApiResponse> AddAsync(ToDoDto model)
        {
            try
            {
                var todo = mapper.Map<ToDo>(model);
                todo.CreateDate = todo.UpdateDate = DateTime.Now;
                await work.GetRepository<ToDo>().InsertAsync(todo);
                if (await work.SaveChangesAsync() > 0)
                {
                    return new ApiResponse { IsSuccess = true, Result = todo };
                }
                return new ApiResponse { Message = "添加失败" };
            }
            catch (Exception exp)
            {
                return new ApiResponse { Message = exp.Message };
            }
        }

        public async Task<ApiResponse> DeleteAsync(int id, int UserId)
        {
            try
            {
                var service = work.GetRepository<ToDo>();
                var model = await service.GetFirstOrDefaultAsync(predicate: (t => t.Id.Equals(id) && t.UserId.Equals(UserId)));
                if (model != null)
                {
                    service.Delete(model);
                    if (await work.SaveChangesAsync() > 0)
                    {
                        return new ApiResponse { IsSuccess = true, Result = model };
                    }
                }
                return new ApiResponse { Message = "删除数据失败" };
            }
            catch (Exception exp)
            {
                return new ApiResponse { Message = exp.Message };
            }
        }

        public async Task<ApiResponse> GetAllAsync(QueryParameter parameter)
        {
            try
            {
                var repository = work.GetRepository<ToDo>();
                var todos = await repository.GetPagedListAsync(predicate:
                   (x => string.IsNullOrWhiteSpace(parameter.Search) ? true : x.Title.Contains(parameter.Search) && (x.UserId.Equals(parameter.UserId))),
                   pageIndex: parameter.PageIndex,
                   pageSize: parameter.PageSize,
                   orderBy: source => source.OrderByDescending(t => t.CreateDate));
                return new ApiResponse { IsSuccess = true, Result = todos, TotalCount = todos.TotalCount };
            }
            catch (Exception exp)
            {
                return new ApiResponse { IsSuccess = false, Message = exp.Message };
            }
        }

        public async Task<ApiResponse> GetAllAsync(ToDoParameter query)
        {
            try
            {
                var service = work.GetRepository<ToDo>();
                var models = await service.GetPagedListAsync(predicate: (t => (string.IsNullOrWhiteSpace(query.Search) ?
                true : t.Title.Contains(query.Search))
                && (!query.Status.HasValue ? true : t.Status.Equals(query.Status))
                && (t.UserId.Equals(query.UserId))),
                pageIndex: query.PageIndex,
                pageSize: query.PageSize,
                orderBy: source => source.OrderByDescending(t => t.Id)
                );

                return new ApiResponse { IsSuccess = true, Result = models, TotalCount = models.TotalCount };
            }
            catch (Exception exp)
            {
                return new ApiResponse { Message = exp.Message };
            }
        }

        public async Task<ApiResponse> GetSingleAsync(int id, int UserId)
        {
            try
            {
                var service = work.GetRepository<ToDo>();
                var model = await service.GetFirstOrDefaultAsync(predicate: (t => t.Id.Equals(id) && t.UserId.Equals(UserId)));
                if (model != null)
                {
                    return new ApiResponse { IsSuccess = true, Result = model };
                }
                else
                {
                    return new ApiResponse { Message = "没有查到数据" };
                }
            }
            catch (Exception exp)
            {
                return new ApiResponse { Message = exp.Message };
            }
        }

        public async Task<ApiResponse> UpdateAsync(ToDoDto model)
        {
            try
            {
                var todo = mapper.Map<ToDo>(model);
                var service = work.GetRepository<ToDo>();
                var entity = await service.GetFirstOrDefaultAsync(predicate: t => t.Id.Equals(todo.Id));
                if (todo.UserId != entity.UserId)
                {
                    return new ApiResponse { Message = "无权限编辑该数据" };
                }
                entity.Content = todo.Content;
                entity.Title = todo.Title;
                entity.Status = todo.Status;
                entity.UpdateDate = DateTime.Now;
                service.Update(entity);
                if (await work.SaveChangesAsync() > 0)
                {
                    return new ApiResponse { IsSuccess = true, Result = entity };
                }
                return new ApiResponse { Message = "更新失败" };
            }
            catch (Exception exp)
            {
                return new ApiResponse { Message = exp.Message };
            }
        }

        public async Task<ApiResponse> Summary(int UserId)
        {
            try
            {
                var todos = await work.GetRepository<ToDo>().GetAllAsync(predicate: x => x.UserId.Equals(UserId));

                var memos = await work.GetRepository<Memo>().GetAllAsync(predicate: x => x.UserId.Equals(UserId));

                SummaryDto summary = new SummaryDto();
                summary.ToDoCount = todos.Count();
                summary.MemoCount = memos.Count();
                summary.FinishToDoCount = todos.Where(t => t.Status == 1).Count();
                if (summary.ToDoCount == 0)
                {
                    summary.FinishToDoRate = "0%";
                }
                else
                {
                    summary.FinishToDoRate = (summary.FinishToDoCount / (double)summary.ToDoCount).ToString("0%");
                }
                summary.ToDoList = new ObservableCollection<ToDoDto>(mapper.Map<List<ToDoDto>>(todos.Where(t => t.Status == 0)));
                summary.MemoList = new ObservableCollection<MemoDto>(mapper.Map<List<MemoDto>>(memos));

                summary.ToDoList = new ObservableCollection<ToDoDto>(mapper.Map<List<ToDoDto>>(todos.Where(t => t.Status == 0)));

                return new ApiResponse { IsSuccess = true, Result = summary };
            }
            catch (Exception exp)
            {
                return new ApiResponse { IsSuccess = false, Message = exp.Message }; ;
            }
        }
    }
}