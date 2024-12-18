using System;
using System.Collections.Generic;

namespace ProjectManager.Models
{
    public class Project : BaseEntity
    {
        private string name;
        private string description;

        public string Name
        {
            get => name;
            set => name = value;
        }

        public string Description
        {
            get => description;
            set => description = value;
        }

        public List<Task> Tasks { get; set; } = new List<Task>();
    }
}
