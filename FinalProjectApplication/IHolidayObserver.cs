using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectApplication
{
    public interface IHolidayObserver
    {
        void GetEmployeesHoliday(List<EmployeeHoliday> employeeHolidays, string errorMessage);
    }
}
