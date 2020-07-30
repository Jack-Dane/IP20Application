using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

namespace FinalProjectApplication
{
    public class LoginController
    {

        Login mView;
        LoginModel mModel;

        public LoginController(Login view, LoginModel model)
        {
            mView = view;
            mModel = model;

            mView.addLoginButtonPressEvent(this);
        }

        public void ButtonPress(object sender, EventArgs e)
        {
            string username = mView.Username;
            string password = mView.Password;
            string store = mView.Store;
            int errorCode = mModel.LoginToSystem(username, password, store);

            switch (errorCode){
                case 1:
                    //error message 1 == success
                    Manager indexModel = mModel.GetManager;
                    Thread t = new Thread(() => OpenIndexPage(indexModel));
                    t.Start();
                    mView.Close();
                    break;
                case 0:
                    //error message 0 == login error eg: username or password incorrect
                    mView.loginError();
                    break;
                case -1:
                    //error message -1 == database connection error
                    mView.connectionError();
                    break;
            }
        }
        private void OpenIndexPage(Manager indexModel)
        {
            index form = new index(indexModel);
            IndexController indexController = new IndexController(form, indexModel);

            Application.Run(form);
        }
    }
}
