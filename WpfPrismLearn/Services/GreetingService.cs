using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPrismLearn.Services
{
    internal class GreetingService : IGreetingService
    {
        public string GetMessage(string name)
        {
            var time = DateTime.Now.ToString("HH:mm:ss");
            return $"Hello, {name}! Welcome to WPF with Prism. Now Time is {time}.";
        }
    }
}
