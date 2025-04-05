using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    class IncorrectTaskException : Exception
    {
        public IncorrectTaskException():base("После команды /addtask через пробел введите название задачи")
        {
            
        }
    }
}
