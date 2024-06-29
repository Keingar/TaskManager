using System;

namespace ToDoList.DataModel
{
    public enum TaskType
    {
        Bool,
        Int,
        Complex
    }

    public static class TaskTypeHelper
    {
        public static TaskType GetTaskTypeFromString(string? taskTypeString) // created to get from sql
        {
            if (taskTypeString == null)
            {
                throw new ArgumentNullException(nameof(taskTypeString));
            }

            if (Enum.TryParse(taskTypeString, true, out TaskType taskType))
            {
                return taskType;
            }
            else
            {
                throw new ArgumentException($"Invalid task type string: {taskTypeString}");
            }
        }

        public static string GetStringFromTaskType(TaskType taskType) // created to save into sql
        {
            switch (taskType)
            {
                case TaskType.Bool:
                    return "bool";
                case TaskType.Int:
                    return "int";
                case TaskType.Complex:
                    return "complex";
                default:
                    break;
            }

            return taskType.ToString();
        }
    }

}