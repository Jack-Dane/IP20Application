using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectApplication
{
    public interface IWorkHoursObserver
    {
        void UpdateWorkHours(List<EmployeeWorkWeek> employeeWorkWeek, string StoreId);
        void WorkHoursUpdated();
        void WorkHoursError(String errroMessage);
    }
}
