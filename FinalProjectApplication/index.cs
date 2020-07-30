using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinalProjectApplication
{
    public partial class index : Form, IExcelObserver
    {
        List<Year> mYears = new List<Year>();
        Manager mManager;

        public index(Manager model)
        {
            InitializeComponent();

            uiPayrollDataGridView.payrollButton(uiCompletePayrollButton);
            uiPayrollDataGridView.errorTextBox(uiPayrollErrorLogRichTextBox);

            MaximizeBox = false;

            mManager = model;

            mManager.AddHolidayObserver(uiHolidayDataGridView);
            mManager.AddWorkHoursObserver(uiEmployeeDataGridView);
            mManager.AddPayrollObserver(uiPayrollDataGridView);
            mManager.registerExcelObserver(this);

            mManager.GetPayrollWeek();

            uiHolidayDataGridView.AutoGenerateColumns = false;
            uiEmployeeDataGridView.AutoGenerateColumns = false;
            uiPayrollDataGridView.AutoGenerateColumns = false;

            foreach (Employee e in mManager.Employees)
            {
                uiEmployeePickerComboBox.Items.Add(e.Name);
            }

            //some work weeks will have 53 days
            for (int i = 1; i < 54; i++)
            {
                uiWeekPickerComboBox.Items.Add(i);
            }

            int currentYear = mManager.CurrentYear;

            uiYearPickerComboBox.Items.Add(currentYear - 1);
            mYears.Add(new Year(currentYear - 1));
            uiYearPickerComboBox.Items.Add(currentYear);
            mYears.Add(new Year(currentYear));
            uiYearPickerComboBox.Items.Add(currentYear + 1);
            mYears.Add(new Year(currentYear + 1));

            uiWeekNumberLabel.Text = "Week N.O. " + mManager.CurrentWeekNumber.ToString();
        }

        public DataGridView SelectedHolidayRows { get { return uiHolidayDataGridView; } }

        public DataGridView WorkHoursGrid { get { return uiEmployeeDataGridView; } }

        public List<Year> Years { get { return mYears; } }

        public ComboBox EmployeeComboBox { get { return uiEmployeePickerComboBox; } }

        public ComboBox WeekPickComboBox { get { return uiWeekPickerComboBox; } }

        public ComboBox YearPickComboBox { get { return uiYearPickerComboBox; } }

        public void addGrantHolidayButtonPress(IndexController controller)
        {
            uiAuthoriseHolidayButton.Click += controller.GrantHolidayButtonPress;
        }

        public void addDeclineHolidayButtonPress(IndexController controller)
        {
            uiDeclineHolidayButton.Click += controller.DeclineHolidayButtonPress;
        }

        public void addSelectButtonPress(IndexController controller)
        {
            uiSelectEmployeeData.Click += controller.SelectEmployeeButtonPress;
        }

        public void addAddRotaDateTimeButtonPress(IndexController controller)
        {
            uiAddRotaDateTimeButton.Click += controller.AddRotaDayButtonPress;
        }

        public void addLogoutButtonPress(IndexController controller)
        {
            uiLogoutButton.Click += controller.LogoutButtonPress;
        }

        public void addExportButtonPress(IndexController controller)
        {
            uiExportWorkWeekButton.Click += controller.ExportButtonPress;
        }

        public void addCompletePayrollButtonPress(IndexController controller)
        {
            uiCompletePayrollButton.Click += controller.CompletePayrollButtonPress;
        }

        public void addRefreshPayrollButtonPress(IndexController controller)
        {
            uiRefreshPayrollButton.Click += controller.RefereshPayrollButtonPress;
        }

        public void ShowError(string body, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            MessageBox.Show(body, title, buttons, icon);
        }

        public void updateContractHours(int contractHours)
        {
            uiContractHours.Text = "Contract Hours: " + contractHours.ToString();
        }

        public void Successful(string directory)
        {
            MessageBox.Show("The MS Excel document has been saved in the directory: " + directory, "Success", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public void Error(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
