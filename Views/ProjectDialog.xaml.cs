using System;
using System.Windows;
using ProjectManager.Models;

namespace ProjectManager.Views
{
    public partial class ProjectDialog : Window
    {
        public Project Project { get; private set; }

        public ProjectDialog(Project project = null)
        {
            InitializeComponent();
            
            if (project != null)
            {
                Project = project;
                NameTextBox.Text = project.Name;
                DescriptionTextBox.Text = project.Description;
                StartDatePicker.SelectedDate = project.StartDate;
                EndDatePicker.SelectedDate = project.EndDate;
                Title = "Редактирование проекта";
            }
            else
            {
                StartDatePicker.SelectedDate = DateTime.Now;
                EndDatePicker.SelectedDate = DateTime.Now.AddMonths(1);
                Title = "Новый проект";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите название проекта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите даты начала и окончания проекта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (StartDatePicker.SelectedDate > EndDatePicker.SelectedDate)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Project == null)
                Project = new Project();

            Project.Name = NameTextBox.Text;
            Project.Description = DescriptionTextBox.Text;
            Project.StartDate = StartDatePicker.SelectedDate.Value;
            Project.EndDate = EndDatePicker.SelectedDate.Value;

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
