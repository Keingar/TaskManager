using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using testAvalonijaAbilitiesApp.ViewModels;
using ToDoList.DataModel;
using testAvalonijaAbilitiesApp.DataModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq.Expressions;
using ToDoList.Services;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using Avalonia.Input;

namespace ToDoList.ViewModels
{
    public partial class ToDoListViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> CreateNewTaskCommand { get; }

        public ReactiveCommand<Unit, Unit> AppyFilterButtonPressedCommand { get; }

        public user User { get; set; }

        public ToDoListViewModel(user CurrentUser)
        {
            CurrentDate = new DateTimeOffset(DateTime.Now);

            User = CurrentUser;

            ListItems = new ObservableCollection<TaskItem>(ToDoListService.GetItems(User));

            filteredList = new ObservableCollection<TaskItem>(ListItems);

            foreach (var item in ListItems)
            {
                item.ParentViewModel = this;

                switch(item.TaskType)
                {
                    case TaskType.Bool:
                        item.IsBoolTask = true;
                        break;
                    case TaskType.Int:
                        item.IsIntTask = true;
                        break;
                    case TaskType.Complex:
                        item.IsComplexTask = true;
                        break;
                }
            }
            ApplySortTasks();
            DeleteTaskButtonCommand = ReactiveCommand.Create<TaskItem>(DeleteTaskButtonPressed);

            CreateNewTaskCommand = ReactiveCommand.Create(CreateNewTaskExecute);

            EditTaskButtonCommand = ReactiveCommand.Create<TaskItem>(EditTask);

            FilterTasksByTitleCommand = ReactiveCommand.Create(ApplyFilterButton);

            AppyFilterButtonPressedCommand = ReactiveCommand.Create(ApplyFilterButton);

            ChangeBoolTaskProgressCommand = ReactiveCommand.Create<TaskItem>(ChangeBoolTaskProgress);
            ChangeIntTaskProgressCommand = ReactiveCommand.Create<TaskItem>(ChangeIntTaskProgress);

            ChangePossibleParentTasksCommand = ReactiveCommand.Create<TaskItem>(ChangePossibleParentTasks);

        }

        private void DeleteTaskButtonPressed(TaskItem taskItem)
        {
            ComplexTaskItem? SaveRelatedComplexItem = null;

            if (taskItem.ParentComplexTask != null)
            {
                SaveRelatedComplexItem = taskItem.ParentComplexTask;
            }

            SaveRelatedComplexItem?.SubTasks.Remove(taskItem);

            DeleteTaskButtonPressedHelperFunction(taskItem);

            SaveRelatedComplexItem?.CalculateProgress();
        }

        private void DeleteTaskButtonPressedHelperFunction(TaskItem taskItem)
        {
            if (taskItem is not ComplexTaskItem)
            {
                ListItems.Remove(taskItem);

                ToDoListService.DeleteItem(taskItem.TaskID);

                ApplyFilterButton();

                return;
            }

            if (taskItem is ComplexTaskItem complexTask)
            {

                foreach (TaskItem subtaskItem in complexTask.SubTasks)
                {
                    DeleteTaskButtonPressedHelperFunction(subtaskItem);
                }

                ListItems.Remove(taskItem);

                ToDoListService.DeleteItem(taskItem.TaskID);

                ApplyFilterButton();

                return;
            }
        }

        public ReactiveCommand<TaskItem, Unit> DeleteTaskButtonCommand { get; }

        public ReactiveCommand<TaskItem, Unit> EditTaskButtonCommand { get; }

        public ReactiveCommand<Unit, Unit> FilterTasksByTitleCommand { get; }

        public ReactiveCommand<TaskItem, Unit> ChangeBoolTaskProgressCommand { get; }
        public ReactiveCommand<TaskItem, Unit> ChangeIntTaskProgressCommand { get; }

        public ReactiveCommand<TaskItem, Unit> ChangePossibleParentTasksCommand { get; }


