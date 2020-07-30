using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectApplication
{
    public class DatabaseConnection : IEmployeeDatabase, IManagerDatabase
    {
        protected readonly string server = "162.241.24.101";
        protected readonly string database = "gamerota_Game";
        protected readonly string uid = "gamerota_root";
        protected readonly string upassword = "NewSqlPassw0rd98";
        protected MySqlConnection mConn;

        protected bool ConnectToDatabase()
        {
            string connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + upassword + ";";

            mConn = new MySqlConnection(connectionString);

            try
            {
                mConn.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void CloseConnection()
        {
            if (mConn != null)
            {
                mConn.Close();
                mConn = null;
            }
        }

        public List<Employee> GetUserInformation(string storeId)
        {
            ConnectToDatabase();

            List<Employee> returnEmployees = new List<Employee>();

            string query = "SELECT tblEmployee.EmployeeId, tblEmployee.FirstName, tblEmployee.LastName, tblEmployee.ContractHours, tblEmployee.Email FROM tblEmployee " +
                "JOIN tblEmployeeStore ON tblEmployee.EmployeeId = tblEmployeeStore.EmployeeId " +
                "JOIN tblStore ON tblStore.StoreId = tblEmployeeStore.StoreId " +
                "WHERE tblStore.StoreId = @store";

            MySqlCommand cmd = new MySqlCommand(query, mConn);
            cmd.Parameters.Add(new MySqlParameter("store", storeId));

            MySqlDataReader rdr = cmd.ExecuteReader();

            Employee emp;

            while (rdr.Read())
            {
                string fName = rdr["FirstName"].ToString();
                string lName = rdr["LastName"].ToString();
                int cHours = (int)rdr["ContractHours"];
                int eId = (int)rdr["EmployeeId"];
                string email = rdr["Email"].ToString();

                emp = new Employee(fName, lName, cHours, eId, storeId, email, this);
                returnEmployees.Add(emp);
            }

            rdr.Close();
            CloseConnection();

            return returnEmployees;
        }

        public List<EmployeeWorkWeek> GetEmployeeWorkWeek(List<EmployeeWorkWeek> workHours, int employeeId, string storeId, DateTime startWeek, DateTime finishWeek)
        {
            ConnectToDatabase();

            string query = "SELECT * FROM tblWorkHours " +
                "WHERE EmployeeId = @eId AND Date >= @startDate AND Date <= @finishDate " +
                "ORDER BY Date";
            MySqlCommand cmd = new MySqlCommand(query, mConn);

            cmd.Parameters.Add(new MySqlParameter("eId", employeeId));
            cmd.Parameters.Add(new MySqlParameter("startDate", startWeek));
            cmd.Parameters.Add(new MySqlParameter("finishDate", finishWeek));

            MySqlDataReader rdr = cmd.ExecuteReader();

            EmployeeWorkWeek eW;
            while (rdr.Read())
            {
                eW = new EmployeeWorkWeek();
                eW.StoreId = rdr["StoreId"].ToString();
                eW.Date = DateTime.Parse(rdr["Date"].ToString());
                eW.Day = eW.Date.DayOfWeek.ToString();
                eW.StartTime = rdr["StartTime"].ToString();
                eW.FinishTime = rdr["FinishTime"].ToString();
                eW.RotaStartTime = rdr["RotaStartTime"].ToString();
                eW.RotaFinishTime = rdr["RotaFinishTime"].ToString();
                eW.LunchTime = rdr["BreakTime"].ToString();

                workHours[workHours.FindIndex(r => r.Date == eW.Date)] = eW;
            }

            rdr.Close();

            CloseConnection();

            return workHours;
        }

        public void DeleteFromWorkWeek(int employeeId, DateTime date)
        {
            ConnectToDatabase();

            string query = "DELETE FROM tblWorkHours WHERE EmployeeId = @eId AND Date = @date";

            MySqlCommand cmd = new MySqlCommand(query, mConn);
            cmd.Parameters.Add(new MySqlParameter("eId", employeeId));
            cmd.Parameters.Add(new MySqlParameter("date", date));

            cmd.ExecuteNonQuery();

            CloseConnection();
        }

        public bool InsertWorkHours(int employeeId, string storeId, string startTime, string finishTime, string rotaStartTime, string rotaFinishTime, string breakTime, DateTime date)
        {
            ConnectToDatabase();

            string query = "INSERT INTO tblWorkHours (WorkHoursId, EmployeeId, StoreId, StartTime, FinishTime, RotaStartTime, RotaFinishTime, BreakTime, Date)" +
                "VALUES (NULL, @eId, @storeId, @startTime, @finishTime, @rotaStartTime, @rotaFinishTime, @breakTime, @date)";

            MySqlCommand cmd = new MySqlCommand(query, mConn);

            cmd.Parameters.Add(new MySqlParameter("eId", employeeId));
            cmd.Parameters.Add(new MySqlParameter("storeId", storeId));
            cmd.Parameters.Add(new MySqlParameter("startTime", startTime));
            cmd.Parameters.Add(new MySqlParameter("finishTime", finishTime));
            cmd.Parameters.Add(new MySqlParameter("rotaStartTime", rotaStartTime));
            cmd.Parameters.Add(new MySqlParameter("rotaFinishTime", rotaFinishTime));
            cmd.Parameters.Add(new MySqlParameter("breakTime", breakTime));
            cmd.Parameters.Add(new MySqlParameter("date", date));

            if (cmd.ExecuteNonQuery() == 0)
            {
                CloseConnection();
                return false;
            }
            CloseConnection();
            return true;
        }

        public List<EmployeeHoliday> GetHolidays(int employeeId)
        {
            ConnectToDatabase();

            DateTime currentDate = DateTime.Now.AddDays(-7);

            List<EmployeeHoliday> returnHoliday = new List<EmployeeHoliday>();

            string query = "SELECT tblHoliday.HolidayId, tblHoliday.Date, tblHoliday.State, tblEmployee.FirstName, tblEmployee.LastName " +
                "FROM tblHoliday JOIN tblEmployee " +
                "ON tblHoliday.EmployeeId = tblEmployee.EmployeeId " +
                "WHERE tblHoliday.EmployeeId = @eId AND Date >= @currentDate " +
                "ORDER BY DATE";

            MySqlCommand cmd = new MySqlCommand(query, mConn);

            cmd.Parameters.Add(new MySqlParameter("eId", employeeId));
            cmd.Parameters.Add(new MySqlParameter("currentDate", currentDate));

            MySqlDataReader rdr = cmd.ExecuteReader();

            EmployeeHoliday empHol;

            while (rdr.Read())
            {
                empHol = new EmployeeHoliday();
                empHol.Id = rdr.GetInt32(0);
                empHol.EmployeeName = rdr.GetString(3) + " " + rdr.GetString(4);
                empHol.Date = rdr.GetDateTime(1);
                empHol.State = rdr.GetString(2);

                returnHoliday.Add(empHol);
            }

            CloseConnection();

            return returnHoliday;
        }

        public string UpdateHolidays(List<EmployeeHoliday> employeeHolidays)
        {
            ConnectToDatabase();

            string query;
            MySqlCommand cmd;
            string errorMessage = "";

            foreach (EmployeeHoliday employeeHoliday in employeeHolidays)
            {
                int id = employeeHoliday.Id;
                string status = employeeHoliday.State;
                DateTime date = employeeHoliday.Date;

                query = "SELECT COUNT(*) from tblWorkHours WHERE Date = @date AND RotaStartTime <> '00:00:00' AND RotaFinishTime <> '00:00:00'";

                cmd = new MySqlCommand(query, mConn);
                cmd.Parameters.Add(new MySqlParameter("@date", date));

                Int32 count = Convert.ToInt32(cmd.ExecuteScalar());

                if (count <= 0 || status != "Approved")
                {
                    query = "UPDATE tblHoliday SET State = @state WHERE HolidayId = @id";

                    cmd = new MySqlCommand(query, mConn);

                    cmd.Parameters.Add(new MySqlParameter("state", status));
                    cmd.Parameters.Add(new MySqlParameter("id", id));

                    cmd.ExecuteNonQuery();
                }
                else
                {
                    errorMessage += string.Format("Holiday could not be authorised on {0} as employee is working", employeeHoliday.Date);
                }
            }

            if (errorMessage.Equals(""))
            {
                errorMessage = "Holidays have been changed successfully";
            }

            CloseConnection();
            return errorMessage;
        }

        public PayrollData GetEmployeePayroll(int employeeId, string storeId, DateTime startDate, DateTime finishDate, string fullName)
        {
            PayrollData payrollData = new PayrollData();
            ConnectToDatabase();

            string query = "SELECT * FROM tblWorkHours " +
                "JOIN tblEmployeeStore ON tblEmployeeStore.EmployeeId = tblWorkHours.EmployeeId " +
                "WHERE tblEmployeeStore.StoreId = @storeId AND tblWorkHours.StoreId = @storeId AND tblWorkHours.EmployeeId = @employeeId AND Date >= @startDate AND Date <= @finishDate";

            MySqlCommand cmd = new MySqlCommand(query, mConn);

            cmd.Parameters.Add(new MySqlParameter("startDate", startDate));
            cmd.Parameters.Add(new MySqlParameter("finishDate", finishDate));
            cmd.Parameters.Add(new MySqlParameter("employeeId", employeeId));
            cmd.Parameters.Add(new MySqlParameter("storeId", storeId));

            MySqlDataReader rdr = cmd.ExecuteReader();

            double totalWorkHours = 0;
            double totalRotaHours = 0;
            int row = 0;
            string errorMessage = "";
            while (rdr.Read())
            {
                string startTime = rdr["StartTime"].ToString();
                string finishTime = rdr["FinishTime"].ToString();
                TimeSpan workHours = TimeSpan.Parse(finishTime).Subtract(TimeSpan.Parse(startTime));
                totalWorkHours += workHours.Subtract(TimeSpan.Parse(rdr["BreakTime"].ToString())).TotalHours;

                //check for errors in the work hours which need to be displayed to the manager
                if (TimeSpan.Parse(startTime) == new TimeSpan(0,0,0) ^ TimeSpan.Parse(finishTime) == new TimeSpan(0,0,0))
                {
                    errorMessage += string.Format("A start time or finish time hasn't been entered on date: {0} for employee: {1} \n\n", DateTime.Parse(rdr["Date"].ToString()).ToShortDateString(), fullName);
                }else if (TimeSpan.Parse(startTime) > TimeSpan.Parse(finishTime))
                {
                    errorMessage += string.Format("The finish time is before the start time on date: {0} for employee: {1} \n\n", DateTime.Parse(rdr["Date"].ToString()).ToShortDateString(), fullName);
                }

                string rotaStartTime = rdr["RotaStartTime"].ToString();
                string rotaFinishTime = rdr["RotaFinishTime"].ToString();
                TimeSpan rotaWorkHours = TimeSpan.Parse(rotaFinishTime).Subtract(TimeSpan.Parse(rotaStartTime));
                totalRotaHours += rotaWorkHours.Subtract(TimeSpan.Parse(rdr["BreakTime"].ToString())).TotalHours;

                row++;
            }
            payrollData.HoursRota = Math.Round(totalRotaHours, 2);
            payrollData.HoursWorked = Math.Round(totalWorkHours, 2);
            payrollData.ErrorMessage = errorMessage;
            rdr.Close();
            CloseConnection();

            return payrollData;
        }
    }
}
