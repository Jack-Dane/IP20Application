using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinalProjectApplication
{
    public class EmployeeDataGridView : DataGridView, IWorkHoursObserver
    {
        public EmployeeDataGridView()
        {}

        public void UpdateWorkHours(List<EmployeeWorkWeek> currentWorkWeek, string StoreId)
        {
            Rows.Clear();
            Refresh();

            if (currentWorkWeek != null)
            {
                string[] row;
                int index = 0;

                foreach (EmployeeWorkWeek eW in currentWorkWeek)
                {
                    string storeId = eW.StoreId;
                    string day = eW.Day;
                    string date = eW.Date.ToShortDateString();
                    string start = eW.StartTime;
                    string finish = eW.FinishTime;
                    string rotaStart = eW.RotaStartTime;
                    string rotaFinish = eW.RotaFinishTime;
                    string lunchTime = eW.LunchTime;

                    row = new string[] {storeId, day, date, start, finish, rotaStart, rotaFinish, lunchTime };
                    Rows.Add(row);
                    if (!storeId.Equals(StoreId))
                    {
                        Rows[index].ReadOnly = true;
                    }
                    index++;
                }
            }
            else
            {
                MessageBox.Show("Something went wrong", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void WorkHoursUpdated()
        {
            string text = "Update Sucessful";
            string title = "Success";
            MessageBoxIcon icon = MessageBoxIcon.Exclamation;
            MessageBox.Show(text, title, MessageBoxButtons.OK, icon);
        }

        public void WorkHoursError(string errorMessage)
        {
            string text = errorMessage;
            string title = "Unsuccessful";
            MessageBoxIcon icon = MessageBoxIcon.Exclamation;
            MessageBox.Show(text, title, MessageBoxButtons.OK, icon);
        }
    }
}