        public ObservableCollection<TaskItem> ListItems { get; }

        private ObservableCollection<TaskItem> filteredList;

        // here list and index for current task for popup when editing task. It's created to avoid creating list of tasks for every single task individually
        // List and index is updated every time when edit popup is opened
        private ObservableCollection<ComplexTaskItem>? _possibleTaskItems;

        public ObservableCollection<ComplexTaskItem>? PossibleTaskItems
        {
            get => _possibleTaskItems;
            set => this.RaiseAndSetIfChanged(ref _possibleTaskItems, value);
        }

        private int _currentParentTaskIndex;

        public int CurrentParentTaskIndex
        {
            get => _currentParentTaskIndex;
            set => this.RaiseAndSetIfChanged(ref _currentParentTaskIndex, value);
        }

        public void ChangePossibleParentTasks(TaskItem currentItem)
        {
            if (currentItem is ComplexTaskItem complexTask)
            {
                // We check if it's complex, if it's the same task, and if it's a subtask of the task
                PossibleTaskItems = new ObservableCollection<ComplexTaskItem>(ListItems.OfType<ComplexTaskItem>().Where(item => item != currentItem && !complexTask.IsSubtaskOf(item)));
            }
            else
            {
                PossibleTaskItems = new ObservableCollection<ComplexTaskItem>(ListItems.OfType<ComplexTaskItem>());
            }

            GetCurrentParentTaskIndex(currentItem);
        }
        // here I want to filter ListItems to only complex tasks and also check if 

        public void GetCurrentParentTaskIndex(TaskItem currentItem)
        {
            // find index of current item parent task 
            if (currentItem.ParentComplexTask == null)
            {
                CurrentParentTaskIndex = -1;
                return;
            }

            if(PossibleTaskItems != null)
            {
                CurrentParentTaskIndex = PossibleTaskItems.IndexOf(currentItem.ParentComplexTask);
            }

        }

        private void ChangeBoolTaskProgress(TaskItem taskItem)
        {
            taskItem.ChangeProgressForBoolTask();

            ApplyFilterButton();
        }

        private int _intTaskProgress = 0;
        public int NewIntTaskProgress
        {
            get => _intTaskProgress;
            set => this.RaiseAndSetIfChanged(ref _intTaskProgress, value);
        }


        private void ChangeIntTaskProgress(TaskItem taskItem)
        {
            taskItem.ChangeProgressForIntTask("3");

            ApplyFilterButton();
        }


        public ObservableCollection<TaskItem> FilteredList
        {
            get => filteredList;
            set => this.RaiseAndSetIfChanged(ref filteredList, value);
        }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        private DateTimeOffset? _ourDate;

        public DateTimeOffset? OurDate
        {
            get => _ourDate;
            set => this.RaiseAndSetIfChanged(ref _ourDate, value);
        }

        private int _ourTaskType = 0;
        public int OurTaskType
        {
            get => _ourTaskType;
            set => this.RaiseAndSetIfChanged(ref _ourTaskType, value);
        }

        private int _ourTaskProgress = 0;
        public int OurTaskProgress
        {
            get => _ourTaskProgress;
            set => this.RaiseAndSetIfChanged(ref _ourTaskProgress, value);
        }

