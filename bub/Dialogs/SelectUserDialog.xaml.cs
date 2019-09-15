using bub.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using bub.Extensions;

namespace bub.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectUserDialogxaml.xaml
    /// </summary>
    public partial class SelectUserDialog : Window
    {
        public static readonly DependencyProperty UsersProperty =
            DependencyProperty.Register("Users", typeof(List<User>), typeof(SelectUserDialog));

        public List<User> Users
        {
            get { return (List<User>)GetValue(UsersProperty); }
            set { SetValue(UsersProperty, value); }
        }

        public SelectUserDialog(List<User> users)
        {
            Users = users;
            InitializeComponent();
            cboUsers.Focus();
            cboUsers.SelectedIndex = 0;
        }

        public bool IsNewUser
        {
            get; private set;
        }

        public User User
        {
            get; private set;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (cboUsers.SelectedItem is User)
            {
                IsNewUser = false;
                User = (User)cboUsers.SelectedItem;
                DialogResult = true;
            }
            else
            {
                IsNewUser = true;
                User = new User(cboUsers.Text, 0);
                DialogResult = !User.Name.IsNullOrEmpty();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
