using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectApplication
{
    public interface IEmployeeDatabase
    {
        List<EmployeeHoliday> GetHolidays(int employeeId);
        List<EmployeeWorkWeek> GetEmployeeWorkWeek(List<EmployeeWorkWeek> workWeek, int employeeId, string storeId, DateTime startWeek, DateTime finishWeek);
        PayrollData GetEmployeePayroll(int employeeId, string storeId, DateTime startWeek, DateTime finishWeek, string fullName);
        bool InsertWorkHours(int employeeId, string storeId, string startTime, string finishTime, string rotaStartTime, string rotaFinishTime, string breakTime, DateTime date);
        void DeleteFromWorkWeek(int employeeId, DateTime date);
    }
}