        private void CreateNewTaskExecute()
        {
            TaskType correctTaskType;


            // I want task class to be complex if OurTaksType 2 and if OurTask is anything else then I want class to be TaskItem
            TaskItem task = OurTaskType == 2 ? new ComplexTaskItem() : new TaskItem();




            if(OurTaskProgress < 1)
            {
                OurTaskProgress = 1;
            }

            switch (OurTaskType)
            {
                case 0:
                    correctTaskType = TaskType.Bool;
                    task.MaxIntProgress = 1;
                    task.IsBoolTask = true;
                    break;
                case 1:
                    correctTaskType = TaskType.Int;
                    task.MaxIntProgress = OurTaskProgress;

                    task.IsIntTask = true;
                    break;
                case 2:
                    correctTaskType = TaskType.Complex;
                    task.MaxIntProgress = OurTaskProgress;

                    task.IsComplexTask = true;
                    break;
                default:
                    correctTaskType = TaskType.Bool;
                    task.MaxIntProgress = 1;

                    break;
            }

            task.Title = Title;
            task.TaskDescription = Description;

            if(OurDate != null)
            {
                task.DueDate = OurDate.Value.DateTime; 
            }

            task.TaskType = correctTaskType;
            task.IsDone = false;
            task.DateOfCreation = DateTime.Now;
            task.IsRoutine = false;
            task.FrequencyInDays = null;
            task.CurrentIntProgress = 0;

            task.UserOwner = User;

            task.ParentViewModel = this;

            task.TaskID = ToDoListService.AddItem(task);

            ListItems.Add(task);

            ApplyFilterButton();

        }

        private void EditTask(TaskItem taskItem)
        {

            taskItem.RaisePropertyChanged(nameof(taskItem.Title));
            taskItem.RaisePropertyChanged(nameof(taskItem.TaskDescription));
            taskItem.RaisePropertyChanged(nameof(taskItem.DueDate));
            taskItem.RaisePropertyChanged(nameof(taskItem.MaxIntProgress));

            ToDoListService.UpdateTaskInfo(taskItem.TaskID, taskItem.Title, taskItem.TaskDescription, taskItem.DueDate, taskItem.MaxIntProgress);

            if(CurrentParentTaskIndex != -1 && PossibleTaskItems != null)
            {
                // create variable of  PossibleTaskItems[CurrentParentTaskIndex] of type complex taks item


                ComplexTaskItem complexTask = PossibleTaskItems[CurrentParentTaskIndex];

                ComplexTaskItem? savedOldParentTaskItem = null;

                if(taskItem.ParentComplexTask != null)
                {
                    // I'm not recalculating progress in this if statement because new subtasks could still be in the same tree which may result in incorrect progress calculation
                    savedOldParentTaskItem = taskItem.ParentComplexTask; 
                }
                

                taskItem.ParentComplexTask = complexTask;

                complexTask.SubTasks.Add(taskItem);

                if(savedOldParentTaskItem != null)
                {
                    savedOldParentTaskItem.SubTasks.Remove(taskItem);
                    ToDoListService.RemoveTaskRelationship(savedOldParentTaskItem.TaskID, taskItem.TaskID);
                }

                ToDoListService.CreateTaskRelationshipInSQL(taskItem.ParentComplexTask.TaskID, taskItem.TaskID);

                savedOldParentTaskItem?.CalculateProgress(); // this call can't be useless if complexTask and savedOldParentTaskItem are in the same tree but if not then it's needed

                complexTask.CalculateProgress();

            }

            ApplyFilterButton();
        }

        private string _filterText = "";
        public string FilterText
        {
            get => _filterText;
            set => this.RaiseAndSetIfChanged(ref _filterText, value);
        }
        public void ResetFilter()
        {
            FilteredList = new ObservableCollection<TaskItem>(ListItems);

            ChosenIsFinishedFilter = 0;
            ChosenPeriodFilter = 0;
            IsComplexTasksEnabled = true;
            IsIntTasksEnabled = true;
            IsBoolTasksEnabled = true;
            FilterText = "";

            ApplySortTasks();
        }

        private int _chosenIsFinishedFilter = 0;
        public int ChosenIsFinishedFilter
        {
            get => _chosenIsFinishedFilter;
            set => this.RaiseAndSetIfChanged(ref _chosenIsFinishedFilter, value);
        }

        private int _chosenPeriodFilter = 0;
        public int ChosenPeriodFilter
        {
            get => _chosenPeriodFilter;
            set => this.RaiseAndSetIfChanged(ref _chosenPeriodFilter, value);
        }

        private bool _isBoolTasksEnabled = true;

