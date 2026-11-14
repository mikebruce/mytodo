using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MyToDo.Shared.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    public class SummaryDto : BaseDto
    {
        private int toDoCount;
        private int finishToDoCount;
        private int memoCount;
        private string finishToDoRate;

        /// <summary>
        /// 待办总数
        /// </summary>
        public int ToDoCount
        {
            get { return toDoCount; }
            set { toDoCount = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// 完成待办总数
        /// </summary>
        public int FinishToDoCount
        {
            get { return finishToDoCount; }
            set { finishToDoCount = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// 备忘录数量
        /// </summary>
        public int MemoCount
        {
            get { return memoCount; }
            set { memoCount = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 完成比率
        /// </summary>
        public string FinishToDoRate
        {

            get { return finishToDoRate; }
            set { finishToDoRate = value; OnPropertyChanged(); }

        }
        private ObservableCollection<ToDoDto> todoList;
        private ObservableCollection<MemoDto> memoList;

        /// <summary>
        /// 待办列表
        /// </summary>
        public ObservableCollection<ToDoDto> ToDoList
        {
            get { return todoList; }
            set { todoList = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// 备忘录列表
        /// </summary>
        public ObservableCollection<MemoDto> MemoList
        {
            get { return memoList; }
            set { memoList = value; OnPropertyChanged(); }
        }
    }
}
