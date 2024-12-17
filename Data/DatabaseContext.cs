using System;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using ProjectManager.Models;

namespace ProjectManager.Data
{
    public class DatabaseContext
    {
        private readonly string _connectionString;

        public DatabaseContext()
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "projectmanager.db");
            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            
            // Создание таблицы Projects
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Projects (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    StartDate TEXT NOT NULL,
                    EndDate TEXT NOT NULL
                )";
            command.ExecuteNonQuery();

            // Создание таблицы TeamMembers
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS TeamMembers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Email TEXT,
                    Role TEXT
                )";
            command.ExecuteNonQuery();

            // Создание таблицы Tasks
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Tasks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    DueDate TEXT NOT NULL,
                    Status INTEGER NOT NULL,
                    Priority INTEGER NOT NULL,
                    ProjectId INTEGER NOT NULL,
                    AssignedToId INTEGER,
                    FOREIGN KEY(ProjectId) REFERENCES Projects(Id),
                    FOREIGN KEY(AssignedToId) REFERENCES TeamMembers(Id)
                )";
            command.ExecuteNonQuery();
        }

        // Методы для работы с проектами
        public void AddProject(Project project)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Projects (Name, Description, StartDate, EndDate)
                VALUES (@name, @description, @startDate, @endDate)";
            
            command.Parameters.AddWithValue("@name", project.Name);
            command.Parameters.AddWithValue("@description", project.Description);
            command.Parameters.AddWithValue("@startDate", project.StartDate.ToString("s"));
            command.Parameters.AddWithValue("@endDate", project.EndDate.ToString("s"));
            
            command.ExecuteNonQuery();
        }

        // Методы для работы с задачами
        public void AddTask(Task task)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Tasks (Title, Description, DueDate, Status, Priority, ProjectId, AssignedToId)
                VALUES (@title, @description, @dueDate, @status, @priority, @projectId, @assignedToId)";
            
            command.Parameters.AddWithValue("@title", task.Title);
            command.Parameters.AddWithValue("@description", task.Description);
            command.Parameters.AddWithValue("@dueDate", task.DueDate.ToString("s"));
            command.Parameters.AddWithValue("@status", (int)task.Status);
            command.Parameters.AddWithValue("@priority", (int)task.Priority);
            command.Parameters.AddWithValue("@projectId", task.ProjectId);
            command.Parameters.AddWithValue("@assignedToId", task.AssignedToId.HasValue ? task.AssignedToId : DBNull.Value);
            
            command.ExecuteNonQuery();
        }

        // Получение всех проектов
        public List<Project> GetAllProjects()
        {
            var projects = new List<Project>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Projects";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                projects.Add(new Project
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    StartDate = DateTime.Parse(reader.GetString(3)),
                    EndDate = DateTime.Parse(reader.GetString(4))
                });
            }

            return projects;
        }

        // Получение задач по проекту
        public List<Task> GetTasksByProject(int projectId)
        {
            var tasks = new List<Task>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Tasks WHERE ProjectId = @projectId";
            command.Parameters.AddWithValue("@projectId", projectId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                tasks.Add(new Task
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.GetString(2),
                    DueDate = DateTime.Parse(reader.GetString(3)),
                    Status = (TaskStatus)reader.GetInt32(4),
                    Priority = (TaskPriority)reader.GetInt32(5),
                    ProjectId = reader.GetInt32(6),
                    AssignedToId = reader.IsDBNull(7) ? null : reader.GetInt32(7)
                });
            }

            return tasks;
        }
    }
}
