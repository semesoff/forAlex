using System.Windows;
using ProjectManager.Models;

namespace ProjectManager.Views
{
    public partial class TeamMemberDialog : Window
    {
        public TeamMember TeamMember { get; private set; }

        public TeamMemberDialog(TeamMember member = null)
        {
            InitializeComponent();

            if (member != null)
            {
                TeamMember = member;
                NameTextBox.Text = member.Name;
                EmailTextBox.Text = member.Email;
                RoleTextBox.Text = member.Role;
                Title = "Редактирование исполнителя";
            }
            else
            {
                Title = "Новый исполнитель";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите ФИО исполнителя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TeamMember == null)
                TeamMember = new TeamMember();

            TeamMember.Name = NameTextBox.Text;
            TeamMember.Email = EmailTextBox.Text;
            TeamMember.Role = RoleTextBox.Text;

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
