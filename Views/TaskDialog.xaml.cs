using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ProjectManager.Models;

namespace ProjectManager.Views
{
    public partial class TaskDialog : Window
    {
        public Task Task { get; private set; }
        private readonly List<TeamMember> _teamMembers;

        public TaskDialog(List<TeamMember> teamMembers, Task task = null)
        {
            InitializeComponent();
            _teamMembers = teamMembers;

            InitializeComboBoxes();

            if (task != null)
            {
                Task = task;
                TitleTextBox.Text = task.Title;
                DescriptionTextBox.Text = task.Description;
                DueDatePicker.SelectedDate = task.DueDate;
                StatusComboBox.SelectedItem = task.Status;
                PriorityComboBox.SelectedItem = task.Priority;
                AssigneeComboBox.SelectedItem = _teamMembers.FirstOrDefault(m => m.Id == task.AssignedToId);
                Title = "Редактирование задачи";
            }
            else
            {
                DueDatePicker.SelectedDate = DateTime.Now.AddDays(7);
                StatusComboBox.SelectedIndex = 0;
                PriorityComboBox.SelectedIndex = 1;
                Title = "Новая задача";
            }
        }

        private void InitializeComboBoxes()
        {
            StatusComboBox.ItemsSource = Enum.GetValues(typeof(TaskStatus));
            PriorityComboBox.ItemsSource = Enum.GetValues(typeof(TaskPriority));
            AssigneeComboBox.ItemsSource = _teamMembers;
            AssigneeComboBox.DisplayMemberPath = "Name";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите название задачи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DueDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите срок выполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Task == null)
                Task = new Task();

            Task.Title = TitleTextBox.Text;
            Task.Description = DescriptionTextBox.Text;
            Task.DueDate = DueDatePicker.SelectedDate.Value;
            Task.Status = (TaskStatus)StatusComboBox.SelectedItem;
            Task.Priority = (TaskPriority)PriorityComboBox.SelectedItem;
            
            var selectedMember = AssigneeComboBox.SelectedItem as TeamMember;
            Task.AssignedToId = selectedMember?.Id;
            Task.AssignedTo = selectedMember;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
