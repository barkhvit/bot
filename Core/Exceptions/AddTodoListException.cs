﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Exceptions
{
    public class AddTodoListException : Exception
    {
        public AddTodoListException(string textMessage) : base(message: textMessage)
        {
            
        }
    }
}
