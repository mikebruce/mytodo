using Arch.EntityFrameworkCore.UnitOfWork;
using AutoMapper;
using MyToDo.Api.Context;
using MyToDo.Shared;
using MyToDo.Shared.Dtos;
using MyToDo.Shared.Parameters;

namespace MyToDo.Api.Service.implementation
{
    /// <summary>
    /// 备忘录
    /// </summary>
    public class MemoService : IMemoService
    {
        public IUnitOfWork work;
        private readonly IMapper mapper;

        public MemoService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            work = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<ApiResponse> AddAsync(MemoDto model)
        {
            try
            {
                var memo = mapper.Map<Memo>(model);
                memo.CreateDate = memo.UpdateDate = DateTime.Now;
                await work.GetRepository<Memo>().InsertAsync(memo);
                if (await work.SaveChangesAsync() > 0)
                {
                    return new ApiResponse { IsSuccess = true, Result = memo };
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
                var service = work.GetRepository<Memo>();
                var model = await service.GetFirstOrDefaultAsync(predicate: t => t.Id.Equals(id) &&
                t.UserId.Equals(UserId));
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

        public async Task<ApiResponse> GetAllAsync(QueryParameter query)
        {
            try
            {
                var service = work.GetRepository<Memo>();
                var models = await service.GetPagedListAsync(predicate: (t => string.IsNullOrWhiteSpace(query.Search) ?
                true : t.Title.Contains(query.Search) && t.UserId.Equals(query.UserId)),
                pageIndex: query.PageIndex,
                pageSize: query.PageSize,
                orderBy: source => source.OrderByDescending(t => t.Id)
                );
                return new ApiResponse { IsSuccess = true, Result = models };
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
                var service = work.GetRepository<Memo>();
                var model = await service.GetFirstOrDefaultAsync(predicate: t => t.Id.Equals(id) && t.UserId.Equals(UserId));
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

        public async Task<ApiResponse> UpdateAsync(MemoDto model)
        {
            try
            {
                var memo = mapper.Map<Memo>(model);
                var service = work.GetRepository<Memo>();
                var entity = await service.GetFirstOrDefaultAsync(predicate: t => t.Id.Equals(memo.Id));

                if (memo.UserId != entity.UserId)
                {
                    return new ApiResponse { Message = "无权限编辑该数据" };
                }

                entity.Content = memo.Content;
                entity.Title = memo.Title;
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
    }
}