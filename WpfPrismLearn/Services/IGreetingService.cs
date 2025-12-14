using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPrismLearn.Services
{
    public interface IGreetingService
    {
        string GetMessage(string name);
    }
}