        public bool IsBoolTasksEnabled
        {
            get => _isBoolTasksEnabled;
            set => this.RaiseAndSetIfChanged(ref _isBoolTasksEnabled, value);
        }

        private bool _isComplexTasksEnabled = true;

        public bool IsComplexTasksEnabled
        {
            get => _isComplexTasksEnabled;
            set => this.RaiseAndSetIfChanged(ref _isComplexTasksEnabled, value);
        }

        private bool _isIntTasksEnabled = true;

        public bool IsIntTasksEnabled
        {
            get => _isIntTasksEnabled;
            set => this.RaiseAndSetIfChanged(ref _isIntTasksEnabled, value);
        }

        public void ApplyFilterButton()
        {
            // ChosenIsFinishedFilter (int) (0 - All, 1 - Finished, 2 - Unfinished)
            // ChosenPeriodFilter (int) (0 - All, 1 - Today, 2 - This Week, 3 - This Month)
            // IsComplexTasksEnabled (bool)
            // IsIntTasksEnabled (bool)
            // IsBoolTasksEnabled (bool)

            var filtered = ListItems.AsEnumerable();

            // Apply IsFinished filter
            switch (ChosenIsFinishedFilter)
            {
                case 1:
                    filtered = filtered.Where(task => task.IsDone);
                    break;
                case 2:
                    filtered = filtered.Where(task => !task.IsDone);
                    break;
            }

            // Apply Period filter
            var now = DateTime.Now;

            switch (ChosenPeriodFilter)
            {
                case 1:
                    filtered = filtered.Where(task => task.DueDate.HasValue && task.DueDate.Value.Date == now.Date);
                    break;
                case 2:
                    var endOfWeek = now.Date.AddDays(7);
                    filtered = filtered.Where(task => task.DueDate.HasValue && task.DueDate.Value.Date >= now.Date && task.DueDate.Value.Date <= endOfWeek);
                    break;
                case 3:
                    var endOfNextMonth = now.AddMonths(1).Date;
                    filtered = filtered.Where(task => task.DueDate.HasValue && task.DueDate.Value.Date >= now.Date && task.DueDate.Value.Date <= endOfNextMonth);
                    break;
            }

            // Apply TaskType filters
            filtered = filtered.Where(task =>
                (IsComplexTasksEnabled && task.TaskType == TaskType.Complex) ||
                (IsIntTasksEnabled && task.TaskType == TaskType.Int) ||
                (IsBoolTasksEnabled && task.TaskType == TaskType.Bool)
            );

            // filter by using search box
            if (string.IsNullOrWhiteSpace(FilterText))
            {
                FilteredList = new ObservableCollection<TaskItem>(filtered);
            }
            else
            {
                   FilteredList = new ObservableCollection<TaskItem>(
                    filtered.Where(item => item.Title.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));
            }

            ApplySortTasks();
        }

        private int _chosenSort = 1;
        public int ChosenSort
        {
            get => _chosenSort;
            set
            {
                this.RaiseAndSetIfChanged(ref _chosenSort, value);
                {
                    ApplySortTasks();
                }
            }
        }

        private void ApplySortTasks()
        {

            IEnumerable<TaskItem> sortedList = FilteredList;

            switch (ChosenSort)
            {
                case 0:
                    // Sort by title
                    sortedList = FilteredList.OrderBy(item => item.Title);
                    break;
                case 1:
                    // Sort by progress
                    sortedList = FilteredList.OrderByDescending(item =>
                        item.MaxIntProgress > 0 ? (double?)item.CurrentIntProgress / item.MaxIntProgress : 0);
                    break;
                case 2:
                    // Sort by due date
                    sortedList = FilteredList.OrderBy(item => item.DueDate);
                    break;
            }

            FilteredList = new ObservableCollection<TaskItem>(sortedList);
        }
        static public DateTime MinYear => new(DateTime.Now.Year, 1, 1);
        public DateTimeOffset CurrentDate { get; set; }


    }
}