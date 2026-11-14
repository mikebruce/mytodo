using MyToDo.Common;
using MyToDo.Common.Models;
using MyToDo.Extensions;
using MyToDo.Service;
using MyToDo.Shared.Dtos;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyToDo.ViewModels
{
    public class IndexViewModel : NavigationViewModel
    {
        private readonly IToDoService toDoService;
        private readonly IMemoService memoService;
        public readonly IDialogHostService dialog;

        private readonly IRegionManager regionManager;

        public IndexViewModel(IContainerProvider provider, IDialogHostService dialog) : base(provider)
        {
            Title = $"你好，{AppSession.UserName} ,今天是 ： {DateTime.Now.ToString("yyyy年MM月dd日 dddd")}";
            CreateTaskBars();
            ExecuteCommand = new DelegateCommand<string>(Execute);
            this.toDoService = provider.Resolve<IToDoService>();
            this.memoService = provider.Resolve<IMemoService>();
            this.regionManager = provider.Resolve<IRegionManager>();
            EditMemoCommand = new DelegateCommand<MemoDto>(EditMemo);
            EditToDoCommand = new DelegateCommand<ToDoDto>(EditTodo);
            ToDoCompltedCommand = new DelegateCommand<ToDoDto>(Complted);
            NavigateCommand = new DelegateCommand<TaskBar>(Navigate);
            this.dialog = dialog;
        }

        private void Navigate(TaskBar bar)
        {
            if (string.IsNullOrWhiteSpace(bar.Target))
                return;

            NavigationParameters parameters = new NavigationParameters();
            if (bar.Title == "已完成")
            {
                parameters.Add("Value", 2);
            }
            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate(bar.Target, parameters);
        }

        private async void Complted(ToDoDto dto)
        {
            try
            {
                UpdateLoading(true);

                var updateResult = await toDoService.UpdateAsync(dto);
                if (updateResult.IsSuccess)
                {
                    var toDoModel = Summary.ToDoList.FirstOrDefault(t => t.Id.Equals(dto.Id));
                    if (toDoModel != null)
                    {
                        Summary.ToDoList.Remove(toDoModel);
                    }
                    Summary.FinishToDoCount += 1;
                    Summary.FinishToDoRate = (Summary.FinishToDoCount / (decimal)Summary.ToDoCount).ToString("0%");
                    this.Refresh();
                    eventAggregator.SendMessage($"已完成");
                }
            }
            catch (Exception exp)
            {
            }
            finally { UpdateLoading(false); }
        }

        private async void EditTodo(ToDoDto dto)
        {
            await ExecuteToDo(dto);
        }

        private async void EditMemo(MemoDto dto)
        {
            await ExecuteMemo(dto);
        }

        private async void Execute(string obj)
        {
            switch (obj)
            {
                case "ToDo":
                    await ExecuteToDo(null);
                    break;

                case "Memo":
                    await ExecuteMemo(null);
                    break;
            }
        }

        public DelegateCommand<ToDoDto> ToDoCompltedCommand { get; private set; }
        public DelegateCommand<ToDoDto> EditToDoCommand { get; private set; }
        public DelegateCommand<MemoDto> EditMemoCommand { get; private set; }

        public DelegateCommand<TaskBar> NavigateCommand { get; private set; }

        #region 属性

        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(); }
        }

        public DelegateCommand<string> ExecuteCommand { get; private set; }
        private ObservableCollection<TaskBar> taskBars;

        public ObservableCollection<TaskBar> TaskBars
        {
            get { return taskBars; }
            set { taskBars = value; RaisePropertyChanged(); }
        }

        private SummaryDto summary;

        public SummaryDto Summary
        {
            get { return summary; }
            set { summary = value; RaisePropertyChanged(); }
        }

        #endregion 属性

        private async Task ExecuteToDo(ToDoDto model)
        {
            DialogParameters pairs = new DialogParameters();
            if (model != null)
            {
                pairs.Add("Value", new ToDoDto
                {
                    Id = model.Id,
                    Title = model.Title,
                    Content = model.Content,
                    Status = model.Status
                });
            }

            var dialogResult = await dialog.ShowDialog("AddToDoView", pairs);
            if (dialogResult != null && dialogResult.Result == Prism.Dialogs.ButtonResult.OK)
            {
                var todo = dialogResult.Parameters.GetValue<ToDoDto>("Value");
                if (todo.Id > 0)
                {
                    var editResult = await toDoService.UpdateAsync(todo);
                    if (editResult.IsSuccess)
                    {
                        var todoModel = Summary.ToDoList.FirstOrDefault(t => t.Id.Equals(todo.Id));
                        if (todoModel != null)
                        {
                            todoModel.Title = todo.Title;
                            todoModel.Content = todo.Content;
                        }
                    }
                }
                else
                {
                    var addresult = await toDoService.AddAsync(todo);
                    if (addresult.IsSuccess)
                    {
                        Summary.ToDoCount += 1;
                        Summary.FinishToDoRate = (Summary.FinishToDoCount / (decimal)Summary.ToDoCount).ToString("0%");
                        Summary.ToDoList.Add(addresult.Result);
                        this.Refresh();
                    }
                }
            }
        }

        private async Task ExecuteMemo(MemoDto model)
        {
            DialogParameters pairs = new DialogParameters();
            if (model != null)
            {
                pairs.Add("Value", new MemoDto
                {
                    Id = model.Id,
                    Title = model.Title,
                    Content = model.Content
                });
            }
            var dialogResult = await dialog.ShowDialog("AddMemoView", pairs);
            if (dialogResult.Result == Prism.Dialogs.ButtonResult.OK)
            {
                var memo = dialogResult.Parameters.GetValue<MemoDto>("Value");
                if (memo.Id > 0)
                {
                    var memoModel = Summary.MemoList.FirstOrDefault(t => t.Id.Equals(memo.Id));
                    if (memoModel != null)
                    {
                        memoModel.Title = memo.Title;
                        memoModel.Content = memo.Content;
                    }
                }
                else
                {
                    var addresult = await memoService.AddAsync(memo);
                    if (addresult.IsSuccess)
                    {
                        Summary.MemoCount += 1;
                        Summary.MemoList.Add(addresult.Result);
                        this.Refresh();
                    }
                }
            }
        }

        private void CreateTaskBars()
        {
            TaskBars = new ObservableCollection<TaskBar>();
            TaskBars.Add(new TaskBar() { Icon = "ClockFast", Title = "汇总", Color = "#FF0CA0FF", Target = "ToDoView" });
            TaskBars.Add(new TaskBar() { Icon = "ClockCheckOutline", Title = "已完成", Color = "#FF1ECA3A", Target = "ToDoView" });
            TaskBars.Add(new TaskBar() { Icon = "ChartLineVariant", Title = "完成比例", Color = "#FF02C6DC", Target = "" });
            TaskBars.Add(new TaskBar() { Icon = "PlaylistStar", Title = "备忘录", Color = "#FFFFA000", Target = "MemoView" });
        }

        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            var summaryResult = await toDoService.SummaryAsync();
            if (summaryResult.IsSuccess)
            {
                Summary = summaryResult.Result;
                this.Refresh();
            }
            base.OnNavigatedTo(navigationContext);
        }

        private async void Refresh()
        {
            taskBars[0].Content = Summary.ToDoCount.ToString();
            taskBars[1].Content = Summary.FinishToDoCount.ToString();
            taskBars[2].Content = Summary.FinishToDoRate;
            taskBars[3].Content = Summary.MemoCount.ToString();
        }
    }
}