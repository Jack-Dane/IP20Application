using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinalProjectApplication
{
    class PayrollDataGridView : DataGridView, IPayrollObserver
    {
        private Button mPayrollButton;
        private RichTextBox mErrorTextBox;

        public PayrollDataGridView()
        {
        }

        public void UpdatePayroll(List<PayrollData> payrollWeek)
        {
            string[] row;
            string errorMessage = "";

            Rows.Clear();
            Refresh();

            foreach (PayrollData payrollData in payrollWeek)
            {
                string name = payrollData.EmployeeName;
                string hoursRota = payrollData.HoursRota.ToString();
                string hoursWorked = payrollData.HoursWorked.ToString();
                string holidayUsed = payrollData.HolidayHours.ToString();
                string contractHours = payrollData.ContractHours.ToString();
                errorMessage += payrollData.ErrorMessage;

                row = new string[] { name, hoursRota, hoursWorked, holidayUsed, contractHours };
                Rows.Add(row);
            }

            mErrorTextBox.Text = errorMessage;

            if (!errorMessage.Equals(""))
            {
                mPayrollButton.Enabled = false;
            }
            else
            {
                mPayrollButton.Enabled = true;
            }
        }

        public void payrollButton(Button payrollButton)
        {
            mPayrollButton = payrollButton;
        }

        public void errorTextBox(RichTextBox errorTextBox)
        {
            mErrorTextBox = errorTextBox;
        }
    }
}
