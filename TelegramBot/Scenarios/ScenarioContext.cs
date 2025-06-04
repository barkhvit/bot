using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.TelegramBot.Scenarios
{
    public class ScenarioContext
    {
        public long UserId { get; set; }
        public ScenarioType CurrentScenario { get; set; }
        public string? CurrentStep { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();

        public ScenarioContext(long userId,ScenarioType scenario)
        {
            UserId = userId;
            CurrentScenario = scenario;
        }
    }

    
    
}
