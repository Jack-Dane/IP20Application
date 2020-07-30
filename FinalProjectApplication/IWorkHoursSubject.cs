using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectApplication
{
    public interface IWorkHoursSubject
    {
        void AddWorkHoursObserver(IWorkHoursObserver o);
        void RemoveWorkHoursObserver(IWorkHoursObserver o);
        void NotifyDataGridObserver();
    }
}