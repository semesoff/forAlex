using System.Windows;
using System.Windows.Controls;
using ProjectManager.Models;
using ProjectManager.Data;

namespace ProjectManager.Views
{
    public partial class TeamMembersListDialog : Window
    {
        private readonly DatabaseContext _dbContext;

        public TeamMembersListDialog(DatabaseContext dbContext)
        {
            InitializeComponent();
            _dbContext = dbContext;
            LoadTeamMembers();
        }

        private void LoadTeamMembers()
        {
            TeamMembersGrid.ItemsSource = _dbContext.GetAllTeamMembers();
        }

        private void AddTeamMember_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TeamMemberDialog();
            if (dialog.ShowDialog() == true)
            {
                _dbContext.AddTeamMember(dialog.TeamMember);
                LoadTeamMembers();
            }
        }

        private void EditTeamMember_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TeamMember member)
            {
                var dialog = new TeamMemberDialog(member);
                if (dialog.ShowDialog() == true)
                {
                    _dbContext.UpdateTeamMember(dialog.TeamMember);
                    LoadTeamMembers();
                }
            }
        }

        private void DeleteTeamMember_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TeamMember member)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить исполнителя '{member.Name}'?\n" +
                    "Исполнитель будет удален из всех назначенных задач.",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _dbContext.DeleteTeamMember(member.Id);
                    LoadTeamMembers();
                }
            }
        }
    }
}
