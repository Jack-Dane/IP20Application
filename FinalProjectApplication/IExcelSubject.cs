using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectApplication
{
    interface IExcelSubject
    {
        void registerExcelObserver(IExcelObserver e);
        void removeExcelObserver(IExcelObserver e);
        void notifyExcelObservers();
    }
}
