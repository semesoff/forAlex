using System;

namespace ProjectManager.Models
{
    public class Task : BaseEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public int ProjectId { get; set; }
        public int? AssignedToId { get; set; }
        public TeamMember AssignedTo { get; set; }

        // Метод для получения отображаемого текста статуса
        public string GetStatusDisplay()
        {
            return Status switch
            {
                TaskStatus.Pending => "Ожидает",
                TaskStatus.InProgress => "В процессе",
                TaskStatus.Completed => "Завершено",
                _ => Status.ToString()
            };
        }

        // Метод для получения отображаемого текста приоритета
        public string GetPriorityDisplay()
        {
            return Priority switch
            {
                TaskPriority.Low => "Низкий",
                TaskPriority.Medium => "Средний",
                TaskPriority.High => "Высокий",
                _ => Priority.ToString()
            };
        }

        public string StatusDisplay => GetStatusDisplay();
        public string PriorityDisplay => GetPriorityDisplay();
    }

    public enum TaskStatus
    {
        Pending,
        InProgress,
        Completed
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High
    }
}
