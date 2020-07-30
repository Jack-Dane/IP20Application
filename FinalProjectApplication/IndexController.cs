using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinalProjectApplication
{
    public class IndexController
    {
        private index mView;
        private Manager mModel;

        public IndexController(index view, Manager model)
        {
            mView = view;
            mModel = model;

            mView.addAddRotaDateTimeButtonPress(this);
            mView.addDeclineHolidayButtonPress(this);
            mView.addGrantHolidayButtonPress(this);
            mView.addLogoutButtonPress(this);
            mView.addSelectButtonPress(this);
            mView.addExportButtonPress(this);
            mView.addCompletePayrollButtonPress(this);
            mView.addRefreshPayrollButtonPress(this);

            mModel.SetEmployeeHolidays();
        }

        public void GrantHolidayButtonPress(object sender, EventArgs e)
        {
            updateHolidays("Approved");
        }

        public void DeclineHolidayButtonPress(object sender, EventArgs e)
        {
            updateHolidays("Denied");
        }

        private void updateHolidays(string status)
        {
            DataGridViewSelectedRowCollection selectedRows = mView.SelectedHolidayRows.SelectedRows;
            List<EmployeeHoliday> employeeHolidays = mModel.EmployeeHolidays;

            if (selectedRows.Count != 0)
            {
                foreach (DataGridViewRow row in selectedRows)
                {
                    foreach (EmployeeHoliday employeeHoliday in employeeHolidays)
                    {
                        if (employeeHoliday.Id == int.Parse(row.Cells[0].Value.ToString()))
                        {
                            employeeHoliday.State = status;
                        }
                    }
                }
                mModel.UpdateHolidaysStatus();
            }
            else
            {
                mView.ShowError("No holiday rows have been selected", "No Rows", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        public void SelectEmployeeButtonPress(object sender, EventArgs e)
        {
            if (mView.YearPickComboBox.SelectedIndex != -1 && mView.WeekPickComboBox.SelectedIndex != -1 && mView.EmployeeComboBox.SelectedIndex != -1)
            {
                GetUpdatedRota();
            }
            else
            {
                mView.ShowError("Not all items have been selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void AddRotaDayButtonPress(object sender, EventArgs e)
        {
            List<EmployeeWorkWeek> rota = new List<EmployeeWorkWeek>();
            if (mView.WorkHoursGrid.Rows.Count != 0)
            {
                foreach (DataGridViewRow row in mView.WorkHoursGrid.Rows)
                {
                    if (row.Cells[0].Value != null)
                    {
                        string storeId = row.Cells[0].Value.ToString();
                        string day = row.Cells[1].Value.ToString();
                        DateTime date = DateTime.Parse(row.Cells[2].Value.ToString());
                        string startTime = row.Cells[3].Value.ToString().EmptyTimeString();
                        string finishTime = row.Cells[4].Value.ToString().EmptyTimeString();
                        string rotaStartTime = row.Cells[5].Value.ToString().EmptyTimeString();
                        string rotaFinishTime = row.Cells[6].Value.ToString().EmptyTimeString();
                        string lunchTime = row.Cells[7].Value.ToString().EmptyTimeString();

                        rota.Add(new EmployeeWorkWeek { StoreId = storeId, Day = day, Date = date, StartTime = startTime, FinishTime = finishTime, RotaStartTime = rotaStartTime, RotaFinishTime = rotaFinishTime, LunchTime = lunchTime });
                    }
                }

                mModel.UpdateEmployeeRota(rota);
                if (mView.Years[mView.YearPickComboBox.SelectedIndex].YearNumber == mModel.CurrentYear
                    && int.Parse(mView.WeekPickComboBox.SelectedItem.ToString()) == mModel.CurrentWeekNumber)
                {
                    mModel.GetPayrollWeek();
                }
            }
            else
            {
                mView.ShowError("No week selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GetUpdatedRota()
        {
            int employeeIndex = mView.EmployeeComboBox.SelectedIndex;
            Year year = mView.Years[mView.YearPickComboBox.SelectedIndex];
            int week = int.Parse(mView.WeekPickComboBox.SelectedItem.ToString());

            mModel.GetEmployeesData(employeeIndex, week, year);
            mView.updateContractHours(mModel.getEmployeeContractHours());
        }

        public void LogoutButtonPress(object sender, EventArgs e)
        {
            Thread t = new Thread(() => OpenLoginPage());
            t.Start();
            mView.Close();

            void OpenLoginPage()
            {
                LoginModel loginModel = new LoginModel();
                Login form = new Login(loginModel);
                LoginController loginController = new LoginController(form, loginModel);
                Application.Run(form);
            }
        }

        public void ExportButtonPress(object sender, EventArgs e)
        {
            if (mView.YearPickComboBox.SelectedIndex != -1 && mView.WeekPickComboBox.SelectedIndex != -1)
            {
                Year year = mView.Years[mView.YearPickComboBox.SelectedIndex];
                int week = int.Parse(mView.WeekPickComboBox.SelectedItem.ToString());

                Thread thread = new Thread(()=>mModel.ExportWorkRota(week, year));
                thread.Start();
            }
            else
            {
                mView.ShowError("The week and year need to be selected before you can export", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void CompletePayrollButtonPress(object sender, EventArgs e)
        {
            mView.ShowError("As this is a prototype, implementation of the complete rota system hasn't been implemented yet", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public void RefereshPayrollButtonPress(object sender, EventArgs e)
        {
            mModel.GetPayrollWeek();
        }
    }
}
