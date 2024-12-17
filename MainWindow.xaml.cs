using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ProjectManager.Models;
using ProjectManager.Data;
using ProjectManager.Views;
using Microsoft.Win32;
using System.IO;
using System.Globalization;
using CsvHelper;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace ProjectManager
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseContext _dbContext;
        private Project _selectedProject;

        public MainWindow()
        {
            InitializeComponent();
            _dbContext = new DatabaseContext();
            LoadProjects();
            InitializeSortComboBox();
        }

        private void LoadProjects()
        {
            ProjectsList.ItemsSource = _dbContext.GetAllProjects();
        }

        private void InitializeSortComboBox()
        {
            SortComboBox.SelectedIndex = 0;
        }

        private void CreateProject_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ProjectDialog();
            if (dialog.ShowDialog() == true)
            {
                _dbContext.AddProject(dialog.Project);
                LoadProjects();
            }
        }

        private void EditProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Project project)
            {
                var dialog = new ProjectDialog(project);
                if (dialog.ShowDialog() == true)
                {
                    _dbContext.UpdateProject(dialog.Project);
                    LoadProjects();
                }
            }
        }

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Project project)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить проект '{project.Name}'?\nВсе задачи проекта также будут удалены.",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _dbContext.DeleteProject(project.Id);
                    LoadProjects();
                    if (_selectedProject?.Id == project.Id)
                    {
                        _selectedProject = null;
                        ProjectNameText.Text = "Выберите проект";
                        TasksGrid.ItemsSource = null;
                    }
                }
            }
        }

        private void ProjectsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedProject = ProjectsList.SelectedItem as Project;
            if (_selectedProject != null)
            {
                ProjectNameText.Text = _selectedProject.Name;
                LoadTasks();
            }
            else
            {
                ProjectNameText.Text = "Выберите проект";
                TasksGrid.ItemsSource = null;
            }
        }

        private void LoadTasks()
        {
            if (_selectedProject != null)
            {
                var tasks = _dbContext.GetTasksByProject(_selectedProject.Id);
                TasksGrid.ItemsSource = tasks;
            }
        }

        private void CreateTask_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProject == null)
            {
                MessageBox.Show("Пожалуйста, выберите проект", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new TaskDialog(_dbContext.GetAllTeamMembers());
            if (dialog.ShowDialog() == true)
            {
                dialog.Task.ProjectId = _selectedProject.Id;
                _dbContext.AddTask(dialog.Task);
                LoadTasks();
            }
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Task task)
            {
                var dialog = new TaskDialog(_dbContext.GetAllTeamMembers(), task);
                if (dialog.ShowDialog() == true)
                {
                    dialog.Task.ProjectId = _selectedProject.Id;
                    _dbContext.UpdateTask(dialog.Task);
                    LoadTasks();
                }
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Task task)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить задачу '{task.Title}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _dbContext.DeleteTask(task.Id);
                    LoadTasks();
                }
            }
        }

        private void ManageTeamMembers_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TeamMembersListDialog(_dbContext);
            dialog.ShowDialog();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TasksGrid.ItemsSource == null) return;

            var tasks = TasksGrid.ItemsSource.Cast<Task>().ToList();
            
            if (SortComboBox.SelectedIndex == 0) // По приоритету
            {
                TasksGrid.ItemsSource = tasks.OrderByDescending(t => t.Priority).ToList();
            }
            else // По сроку выполнения
            {
                TasksGrid.ItemsSource = tasks.OrderBy(t => t.DueDate).ToList();
            }
        }

        private void ExportToCsv_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProject == null || TasksGrid.ItemsSource == null)
            {
                MessageBox.Show("Нет данных для экспорта", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"{_selectedProject.Name}_tasks.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using var writer = new StreamWriter(saveFileDialog.FileName);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.WriteRecords(TasksGrid.ItemsSource);
                MessageBox.Show("Экспорт в CSV выполнен успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExportToPdf_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProject == null || TasksGrid.ItemsSource == null)
            {
                MessageBox.Show("Нет данных для экспорта", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"{_selectedProject.Name}_tasks.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using var writer = new PdfWriter(saveFileDialog.FileName);
                using var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                document.Add(new Paragraph($"Проект: {_selectedProject.Name}"));
                document.Add(new Paragraph($"Отчет создан: {DateTime.Now:g}"));
                document.Add(new Paragraph("\n"));

                var table = new Table(5);
                table.AddCell("Название");
                table.AddCell("Срок");
                table.AddCell("Статус");
                table.AddCell("Приоритет");
                table.AddCell("Исполнитель");

                foreach (Task task in TasksGrid.ItemsSource)
                {
                    table.AddCell(task.Title);
                    table.AddCell(task.DueDate.ToString("d"));
                    table.AddCell(task.Status.ToString());
                    table.AddCell(task.Priority.ToString());
                    table.AddCell(task.AssignedTo?.Name ?? "Не назначен");
                }

                document.Add(table);
                MessageBox.Show("Экспорт в PDF выполнен успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
