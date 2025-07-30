using Bot.TelegramBot.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.BackgroundTask
{
    public class ResetScenarioBackgroundTask : BackgroundTask
    {
        private readonly TimeSpan _resetScenarioTimeout;
        private readonly IScenarioContextRepository _scenarioRepository;
        private readonly ITelegramBotClient _bot;

        // Конструктор принимает:
        // - Таймаут для сброса сценария (1 час)
        // - Репозиторий сценариев
        // - Клиент бота для отправки сообщений
        public ResetScenarioBackgroundTask(
            TimeSpan resetScenarioTimeout,
            IScenarioContextRepository scenarioRepository,
            ITelegramBotClient bot)
            : base(TimeSpan.FromHours(1), nameof(ResetScenarioBackgroundTask))
            {
                _resetScenarioTimeout = resetScenarioTimeout;
                _scenarioRepository = scenarioRepository;
                _bot = bot;
            }

        protected override async Task Execute(CancellationToken ct)
        {
            // Получаем все активные контексты сценариев
            var contexts = await _scenarioRepository.GetContexts(ct);
            var now = DateTime.UtcNow;

            // Проверяем каждый контекст
            foreach (var context in contexts)
            {
                // Если контекст старше заданного таймаута
                if (now - context.CreatedAt > _resetScenarioTimeout)
                {
                    // Сбрасываем контекст
                    await _scenarioRepository.ResetContext(context.UserId, ct);

                    // Создаем клавиатуру с основными командами
                    var keyboard = new ReplyKeyboardMarkup(new[]
                    {
                    new KeyboardButton("/addtask"),
                    new KeyboardButton("/show"),
                    new KeyboardButton("/report")
                })
                    {
                        ResizeKeyboard = true // Автоматически подгонять размер
                    };

                    // Отправляем сообщение пользователю
                    await _bot.SendMessage(
                        context.UserId,
                        $"Сценарий отменен, так как не поступил ответ в течение {_resetScenarioTimeout}",
                        replyMarkup: keyboard,
                        cancellationToken: ct);
                }
            }
        }
    }

}

