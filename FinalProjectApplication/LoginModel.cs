using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;

namespace FinalProjectApplication
{
    public class LoginModel : DatabaseConnection
    {
        public int LoginToSystem(string username, string password, string storeId)
        {

            if (base.ConnectToDatabase())
            {
                string query = "SELECT tblEmployee.EmployeeId, tblEmployee.Password, tblEmployee.Salt, tblStore.Name " +
                    "FROM tblEmployee JOIN tblEmployeeStore ON tblEmployee.EmployeeId = tblEmployeeStore.EmployeeId " +
                    "JOIN tblStore ON tblStore.StoreId = tblEmployeeStore.StoreId " +
                    "WHERE tblStore.StoreId = @storeId " +
                    "AND tblEmployee.Username = @username " +
                    "AND tblEmployee.JobTitle = 'Manager'";

                MySqlCommand cmd = new MySqlCommand(query, mConn);

                cmd.Parameters.Add(new MySqlParameter("username", username));
                cmd.Parameters.Add(new MySqlParameter("storeId", storeId));

                MySqlDataReader rdr = cmd.ExecuteReader();

                if (rdr != null)
                {
                    if (rdr.HasRows)
                    {
                        while (rdr.Read())
                        {
                            //now using hashing algorithm to check 
                            using (SHA256 sha256Hash = SHA256.Create())
                            {
                                // ComputeHash - returns byte array  
                                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password + rdr["Salt"]));

                                // Convert byte array to a string   
                                StringBuilder builder = new StringBuilder();
                                for (int i = 0; i < bytes.Length; i++)
                                {
                                    builder.Append(bytes[i].ToString("x2"));
                                }

                                string localEncryptedPassword = builder.ToString();

                                if (localEncryptedPassword.Equals(rdr["Password"]))
                                {
                                    string employeeId = rdr["EmployeeId"].ToString();

                                    List<Employee> employees = this.GetUserInformation(storeId);

                                    Manager manager = new Manager(storeId, employeeId, employees, this);
                                    GetManager = manager;
                                    return 1;
                                }
                                else
                                {
                                    return 0;
                                }
                            }
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return -1;
                }
            }

            return -1;
        }

        public Manager GetManager { get; set; }
    }
}
