using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.Core.Services
{
    public static class SetMyCommandsService
    {
        public static List<BotCommand> mainCommands = new List<BotCommand>()
        {
            new(){Command = "showalltasks", Description = "показать все задачи" },
            new(){Command = "showtasks", Description = "показать активные задачи" },
            new(){Command = "report", Description = "отчет по задачам" },
            new(){Command = "info", Description = "информация о боте" },
            new(){Command = "help", Description = "помощь" }
        };
    }
}
