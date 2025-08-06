using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.TelegramBot.Scenarios
{
    public class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        private readonly ConcurrentDictionary<long, ScenarioContext> _contexts = new();
        public Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            _contexts.TryGetValue(userId, out var context);
            return Task.FromResult(context);
        }

        public Task<IReadOnlyList<ScenarioContext>> GetContexts(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            // если словарь пуст
            if (_contexts.IsEmpty)
            {
                return Task.FromResult<IReadOnlyList<ScenarioContext>>(Array.Empty<ScenarioContext>());
            }

            // Создаем список из словаря
            IReadOnlyList<ScenarioContext> contexts = _contexts.Values.ToArray();
            return Task.FromResult(contexts);
        }

        public Task ResetContext(long userId, CancellationToken ct)
        {
            _contexts.Remove(userId,out ScenarioContext? sc);
            return Task.CompletedTask;
        }

        public Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            _contexts[userId] = context;
            return Task.CompletedTask;
        }
    }
}
