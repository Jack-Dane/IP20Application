using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinalProjectApplication
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            LoginModel loginModel = new LoginModel();
            Login form1 = new Login(loginModel);
            LoginController loginController = new LoginController(form1, loginModel);
            Application.Run(form1);
        }
    }
}
