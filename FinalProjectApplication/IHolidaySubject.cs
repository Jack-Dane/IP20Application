using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectApplication
{
    public interface IHolidaySubject
    {
        void AddHolidayObserver(IHolidayObserver o);
        void RemoveHolidayObserver(IHolidayObserver o);
        void NotifyHolidayObserver();
    }
}
