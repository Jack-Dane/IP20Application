using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectApplication
{
    public interface IExcelObserver
    {
        void Successful(string directory);
        void Error(string ErrorMessage);
    }
}
