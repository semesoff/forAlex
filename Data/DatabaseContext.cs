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
            
            // Получаем Id только что созданного проекта
            command.CommandText = "SELECT last_insert_rowid()";
            project.Id = Convert.ToInt32(command.ExecuteScalar());
        }

        public void UpdateProject(Project project)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Projects 
                SET Name = @name, Description = @description, StartDate = @startDate, EndDate = @endDate
                WHERE Id = @id";
            
            command.Parameters.AddWithValue("@id", project.Id);
            command.Parameters.AddWithValue("@name", project.Name);
            command.Parameters.AddWithValue("@description", project.Description);
            command.Parameters.AddWithValue("@startDate", project.StartDate.ToString("s"));
            command.Parameters.AddWithValue("@endDate", project.EndDate.ToString("s"));
            
            command.ExecuteNonQuery();
        }

        public void DeleteProject(int projectId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            // Сначала удаляем все задачи проекта
            command.CommandText = "DELETE FROM Tasks WHERE ProjectId = @projectId";
            command.Parameters.AddWithValue("@projectId", projectId);
            command.ExecuteNonQuery();

            // Затем удаляем сам проект
            command.CommandText = "DELETE FROM Projects WHERE Id = @projectId";
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
            
            command.CommandText = "SELECT last_insert_rowid()";
            task.Id = Convert.ToInt32(command.ExecuteScalar());
        }

        public void UpdateTask(Task task)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Tasks 
                SET Title = @title, Description = @description, DueDate = @dueDate, 
                    Status = @status, Priority = @priority, AssignedToId = @assignedToId
                WHERE Id = @id";
            
            command.Parameters.AddWithValue("@id", task.Id);
            command.Parameters.AddWithValue("@title", task.Title);
            command.Parameters.AddWithValue("@description", task.Description);
            command.Parameters.AddWithValue("@dueDate", task.DueDate.ToString("s"));
            command.Parameters.AddWithValue("@status", (int)task.Status);
            command.Parameters.AddWithValue("@priority", (int)task.Priority);
            command.Parameters.AddWithValue("@assignedToId", task.AssignedToId.HasValue ? task.AssignedToId : DBNull.Value);
            
            command.ExecuteNonQuery();
        }

        public void DeleteTask(int taskId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Tasks WHERE Id = @taskId";
            command.Parameters.AddWithValue("@taskId", taskId);
            command.ExecuteNonQuery();
        }

        // Методы для работы с исполнителями
        public void AddTeamMember(TeamMember member)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO TeamMembers (Name, Email, Role)
                VALUES (@name, @email, @role)";
            
            command.Parameters.AddWithValue("@name", member.Name);
            command.Parameters.AddWithValue("@email", member.Email);
            command.Parameters.AddWithValue("@role", member.Role);
            
            command.ExecuteNonQuery();
            
            command.CommandText = "SELECT last_insert_rowid()";
            member.Id = Convert.ToInt32(command.ExecuteScalar());
        }

        public void UpdateTeamMember(TeamMember member)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE TeamMembers 
                SET Name = @name, Email = @email, Role = @role
                WHERE Id = @id";
            
            command.Parameters.AddWithValue("@id", member.Id);
            command.Parameters.AddWithValue("@name", member.Name);
            command.Parameters.AddWithValue("@email", member.Email);
            command.Parameters.AddWithValue("@role", member.Role);
            
            command.ExecuteNonQuery();
        }

        public void DeleteTeamMember(int memberId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            // Сначала обновляем задачи, где этот исполнитель назначен
            command.CommandText = "UPDATE Tasks SET AssignedToId = NULL WHERE AssignedToId = @memberId";
            command.Parameters.AddWithValue("@memberId", memberId);
            command.ExecuteNonQuery();

            // Затем удаляем самого исполнителя
            command.CommandText = "DELETE FROM TeamMembers WHERE Id = @memberId";
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

        // Получение всех исполнителей
        public List<TeamMember> GetAllTeamMembers()
        {
            var members = new List<TeamMember>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM TeamMembers";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                members.Add(new TeamMember
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Role = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }

            return members;
        }

        // Получение задач по проекту
        public List<Task> GetTasksByProject(int projectId)
        {
            var tasks = new List<Task>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT t.*, m.Name as AssigneeName, m.Email as AssigneeEmail, m.Role as AssigneeRole
                FROM Tasks t
                LEFT JOIN TeamMembers m ON t.AssignedToId = m.Id
                WHERE t.ProjectId = @projectId";
            command.Parameters.AddWithValue("@projectId", projectId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var task = new Task
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.GetString(2),
                    DueDate = DateTime.Parse(reader.GetString(3)),
                    Status = (TaskStatus)reader.GetInt32(4),
                    Priority = (TaskPriority)reader.GetInt32(5),
                    ProjectId = reader.GetInt32(6),
                    AssignedToId = reader.IsDBNull(7) ? null : reader.GetInt32(7)
                };

                if (!reader.IsDBNull(8)) // Если есть назначенный исполнитель
                {
                    task.AssignedTo = new TeamMember
                    {
                        Id = task.AssignedToId.Value,
                        Name = reader.GetString(8),
                        Email = reader.IsDBNull(9) ? null : reader.GetString(9),
                        Role = reader.IsDBNull(10) ? null : reader.GetString(10)
                    };
                }

                tasks.Add(task);
            }

            return tasks;
        }
    }
}
