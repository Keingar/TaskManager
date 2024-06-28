using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoList.DataModel;
using ToDoList.Services;

namespace testAvalonijaAbilitiesApp.DataModel
{
    public class ComplexTaskItem : TaskItem
    {
        public ComplexTaskItem()
        {
            SubTasks = new List<TaskItem>();


        }

        // Additional property
        public List<TaskItem> SubTasks { get; set; }

        public void CalculateProgress() 
        {
           // CalculateProgressHelperFunction(); 

         //   parentComplexTask?.CalculateProgress();
            if(ParentComplexTask != null)
            {
                ParentComplexTask.CalculateProgress();
            }
            else
            {
                CalculateProgressHelperFunction(); // calculate for this task and all subtasks

            }
        }

        public bool IsSubtaskOf(TaskItem task)
        {
            if (task is not ComplexTaskItem complexTask || complexTask == null || complexTask == this )
            {
                return false;
            }

            foreach (var subtask in SubTasks)
            {
                if (subtask == complexTask)
                {
                    return true;
                }

                if (subtask is ComplexTaskItem subComplexTask)
                {
                    if (subComplexTask.IsSubtaskOf(complexTask))
                    {
                        return true;
                    }
                }
            }

            // not found in subtasks
            return false;
        }


        private void CalculateProgressHelperFunction()
        {
            if (SubTasks.Count == 0)
            {
                MaxIntProgress = 1; // otherwise progress bar will 100% filled
                CurrentIntProgress = 0;
                IsDone = false;
                return;
            }

            MaxIntProgress = 0;
            CurrentIntProgress = 0;

            foreach (TaskItem subtask in SubTasks)
            {
                if (subtask is ComplexTaskItem subComplexTask)
                {
                    subComplexTask.CalculateProgressHelperFunction();

                    MaxIntProgress += subComplexTask.MaxIntProgress;
                    CurrentIntProgress += subComplexTask.CurrentIntProgress;
                }
                else
                {
                    MaxIntProgress++;

                    if (subtask.IsDone)
                    {
                        CurrentIntProgress++;
                    }
                }
            }

            if (MaxIntProgress == CurrentIntProgress)
            {
                IsDone = true;
            }
            else
            {
                IsDone = false;
            }

            this.RaisePropertyChanged(nameof(IsDone));
            this.RaisePropertyChanged(nameof(CurrentIntProgress));
            this.RaisePropertyChanged(nameof(MaxIntProgress));

            ToDoListService.SaveChangesToDatabase(this);

        }


    }
}
