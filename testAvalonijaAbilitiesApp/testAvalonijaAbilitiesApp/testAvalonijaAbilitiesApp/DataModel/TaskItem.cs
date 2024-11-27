using ReactiveUI;
using System;
using System.Reactive;
using ToDoList.ViewModels;
using testAvalonijaAbilitiesApp.DataModel;
using ToDoList.Services;

namespace ToDoList.DataModel
{
    public class TaskItem : ReactiveObject // I decided to inherit from ReactiveObject to make data binding easier
    {
        #pragma warning disable CS8618 // disabled warning because otherwise I will need to duplicate a lot of code in ToDoListService when adding item and also change parameter for functions
        // In ToDoListService I create either TaskItem or ComplexTaskItem instance depending on type and because of that code is way clearer when there is no paremeters in constructor
        public TaskItem()
        {
           ChangeProgressButtonPressedCommand = ReactiveCommand.Create(ChangeProgressButtonPressed);

            ChangedProgressForIntTaskCommand = ReactiveCommand.Create<string>(ChangeProgressForIntTask);

        }
        #pragma warning restore CS8618

        public int TaskID { get; set; } 
        public string Title { get; set; } 
        public string TaskDescription { get; set; } = String.Empty; 
        public bool IsDone { get; set; } 
        public TaskType TaskType { get; set; } 
        public DateTime DateOfCreation { get; set; }  

        public DateTime? DueDate { get; set; }

        public user UserOwner { get; set; }

        public bool IsRoutine { get; set; }
        public int? FrequencyInDays { get; set; }

        private int? currentIntProgress; 
        private int? maxIntProgress; 
        public ComplexTaskItem ParentComplexTask { get; set; }

        public ToDoListViewModel ParentViewModel { get; set; }

        public ReactiveCommand<Unit, Unit> ChangeProgressButtonPressedCommand { get; }
        public ReactiveCommand<string, Unit> ChangedProgressForIntTaskCommand { get; }

        public DateTimeOffset DueDateOffset
        {
            get => new(DueDate ?? DateTime.Now);
            set => DueDate = value.DateTime;
        }



        private bool changeProgressButtonIsOpen;
        public bool ChangeProgressButtonIsOpen
        {
            get => changeProgressButtonIsOpen;
            set => this.RaiseAndSetIfChanged(ref changeProgressButtonIsOpen, value);
        }
        public void ChangeProgressButtonPressed() => ChangeProgressButtonIsOpen = !ChangeProgressButtonIsOpen;

        public int? CurrentIntProgress
        {
            get => currentIntProgress;
            set => this.RaiseAndSetIfChanged(ref currentIntProgress, value);
        }

        public int? MaxIntProgress
        {
            get => maxIntProgress;
            set => this.RaiseAndSetIfChanged(ref maxIntProgress, value);
        }

        public void ChangeProgressForBoolTask()
        {
            IsDone = !IsDone;
            if(IsDone)
            {
                currentIntProgress = maxIntProgress;
            }
            else
            {
                currentIntProgress = 0;
            }

            this.RaisePropertyChanged(nameof(IsDone));
            this.RaisePropertyChanged(nameof(CurrentIntProgress));
            this.RaisePropertyChanged(nameof(MaxIntProgress));

            ParentComplexTask?.CalculateProgress();

            ToDoListService.SaveChangesToDatabase(this);

        }

        public void ChangeProgressForIntTask(string progressText)
        {
            if (!int.TryParse(progressText, out int progress))
            {
                return;
            }

            CurrentIntProgress = progress;

            if (CurrentIntProgress == MaxIntProgress)
            {
                IsDone = true;
            }
            else
            {
                IsDone = false; // to ensure that you can revert changes
            }

            this.RaisePropertyChanged(nameof(IsDone));
            this.RaisePropertyChanged(nameof(CurrentIntProgress));
            this.RaisePropertyChanged(nameof(MaxIntProgress));

            ParentComplexTask?.CalculateProgress();


            ToDoListService.SaveChangesToDatabase(this);

            ParentViewModel.ApplyFilterButton();
        }


        // for change progress popup it's used for binding visibility of popups when you change progress
        public bool IsComplexTask { get; set; }
        public bool IsIntTask { get; set; }
        public bool IsBoolTask { get; set; }

    }
}