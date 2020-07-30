using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectApplication
{
    public class Employee
    {
        string mFirstName;
        string mLastName;
        int mContractHours;
        int mId;
        string mStoreId;
        List<EmployeeHoliday> mHolidayDays;
        IEmployeeDatabase mDBConnection;
        string mEmail;

        public Employee(string firstName, string lastName, int contractHours, int id, string storeId, string email, IEmployeeDatabase dbConnection)
        {
            mDBConnection = dbConnection;
            mFirstName = firstName;
            mLastName = lastName;
            mContractHours = contractHours;
            mId = id;
            mStoreId = storeId;
            mEmail = email;

            refreshHoliday();
        }

        public void refreshHoliday()
        {
            mHolidayDays = mDBConnection.GetHolidays(mId);
        }

        public List<EmployeeWorkWeek> GetHours(int week, Year y)
        {
            List<DateTime> wholeWeek = y.GetWeekStartFinish(week);

            DateTime startWeek = wholeWeek[0];
            DateTime finishWeek = wholeWeek[1];

            List<EmployeeWorkWeek> workHours = new List<EmployeeWorkWeek>();

            DateTime enStartWeek = startWeek;
            while (enStartWeek <= finishWeek)
            {
                workHours.Add(new EmployeeWorkWeek { StoreId=mStoreId, Date = enStartWeek, Day = enStartWeek.DayOfWeek.ToString(), StartTime = "00:00:00", FinishTime = "00:00:00", RotaStartTime = "00:00:00", RotaFinishTime = "00:00:00", LunchTime = "00:00:00" });
                enStartWeek = enStartWeek.AddDays(1);
            }

            workHours = mDBConnection.GetEmployeeWorkWeek(workHours, mId, mStoreId, startWeek, finishWeek);

            foreach (EmployeeWorkWeek workDay in workHours)
            {
                foreach (EmployeeHoliday holiday in mHolidayDays)
                {
                    if (workDay.Date.Equals(holiday.Date) && holiday.State == "Approved")
                    {
                        workDay.StoreId = "Holiday";
                    }
                }
            }

            return workHours;
        }

        public PayrollData GetEmployeePayroll(int week, Year year)
        {
            List<DateTime> wholeWeek = year.GetWeekStartFinish(week);

            DateTime startWeek = wholeWeek[0];
            DateTime finishWeek = wholeWeek[1];

            string fullName = mFirstName + " " + mLastName;

            PayrollData payrollData = mDBConnection.GetEmployeePayroll(mId, mStoreId, startWeek, finishWeek, fullName);

            payrollData.EmployeeName = fullName;
            payrollData.ContractHours = mContractHours;
            if (payrollData.HoursWorked < payrollData.ContractHours)
            {
                payrollData.HolidayHours = Math.Round(payrollData.ContractHours - payrollData.HoursWorked, 2);
            }
            else
            {
                payrollData.HolidayHours = 0;
            }
            return payrollData;
        }

        public void UpdateRota(EmployeeWorkWeek workWeek)
        {
            if (workWeek.StoreId == mStoreId)
            {
                DateTime date = workWeek.Date;
                mDBConnection.DeleteFromWorkWeek(mId, date);
                if (TimeSpan.Parse(workWeek.RotaStartTime) != new TimeSpan(0, 0, 0) || TimeSpan.Parse(workWeek.RotaFinishTime) != new TimeSpan(0, 0, 0)
                        || TimeSpan.Parse(workWeek.StartTime) != new TimeSpan(0, 0, 0) || TimeSpan.Parse(workWeek.FinishTime) != new TimeSpan(0, 0, 0)
                        || TimeSpan.Parse(workWeek.LunchTime) != new TimeSpan(0, 0, 0))
                {
                    bool sucsses = mDBConnection.InsertWorkHours(mId, mStoreId, workWeek.StartTime, workWeek.FinishTime, workWeek.RotaStartTime, workWeek.RotaFinishTime, workWeek.LunchTime, workWeek.Date);
                }
            }
        }

        public virtual int Hours { get { return mContractHours; } }

        public string Name { get { return mFirstName + " " + mLastName; } }

        public int Id { get { return mId; } }

        public List<EmployeeHoliday> EmployeeHoliday { get { return mHolidayDays; } }
    }
}
