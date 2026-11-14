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
    public class MemoViewModel : NavigationViewModel
    {
        public IMemoService service;
        private readonly IDialogHostService dialogHost;

        public MemoViewModel(IMemoService service, IContainerProvider provider) : base(provider)
        {
            MemoDtos = new ObservableCollection<MemoDto>();
            ExecuteCommand = new DelegateCommand<string>(Execute);
            SelectedCommand = new DelegateCommand<MemoDto>(Selected);
            DeleteCommand = new DelegateCommand<MemoDto>(Delete);
            dialogHost = provider.Resolve<IDialogHostService>();
            this.service = service;
        }

        private async void Delete(MemoDto dto)
        {
            var dialogResult = await dialogHost.Question("温馨提示", $"确认删除备忘事项：" + dto.Title + "?");
            if (dialogResult.Result != Prism.Dialogs.ButtonResult.OK) { return; }
            UpdateLoading(true);
            var result = await service.DeleteAsync(dto.Id);
            if (result.IsSuccess)
            {
                var todo = MemoDtos.FirstOrDefault(t => t.Id == dto.Id);
                if (todo != null)
                {
                    MemoDtos.Remove(todo);
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

        private MemoDto currentDto;

        /// <summary>
        /// 编辑选中对象
        /// </summary>
        public MemoDto CurrentDto
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

        private void Add()
        {
            CurrentDto = new MemoDto();
            IsRightDrawerOpen = true;
        }

        private async void Selected(MemoDto dto)
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
                    var momo = MemoDtos.FirstOrDefault(t => t.Id == CurrentDto.Id);
                    if (momo != null)
                    {
                        momo.Title = CurrentDto.Title;

                        momo.Content = CurrentDto.Content;
                    }
                }
                IsRightDrawerOpen = false;
            }
            else
            {
                var result = await service.AddAsync(CurrentDto);
                if (result.IsSuccess)
                {
                    MemoDtos.Add(result.Result);
                    IsRightDrawerOpen = false;
                }
            }
            UpdateLoading(false);
        }

        public DelegateCommand<String> ExecuteCommand { get; private set; }
        public DelegateCommand<MemoDto> SelectedCommand { get; private set; }
        public DelegateCommand<MemoDto> DeleteCommand { get; private set; }
        private ObservableCollection<MemoDto> memoDtos;

        public ObservableCollection<MemoDto> MemoDtos
        {
            get { return memoDtos; }
            set { memoDtos = value; RaisePropertyChanged(); }
        }

        private async void GetDataAsync()
        {
            UpdateLoading(true);
            var result = await service.GetAllAsync(new QueryParameter()
            {
                PageIndex = 0,
                PageSize = 20,
                Search = Search,
                UserId = 0
            });
            if (result.IsSuccess)
            {
                MemoDtos.Clear();
                foreach (var item in result.Result.Items)
                {
                    MemoDtos.Add(item);
                }
            }
            UpdateLoading(false);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            IsRightDrawerOpen = false;
            base.OnNavigatedTo(navigationContext);

            GetDataAsync();
        }
    }
}