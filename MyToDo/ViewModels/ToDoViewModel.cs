using Microsoft.EntityFrameworkCore.Diagnostics;
using MyToDo.Common;
using MyToDo.Common.Models;
using MyToDo.Extensions;
using MyToDo.Service;
using MyToDo.Shared.Dtos;
using MyToDo.Shared.Parameters;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyToDo.ViewModels
{
    public class ToDoViewModel : NavigationViewModel
    {
        public IToDoService service;
        private readonly IDialogHostService dialogHost;

        public ToDoViewModel(IToDoService service, IContainerProvider provider) : base(provider)
        {
            ToDoDtos = new ObservableCollection<ToDoDto>();
            ExecuteCommand = new DelegateCommand<string>(Execute);
            SelectedCommand = new DelegateCommand<ToDoDto>(Selected);
            DeleteCommand = new DelegateCommand<ToDoDto>(Delete);
            dialogHost = provider.Resolve<IDialogHostService>();
            PrevPageCommand = new DelegateCommand(PrevPage);
            NextPageCommand = new DelegateCommand(NextPage);
            this.service = service;
        }

        private async void Delete(ToDoDto dto)
        {
            var dialogResult = await dialogHost.Question("温馨提示", $"确认删除待办事项：" + dto.Title + "?");
            if (dialogResult.Result != Prism.Dialogs.ButtonResult.OK) { return; }
            UpdateLoading(true);
            var result = await service.DeleteAsync(dto.Id);
            if (result.IsSuccess)
            {
                var todo = toDoDtos.FirstOrDefault(t => t.Id == dto.Id);
                if (todo != null)
                {
                    toDoDtos.Remove(todo);
                }
            }
            UpdateLoading(false);
        }

        private void Execute(string obj)
        {
            switch (obj)
            {
                case "Add":
                    Add();
                    break;

                case "Query":
                    PageIndex = 0;
                    GetDataAsync();
                    break;

                case "Save":
                    Save();
                    break;
            }
        }

        private bool isRightDrawerOpen;

        /// <summary>
        /// 右侧编辑窗口是否打开
        /// </summary>
        public bool IsRightDrawerOpen
        {
            get { return isRightDrawerOpen; }
            set { isRightDrawerOpen = value; RaisePropertyChanged(); }
        }

        private ToDoDto currentDto;

        /// <summary>
        /// 编辑选中对象
        /// </summary>
        public ToDoDto CurrentDto
        {
            get { return currentDto; }
            set { currentDto = value; RaisePropertyChanged(); }
        }

        private string search;

        /// <summary>
        /// 搜索条件
        /// </summary>
        public string Search
        {
            get { return search; }
            set { search = value; RaisePropertyChanged(); }
        }

        private int selectIndex;

        /// <summary>
        /// 下拉选中项
        /// </summary>
        public int SelectIndex
        {
            get { return selectIndex; }
            set { selectIndex = value; RaisePropertyChanged(); }
        }

        private void Add()
        {
            CurrentDto = new ToDoDto();
            IsRightDrawerOpen = true;
        }

        private async void Selected(ToDoDto dto)
        {
            UpdateLoading(true);
            var result = await service.GetFirstOrDefaultAsync(dto.Id);
            if (result.IsSuccess)
            {
                CurrentDto = result.Result;
                IsRightDrawerOpen = true;
            }
            UpdateLoading(false);
        }

        private async void Save()
        {
            if (string.IsNullOrWhiteSpace(CurrentDto.Title) || string.IsNullOrWhiteSpace(CurrentDto.Content))
            {
                return;
            }
            UpdateLoading(true);
            if (CurrentDto.Id > 0)
            {
                var result = await service.UpdateAsync(CurrentDto);
                if (result.IsSuccess)
                {
                    //更新数据 刷新UI
                    var todo = toDoDtos.FirstOrDefault(t => t.Id == CurrentDto.Id);
                    if (todo != null)
                    {
                        todo.Title = CurrentDto.Title;
                        todo.Status = CurrentDto.Status;
                        todo.Content = CurrentDto.Content;
                    }
                }
                IsRightDrawerOpen = false;
            }
            else
            {
                var result = await service.AddAsync(CurrentDto);
                if (result.IsSuccess)
                {
                    toDoDtos.Add(result.Result);
                    IsRightDrawerOpen = false;
                }
            }
            UpdateLoading(false);
        }

        public DelegateCommand<String> ExecuteCommand { get; private set; }
        public DelegateCommand<ToDoDto> SelectedCommand { get; private set; }
        public DelegateCommand<ToDoDto> DeleteCommand { get; private set; }
        private ObservableCollection<ToDoDto> toDoDtos;

        public ObservableCollection<ToDoDto> ToDoDtos
        {
            get { return toDoDtos; }
            set { toDoDtos = value; RaisePropertyChanged(); }
        }

        private async void GetDataAsync()
        {
            UpdateLoading(true);

            int? status = SelectIndex == 0 ? null : SelectIndex == 2 ? 1 : 0;

            var result = await service.GetAllFilterAsync(new ToDoParameter()
            {
                PageIndex = PageIndex,
                PageSize = PageSize,
                Search = Search,
                Status = status,
                UserId = 0
            });
            if (result.IsSuccess)
            {
                toDoDtos.Clear();
                foreach (var item in result.Result.Items)
                {
                    toDoDtos.Add(item);
                }
                TotalCount = result.TotalCount;
            }
            UpdateLoading(false);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            if (navigationContext.Parameters.ContainsKey("Value"))
            {
                SelectIndex = navigationContext.Parameters.GetValue<int>("Value");
                IsRightDrawerOpen = false;
            }
            else
                SelectIndex = 0;
            GetDataAsync();
        }

        #region
        private int pageIndex = 0;

        public int PageIndex
        {
            get => pageIndex;
            set { pageIndex = value; RaisePropertyChanged(); }
        }

        private int pageSize = 15;

        public int PageSize
        {
            get => pageSize;
            set { pageSize = value; RaisePropertyChanged(); }
        }

        private int totalCount;

        public int TotalCount
        {
            get => totalCount;
            set
            {
                totalCount = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(TotalPages));
                RaisePropertyChanged(nameof(CurrentPageDisplay));
            }
        }

        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public string CurrentPageDisplay => $"第 {PageIndex + 1} / {Math.Max(TotalPages, 1)} 页";

        public DelegateCommand PrevPageCommand { get; private set; }
        public DelegateCommand NextPageCommand { get; private set; }

        public void PrevPage()
        {
            if (PageIndex > 0)
            {
                PageIndex--;
                GetDataAsync();
            }
        }

        public void NextPage()
        {
            if (PageIndex < TotalPages - 1)
            {
                PageIndex++;
                GetDataAsync();
            }
        }

        #endregion
    }
}