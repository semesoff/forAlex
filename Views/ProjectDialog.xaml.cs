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
                Title = "Редактирование проекта";
            }
            else
            {
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

            if (Project == null)
                Project = new Project();

            Project.Name = NameTextBox.Text;
            Project.Description = DescriptionTextBox.Text;

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
