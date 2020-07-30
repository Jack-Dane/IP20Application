using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectApplication
{
    interface IPayrollSubject
    {
        void AddPayrollObserver(IPayrollObserver o);
        void RemovePayrollObserver(IPayrollObserver o);
        void NotifyPayrollObserver();
    }
}
