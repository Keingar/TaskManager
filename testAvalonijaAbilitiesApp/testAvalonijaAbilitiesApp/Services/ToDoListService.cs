using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testAvalonijaAbilitiesApp.DataModel;
using ToDoList.DataModel;
using System.Security.Cryptography;
using System.IO;

namespace ToDoList.Services
{
    public static class ToDoListService
    {
        private static readonly string connectionString = "Server=localhost;Database=TaskManagerDB;Integrated Security=True;";
        static public IEnumerable<TaskItem> GetItems(user User)
        {
            List<TaskItem> tasks = new();

            using (SqlConnection connection = new(connectionString))
            {
                connection.Open();

                SqlCommand command = new("GetTasksByUserId", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@UserId", User.User_ID);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    TaskType taskType = TaskTypeHelper.GetTaskTypeFromString(Convert.ToString(reader["TaskType"]));

                    TaskItem task;
                    if (taskType == TaskType.Complex)
                    {
                        task = new ComplexTaskItem();
                    }
                    else
                    {
                        task = new TaskItem();
                    }

                    task.TaskID = Convert.ToInt32(reader["TaskID"]);

                    #pragma warning disable CS8601 // even if it's null it's fine because it's handled it's possible to create completely null task anyway
                    task.Title = Convert.ToString(reader["Title"]);
                    task.TaskDescription = Convert.ToString(reader["TaskDescription"]);
                    #pragma warning restore CS8601 

                    task.IsDone = Convert.ToBoolean(reader["IsDone"]);
                    task.TaskType = taskType;
                    task.DateOfCreation = Convert.ToDateTime(reader["DateOfCreation"]);
                    task.DueDate = reader["DueDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DueDate"]);
                    task.IsRoutine = Convert.ToBoolean(reader["IsRoutine"]);
                    task.FrequencyInDays = reader["FrequencyInDays"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["FrequencyInDays"]);
                    task.CurrentIntProgress = reader["CurrentIntProgress"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["CurrentIntProgress"]);
                    task.MaxIntProgress = reader["MaxIntProgress"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["MaxIntProgress"]);

                    task.UserOwner = User;

                    tasks.Add(task);
                }

                reader.Close();
            }

            SetUpSubtasksForComplexTasks(tasks);


            foreach (TaskItem task in tasks)
            {
                if (task is ComplexTaskItem complexTask)
                {
                    complexTask.CalculateProgress();
                }
            }

            return tasks;
        }

        static private void SetUpSubtasksForComplexTasks(List<TaskItem> tasks) // used only once to initiolize all complex tasks relationship
        {
            using SqlConnection connection = new(connectionString);
            connection.Open();

            string sqlQuery = "SELECT * FROM TaskRelationship";
            SqlCommand command = new(sqlQuery, connection);

            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int firstTaskID = Convert.ToInt32(reader["FirstTaskID"]);
                int secondTaskID = Convert.ToInt32(reader["SecondTaskID"]);

                TaskItem firstTask = FindTaskByID(tasks, firstTaskID);
                TaskItem secondTask = FindTaskByID(tasks, secondTaskID);

                if (firstTask is ComplexTaskItem complexTask && secondTask != null)
                {
                    if (complexTask.TaskID == secondTask.TaskID)
                    {
                        throw new ArgumentException("Complex task cannot be a subtask of itself");
                    }

                    complexTask.SubTasks.Add(secondTask);

                    secondTask.ParentComplexTask = complexTask;
                }
            }

