using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinalProjectApplication
{
    public partial class Login : Form
    {

        public Login(LoginModel model)
        {
            InitializeComponent();
            MaximizeBox = false;

            uiUsernameTextBox.MaxLength = 100;

            uiPasswordTextBox.PasswordChar = '*';
            uiPasswordTextBox.MaxLength = 100;
        }

        public void addLoginButtonPressEvent(LoginController controller)
        {
            uiLoginButton.Click += new EventHandler(controller.ButtonPress);
        }

        public string Username { get { return uiUsernameTextBox.Text; } }

        public string Password { get { return uiPasswordTextBox.Text; } }

        public string Store { get { return uiStoreNumberTextBox.Text; } }

        public void loginError()
        {
            MessageBox.Show("StoreId, Username or Password was incorrect", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void connectionError()
        {
            MessageBox.Show("Error connecting to database", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
