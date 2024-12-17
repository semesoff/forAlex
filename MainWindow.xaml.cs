using System;
using System.Collections.Generic;
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
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.IO.Font.Constants;

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
                try
                {
                    using var writer = new PdfWriter(saveFileDialog.FileName);
                    using var pdf = new PdfDocument(writer);
                    using var document = new Document(pdf);

                    PdfFont font = PdfFontFactory.CreateFont("c:/windows/fonts/arial.ttf", PdfEncodings.IDENTITY_H);
                    
                    // Добавляем заголовок
                    var title = new Paragraph($"Отчет по проекту: {_selectedProject?.Name ?? "Неизвестный проект"}")
                        .SetFont(font)
                        .SetFontSize(16)
                        .SetBold();
                    document.Add(title);

                    // Добавляем дату создания отчета
                    document.Add(new Paragraph($"Дата создания отчета: {DateTime.Now:g}")
                        .SetFont(font)
                        .SetFontSize(10)
                        .SetItalic());

                    document.Add(new Paragraph("\n"));

                    // Создаем таблицу с фиксированной шириной столбцов
                    var table = new Table(5);
                    table.SetWidth(500); // Фиксированная ширина таблицы

                    // Добавляем заголовки
                    string[] headers = { "Название", "Срок", "Статус", "Приоритет", "Исполнитель" };
                    foreach (var header in headers)
                    {
                        var cell = new Cell()
                            .Add(new Paragraph(header).SetFont(font))
                            .SetBold()
                            .SetBackgroundColor(new iText.Kernel.Colors.DeviceRgb(240, 240, 240));
                        table.AddCell(cell);
                    }

                    // Проверка наличия задач
                    if (TasksGrid.ItemsSource is IEnumerable<Task> tasks && tasks.Any())
                    {
                        // Добавляем данные
                        foreach (var task in tasks)
                        {
                            table.AddCell(new Cell().Add(new Paragraph(task.Title ?? "").SetFont(font).SetFontSize(12)));
                            table.AddCell(new Cell().Add(new Paragraph(task.DueDate.ToString("dd.MM.yyyy") ?? "").SetFont(font).SetFontSize(12)));
                            table.AddCell(new Cell().Add(new Paragraph(task.GetStatusDisplay() ?? "").SetFont(font).SetFontSize(12)));
                            table.AddCell(new Cell().Add(new Paragraph(task.GetPriorityDisplay() ?? "").SetFont(font).SetFontSize(12)));
                            table.AddCell(new Cell().Add(new Paragraph(task.AssignedTo?.Name ?? "Не назначен").SetFont(font).SetFontSize(12)));
                        }
                    }
                    else
                    {
                        document.Add(new Paragraph("Нет задач для отображения").SetFont(font).SetFontSize(12));
                    }
                    
                    document.Add(table);
                    document.Close();

                    MessageBox.Show("Экспорт в PDF выполнен успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Открываем файл после создания
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = saveFileDialog.FileName,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании PDF: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Console.WriteLine($"PDF Export Error: {ex}"); // Добавление логирования ошибки
                }
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
                try
                {
                    using var writer = new StreamWriter(saveFileDialog.FileName, false, System.Text.Encoding.UTF8);
                    using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                    csv.WriteRecords((IEnumerable<Task>)TasksGrid.ItemsSource);

                    MessageBox.Show("Экспорт в CSV выполнен успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании CSV: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
