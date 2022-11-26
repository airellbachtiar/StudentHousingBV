using StudentHousingBV.Classes.House;
using StudentHousingBV.Classes.Report;
using StudentHousingBV.Classes.Scheduler;
using StudentHousingBV.Classes.Tracker;
using StudentHousingBV.Classes.User;
using System;
using System.Windows.Forms;

namespace StudentHousingBV
{
    public partial class LoginForm : Form
    {
        private House house;
        private bool loginBool = false;
        private Scheduler scheduler;
        private Report report;

        public LoginForm(House house, Scheduler scheduler, Report report)
        {
            InitializeComponent();
            this.house = house;
            this.scheduler = scheduler;
            this.report = report;
            txtPassword.PasswordChar = '*';

            //Adding every user to the combobox.
            foreach (User u in this.house.users)
            {
                cboxUsername.Items.Add(u.displayName);
            }

            cboxUsername.SelectedIndex = 0;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = cboxUsername.Text;
            string password = txtPassword.Text;

            login(username, password);
        }

        public void login(string username, string password)
        {
            User user = null;
            foreach (User u in house.users)
            {
                if (u.CheckLogin(username, password) == true)
                {
                    // go to the right form
                    user = u;
                    loginBool = true;
                    break;
                }
            }

            if (loginBool)
            {
                userForm(user);
            }
            else
            {
                MessageBox.Show("Wrong password");
                txtPassword.Clear();
            }
        }

        public void userForm(User user)
        {
            this.Hide();
            Form form1;

            // If it's not a student, then it's a landlord.
            if (user.isStudent())
            {
                form1 = new TenantForm(this.house, (Student)user, scheduler, report);
                form1.Text = $"Tenant Page - {user.displayName}";
            }
            else
            {
                form1 = new AdminForm(this.house, (Admin)user, scheduler, report);
            }
            form1.Closed += (s, args) => this.Close();
            form1.Show();
        }
    }
}
