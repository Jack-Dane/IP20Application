using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectApplication
{
    public class Year
    {
        private int mYear;
        private DateTime mFirstSunday;

        public Year(int year)
        {
            mYear = year;

            mFirstSunday = FirstSunday(new DateTime(mYear, 1, 1));
        }

        public List<DateTime> GetWeekStartFinish(int weekNO)
        {
            weekNO -=1;
            DateTime first = mFirstSunday.AddDays(weekNO * 7);
            DateTime second = first.AddDays(6);

            return new List<DateTime>() {first, second };
        }

        private DateTime FirstSunday(DateTime date)
        {
            if (date.DayOfWeek != DayOfWeek.Sunday)
            {
                return FirstSunday(date.AddDays(1));
            }

            return date;
        }

        public int GetWeekNumber(DateTime date)
        {
            int weekNo = 0;
            while (date >= mFirstSunday.AddDays(7 * weekNo))
            {
                weekNo++;
            }
            if (weekNo == 0)
            {
                Year year = new Year(date.AddYears(-1).Year);
                return year.GetWeekNumber(date);
            }

            return weekNo;
        }

        public int YearNumber { get { return mYear; } }

        public DateTime GetFirstSunday { get { return mFirstSunday; } }
    }

    public class EmployeeWorkWeek
    {
        public String StoreId { get; set; }
        public DateTime Date { get; set; }
        public string Day { get; set; }
        public string StartTime { get; set; }
        public string FinishTime { get; set; }
        public string RotaStartTime { get; set; }
        public string RotaFinishTime { get; set; }
        public string LunchTime { get; set; }
    }

    public class EmployeeHoliday
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public DateTime Date { get; set; }
        public string State { get; set; }
    }

    public class PayrollData
    {
        public string EmployeeName { get; set; }
        public double HoursRota { get; set; }
        public double HoursWorked { get; set; }
        public double HolidayHours { get; set; }
        public int ContractHours { get; set; }
        public string ErrorMessage { get; set; }
    }

    public static class DateTimeExtention {
        public static bool IsValidTime(this string input)
        {
            TimeSpan output;
            return TimeSpan.TryParse(input, out output);
        }

        public static string EmptyTimeString(this string input)
        {
            return (input == string.Empty) ? "00:00:00" : input;
        }
    }
}