            reader.Close();
        }

        static public int AddItem(TaskItem newItem)
        {

            using SqlConnection connection = new(connectionString);
            string taskTypeString = TaskTypeHelper.GetStringFromTaskType(newItem.TaskType);

            connection.Open();

            SqlCommand command = new("InsertTask", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@Title", newItem.Title);
            command.Parameters.AddWithValue("@TaskDescription", newItem.TaskDescription);
            command.Parameters.AddWithValue("@TaskType", taskTypeString);
            command.Parameters.AddWithValue("@DueDate", newItem.DueDate ?? (object)DBNull.Value); // Handle nullable values
            command.Parameters.AddWithValue("@IsRoutine", newItem.IsRoutine);
            command.Parameters.AddWithValue("@FrequencyInDays", newItem.FrequencyInDays ?? (object)DBNull.Value); // Handle nullable values
            command.Parameters.AddWithValue("@MaxIntProgress", newItem.MaxIntProgress ?? (object)DBNull.Value); // Handle nullable values
            command.Parameters.AddWithValue("@UserId", newItem.UserOwner.User_ID);

            SqlParameter taskIdParam = new("@TaskID", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(taskIdParam);

            command.ExecuteNonQuery();

            if (taskIdParam.Value == DBNull.Value)
            {
                throw new InvalidOperationException("Task ID was not returned. The insert operation might have failed.");
            }

            int taskID = Convert.ToInt32(taskIdParam.Value);

            return taskID;
        }

        static private TaskItem FindTaskByID(List<TaskItem> tasks, int taskID)
        {


            #pragma warning disable CS8603 // here null refernce is inpossible becuase this function is using ID from the same list and function was created only for convinient way to get task
            return tasks.FirstOrDefault(task => task.TaskID == taskID);
            #pragma warning restore CS8603 
        }

        static public void UpdateTaskInfo(int taskId, string newTaskTitle, string newTaskDescription, DateTime? newDueDate, int? newMaxProgress)
        {

            using SqlConnection connection = new(connectionString);
            using SqlCommand command = new("UpdateTaskInfo", connection);
            command.CommandType = CommandType.StoredProcedure;

            // Adding parameters
            command.Parameters.AddWithValue("@TaskID", taskId);
            command.Parameters.AddWithValue("@NewTaskTitle", newTaskTitle);
            command.Parameters.AddWithValue("@NewTaskDescription", newTaskDescription);
            command.Parameters.AddWithValue("@NewDueDate", newDueDate);
            command.Parameters.AddWithValue("@NewMaxProgress", newMaxProgress);

            connection.Open();
            command.ExecuteNonQuery();
        }

        static public int SignUpNewUser(string userName, string password)
        {
            string passwordHash = HashPassword(password);

            using SqlConnection connection = new(connectionString);
            connection.Open();

            SqlCommand command = new("CreateUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@username", userName);
            command.Parameters.AddWithValue("@password_hash", passwordHash);

            SqlParameter statusParam = new("@status", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(statusParam);


            command.ExecuteNonQuery();
            int status = (int)statusParam.Value;

            return status; // -1 user exists, otherwise 1 
        }

        static public int SignInUser(string userName, string password)
        {
            string passwordHash = HashPassword(password);

            using SqlConnection connection = new(connectionString);
            connection.Open();

            SqlCommand command = new("LoginUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@username", userName);
            command.Parameters.AddWithValue("@password_hash", passwordHash);

            SqlParameter loginResultParam = new("@login_result", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(loginResultParam);

            command.ExecuteNonQuery();
            int loginResult = (int)loginResultParam.Value;

            return loginResult; // 0 log in succeeded, 1  error
        }

        static private string HashPassword(string password)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }

        public static user GetUser(string username)
        {
            using SqlConnection connection = new(connectionString);
            connection.Open();

            SqlCommand command = new("GetUserInformation", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@username", username);

            SqlParameter userIdParam = new("@user_id", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(userIdParam);

            SqlParameter createdAtParam = new("@created_at", SqlDbType.DateTime)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(createdAtParam);

            command.ExecuteNonQuery();

            int userId = (int)userIdParam.Value;
            DateTime createdAt = (DateTime)createdAtParam.Value;

            return new user(username, userId, createdAt);
        }

        static public void SaveChangesToDatabase(TaskItem itemToSave)
        {

            using SqlConnection connection = new(connectionString);
            connection.Open();

            SqlCommand command = new("UpdateTaskProgress", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@TaskID", itemToSave.TaskID);
            command.Parameters.AddWithValue("@IsDone", (object)itemToSave.IsDone ?? DBNull.Value);

            #pragma warning disable CS8600 // warning is disabled because it's actually can be null inside stored procedure
            command.Parameters.AddWithValue("@CurrentIntProgress", (object)itemToSave.CurrentIntProgress ?? DBNull.Value);
            command.Parameters.AddWithValue("@MaxIntProgress", (object)itemToSave.MaxIntProgress ?? DBNull.Value);
            #pragma warning restore CS8600 

            command.ExecuteNonQuery();
        }
        static public void DeleteItem(int taskID)
        {
            string connectionString = "Server=localhost;Database=TaskManagerDB;User Id=testLog;Password=123;";

            using SqlConnection connection = new(connectionString);
            connection.Open();

            SqlCommand command = new("DeleteTask", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@TaskID", taskID);

            command.ExecuteNonQuery();
        }

        static public void CreateTaskRelationshipInSQL(int firstTaskID, int secondTaskID)
        {
            using SqlConnection connection = new(connectionString);
            connection.Open();

            SqlCommand command = new("CreateTaskRelationship", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@FirstTaskID", firstTaskID);
            command.Parameters.AddWithValue("@SecondTaskID", secondTaskID);

            command.ExecuteNonQuery();
        }

        public static void RemoveTaskRelationship(int firstTaskID, int secondTaskID)
        {
            using SqlConnection connection = new(connectionString);
            connection.Open();

            SqlCommand command = new("RemoveTaskRelationship", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@FirstTaskID", firstTaskID);
            command.Parameters.AddWithValue("@SecondTaskID", secondTaskID);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                // Handle specific SQL errors
                if (ex.Number == 50000) // Custom error number for RAISERROR
                {
                    throw new InvalidOperationException(ex.Message);
                }
                else
                {
                    throw; // Rethrow other SQL exceptions
                }
            }
        }


        public static void RestoreDatabase(string databaseName)
        {
            // Specify the absolute path to your .bak file

            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string backupFileName = "TaskManagerDB.bak";
            string backupFilePath = Path.Combine(currentDirectory, backupFileName);

            string newConnectionString = "Server=localhost;Database=Master;Integrated Security=True;";

            using SqlConnection connection = new SqlConnection(newConnectionString);
            connection.Open();

            string restoreQuery = $@"
                USE master;
                Create DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                RESTORE DATABASE [{databaseName}]
                FROM DISK = N'{backupFilePath}'
                WITH MOVE '{databaseName}' TO N'E:\\testAvaloniaAbilitiesApp\\testAvalonijaAbilitiesApp\\testAvalonijaAbilitiesApp\\bin\\Debug\\net6.0\\{databaseName}.mdf',
                MOVE '{databaseName}_Log' TO N'E:\\testAvaloniaAbilitiesApp\\testAvalonijaAbilitiesApp\\testAvalonijaAbilitiesApp\\bin\\Debug\\net6.0\\{databaseName}_Log.ldf',
                REPLACE;
                ALTER DATABASE [{databaseName}] SET MULTI_USER;";

            using SqlCommand command = new SqlCommand(restoreQuery, connection);
            command.ExecuteNonQuery();
        }



        public static bool DatabaseExists()
        {
            string newConnectionString = "Server=localhost;Database=Master;Integrated Security=True;";


            using SqlConnection connection = new(newConnectionString);
            connection.Open();
            using SqlCommand command = new($"SELECT database_id FROM sys.databases WHERE Name = 'TaskManagerDB'", connection);
            return (command.ExecuteScalar() != null);
        }
    }
}
