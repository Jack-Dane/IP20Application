using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinalProjectApplication
{
    public class HolidayDataGridView : DataGridView, IHolidayObserver
    {
        public HolidayDataGridView()
        {
        }


        public void GetEmployeesHoliday(List<EmployeeHoliday> employeeHoliday, string errorMessage)
        {
            Rows.Clear();
            Refresh();

            foreach (EmployeeHoliday empHoliday in employeeHoliday)
            {
                string id = empHoliday.Id.ToString();
                string employeeId = empHoliday.EmployeeName;
                DateTime date = empHoliday.Date;
                string status = empHoliday.State;

                Rows.Add(id, date, employeeId, status);
            }

            if (errorMessage != null)
            {
                MessageBox.Show(errorMessage, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
