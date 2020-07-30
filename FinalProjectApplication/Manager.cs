using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace FinalProjectApplication
{
    public class Manager : IWorkHoursSubject, IHolidaySubject, IPayrollSubject, IExcelSubject
    {
        private string mStoreId;
        private string mId;
        private int mCurrentYear;
        private Employee mSelectedEmployee;
        private List<Employee> mEmployees;
        private IManagerDatabase mDBConnection;
        private List<IWorkHoursObserver> mWorkHoursObservers;
        private List<IHolidayObserver> mHolidayObservers;
        private List<IPayrollObserver> mPayrollObservers;
        private List<IExcelObserver> mExcelObservers;
        private bool mSavedExcelSuccess = false;
        private string mExcelDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private bool mUpdatedWorkDaysSuccess = false;
        private bool mUpdated = false;
        private int mWeekNo;
        private Year mYear;
        private List<EmployeeWorkWeek> mSelectedEmployeeWorkWeek;
        private List<EmployeeHoliday> mEmployeesHoliday;
        private List<PayrollData> mPayrollData;
        private int mCurrentWeek;
        private string mCurrentErrorMessage;

        public Manager(string storeId, string employeeId, List<Employee> employees, IManagerDatabase dbConnection)
        {
            mDBConnection = dbConnection;
            mStoreId = storeId;
            mId = employeeId;

            //set year
            DateTime date = DateTime.Now;
            mCurrentYear = date.Year;
            mEmployees = employees;

            mSelectedEmployeeWorkWeek = new List<EmployeeWorkWeek>();
            mPayrollData = new List<PayrollData>();

            mHolidayObservers = new List<IHolidayObserver>();
            mWorkHoursObservers = new List<IWorkHoursObserver>();
            mPayrollObservers = new List<IPayrollObserver>();
            mExcelObservers = new List<IExcelObserver>();
        }

        public void GetEmployeesData(int employeeIndex, int week, Year year)
        {
            Employee emp = Employees[employeeIndex];
            mSelectedEmployee = emp;
            mWeekNo = week;
            mYear = year;
            mSelectedEmployeeWorkWeek = (emp.GetHours(week, year));

            NotifyDataGridObserver();
        }

        public void UpdateEmployeeRota(List<EmployeeWorkWeek> rota)
        {
            //check that the rota or holiay hasnt been updated by another store while deciding what days to rota
            mSelectedEmployee.refreshHoliday();
            List<EmployeeWorkWeek> currentEmployeeWorkWeek = mSelectedEmployee.GetHours(mWeekNo, mYear);

            List<int> correctValues = new List<int>();
            mCurrentErrorMessage = "The rota could not be updated due to: \n";

            int errorCode;
            int index = 0;

            for (int i = 0; i < rota.Count; i++)
            {
                errorCode = validateWorkWeek(rota[i], currentEmployeeWorkWeek[i]);
                correctValues.Add(errorCode);
                index++;

                switch (errorCode)
                {
                    case 0:
                        mCurrentErrorMessage += "Error with database \n";
                        break;
                    case -1:
                        mCurrentErrorMessage += string.Format("Holiday booked on date: {0} \n", rota[i].Date.ToShortDateString());
                        break;
                    case -2:
                        mCurrentErrorMessage += string.Format("Only rota start time or rota finish time enetered on date: {0} \n", rota[i].Date.ToShortDateString());
                        break;
                    case -3:
                        mCurrentErrorMessage += string.Format("Valid time not entered on date: {0} \n", rota[i].Date.ToShortDateString());
                        break;
                    case -4:
                        mCurrentErrorMessage += string.Format("The employee is scheduled to work on date: {0} \n", rota[i].Date.ToShortDateString());
                        break;
                    case -5:
                        mCurrentErrorMessage += string.Format("The rota start time is after the rota finish time on date: {0} \n", rota[i].Date.ToShortDateString());
                        break;
                }
            }

            mCurrentErrorMessage += "Please select the work week again";

            //if all days have been sucesseful
            mUpdatedWorkDaysSuccess = correctValues.All(value => value == 1);

            mUpdated = true;
            if (mUpdatedWorkDaysSuccess)
            {
                foreach (EmployeeWorkWeek workHours in rota)
                {
                    mSelectedEmployee.UpdateRota(workHours);
                }
                mSelectedEmployeeWorkWeek = rota;
            }

            NotifyDataGridObserver();
        }

        public void SetEmployeeHolidays()
        {
            List<EmployeeHoliday> tempEmployeeHoliay = new List<EmployeeHoliday>();

            foreach (Employee emp in mEmployees)
            {
                emp.refreshHoliday();
                List<EmployeeHoliday> empHoliday = emp.EmployeeHoliday;

                empHoliday.ForEach(item => tempEmployeeHoliay.Add(item));
            }

            mEmployeesHoliday = tempEmployeeHoliay;

            NotifyHolidayObserver();
        }

        public void ExportWorkRota(int weekNo, Year year)
        {
            Excel.Application excelApplication = new Excel.Application();
            if (excelApplication != null)
            {
                Excel.Workbook excelFileBook;
                Excel.Worksheet excelFileSheet;
                object misValue = System.Reflection.Missing.Value;

                excelFileBook = excelApplication.Workbooks.Add(misValue);
                excelFileSheet = (Excel.Worksheet)excelFileBook.Worksheets.get_Item(1);

                List<DateTime> weekStartFinish = year.GetWeekStartFinish(weekNo);
                DateTime startDate = weekStartFinish[0];
                DateTime finishDate = weekStartFinish[1];

                int loopIndex = 2;
                DateTime tempStartDate = startDate;
                while(tempStartDate <= finishDate)
                {
                    //input date at top of screen
                    excelFileSheet.Cells[1, loopIndex] = "'" + tempStartDate.ToShortDateString();
                    tempStartDate = tempStartDate.AddDays(1);
                    loopIndex++;
                }

                int xLoop = 2;
                int yLoop = 2;
                List<EmployeeWorkWeek> employeeRota;
                foreach (Employee emp in mEmployees)
                {
                    excelFileSheet.Cells[yLoop, 1] = emp.Name;

                    employeeRota = emp.GetHours(weekNo, year);

                    tempStartDate = startDate;
                    foreach (EmployeeWorkWeek workWeek in employeeRota)
                    {
                        if (workWeek.StoreId == mStoreId)
                        {
                            string displayData = workWeek.RotaStartTime + " - " + workWeek.RotaFinishTime;
                            excelFileSheet.Cells[yLoop, xLoop] = displayData;
                            xLoop++;
                        }else if (workWeek.StoreId == "Holiday")
                        {
                            string displayData = "Holiday";
                            excelFileSheet.Cells[yLoop, xLoop] = displayData;
                            xLoop++;
                        }
                        else
                        {
                            string displayData = "00:00:00 - 00:00:00";
                            excelFileSheet.Cells[yLoop, xLoop] = displayData;
                            xLoop++;
                        }
                    }

                    xLoop = 2;
                    yLoop++;
                }

                excelFileSheet.Columns.AutoFit();

                try
                {
                    excelFileBook.SaveAs(mExcelDirectory + "\\WorkWeek.xls", Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
                    mSavedExcelSuccess = true;
                }
                catch(Exception e)
                {
                    //open window to save
                }

                if (!excelFileBook.Saved)
                {
                    mSavedExcelSuccess = false;
                }
                
                excelFileBook.Close(0);
                excelApplication.Quit();

                Marshal.ReleaseComObject(excelFileSheet);
                Marshal.ReleaseComObject(excelFileBook);
                Marshal.ReleaseComObject(excelApplication);
            }
            else
            {
                mSavedExcelSuccess = false;
                //EXCEL not installed on system
            }
            
            notifyExcelObservers();
        }

        public void UpdateHolidaysStatus()
        {
            mCurrentErrorMessage = mDBConnection.UpdateHolidays(mEmployeesHoliday);

            SetEmployeeHolidays();
        }

        public void GetPayrollWeek()
        {
            Year currentYear = new Year(mCurrentYear);
            DateTime now = DateTime.Now;

            int weekNo = currentYear.GetWeekNumber(now);
            mCurrentWeek = weekNo;

            List<PayrollData> employeesPayrollData = new List<PayrollData>();

            foreach (Employee emp in mEmployees)
            {
                employeesPayrollData.Add(emp.GetEmployeePayroll(weekNo, currentYear));
            }

            mPayrollData = employeesPayrollData;

            NotifyPayrollObserver();
        }

        private int validateWorkWeek(EmployeeWorkWeek workWeek, EmployeeWorkWeek currenWorkWeek)
        {
            //if the rota is for the employee working in this store, otherwise don't update
            if (workWeek.StoreId.Equals(mStoreId))
            {
                if (workWeek.StartTime.IsValidTime() && workWeek.FinishTime.IsValidTime() && workWeek.RotaStartTime.IsValidTime() && workWeek.RotaFinishTime.IsValidTime() && workWeek.LunchTime.IsValidTime())
                {
                    //delete all where time and date has been set to 00:00:00
                    DateTime date = workWeek.Date;

                    if (TimeSpan.Parse(workWeek.RotaStartTime) != new TimeSpan(0, 0, 0) && TimeSpan.Parse(workWeek.RotaFinishTime) != new TimeSpan(0, 0, 0))
                    {
                        if (TimeSpan.Parse(workWeek.RotaStartTime) < TimeSpan.Parse(workWeek.RotaFinishTime))
                        {
                            List<EmployeeHoliday> holidayDays = mSelectedEmployee.EmployeeHoliday;
                            foreach (EmployeeHoliday empHol in holidayDays)
                            {
                                if (empHol.Date == date && empHol.State == "Approved")
                                {
                                    return -1;
                                }
                            }
                            if (!currenWorkWeek.StoreId.Equals(mStoreId))
                            {
                                return -4;
                            }
                        }
                        else
                        {
                            return -5;
                        }
                    }
                    else
                    {
                        //if they are not all empty, error has occured
                        if (TimeSpan.Parse(workWeek.RotaStartTime) == new TimeSpan(0, 0, 0) ^ TimeSpan.Parse(workWeek.RotaFinishTime) == new TimeSpan(0, 0, 0))
                        {
                            return -2;
                        }
                    }
                }
                else
                {
                    //valid time haven't been entered, for example contaning letters
                    return -3;
                }
            }
            return 1;
        }

        public int getEmployeeContractHours()
        {
            return mSelectedEmployee.Hours;
        }

        public void AddWorkHoursObserver(IWorkHoursObserver o)
        {
            mWorkHoursObservers.Add(o);
        }

        public void RemoveWorkHoursObserver(IWorkHoursObserver o)
        {
            mWorkHoursObservers.Remove(o);
        }

        public void NotifyDataGridObserver()
        {
            foreach (IWorkHoursObserver observer in mWorkHoursObservers)
            {
                if (mUpdated)
                {
                    if (mUpdatedWorkDaysSuccess)
                    {
                        observer.WorkHoursUpdated();
                        observer.UpdateWorkHours(mSelectedEmployeeWorkWeek, mStoreId);
                    }
                    else
                    {
                        observer.WorkHoursError(mCurrentErrorMessage);
                    }
                }
                else
                {
                    observer.UpdateWorkHours(mSelectedEmployeeWorkWeek, mStoreId);
                }

            }

            mUpdated = false;
        }

        public void AddHolidayObserver(IHolidayObserver o)
        {
            mHolidayObservers.Add(o);
        }

        public void RemoveHolidayObserver(IHolidayObserver o)
        {
            mHolidayObservers.Remove(o);
        }

        public void NotifyHolidayObserver()
        {
            foreach (IHolidayObserver observer in mHolidayObservers)
            {
                observer.GetEmployeesHoliday(mEmployeesHoliday, mCurrentErrorMessage);
            }
        }

        public void AddPayrollObserver(IPayrollObserver o)
        {
            mPayrollObservers.Add(o);
        }

        public void RemovePayrollObserver(IPayrollObserver o)
        {
            mPayrollObservers.Remove(o);
        }

        public void NotifyPayrollObserver()
        {
            foreach (IPayrollObserver o in mPayrollObservers)
            {
                o.UpdatePayroll(mPayrollData);
            }
        }

        public void registerExcelObserver(IExcelObserver e)
        {
            mExcelObservers.Add(e);
        }

        public void removeExcelObserver(IExcelObserver e)
        {
            mExcelObservers.Remove(e);
        }

        public void notifyExcelObservers()
        {
            foreach (IExcelObserver e in mExcelObservers)
            {
                if (mSavedExcelSuccess)
                {
                    e.Successful(mExcelDirectory);
                }
                else
                {
                    e.Error("Something went wrong, make sure excel is installed on the system");
                }
            }
        }

        public int CurrentYear { get { return mCurrentYear; } }

        public List<EmployeeWorkWeek> EmployeeWorkWeek { get { return mSelectedEmployeeWorkWeek; } }

        public List<Employee> Employees { get { return mEmployees; } }

        public List<EmployeeHoliday> EmployeeHolidays { get { return mEmployeesHoliday; } }

        public List<PayrollData> PayrollWeek { get { return mPayrollData; } }

        public int CurrentWeekNumber { get { return mCurrentWeek; } }
    }
}
