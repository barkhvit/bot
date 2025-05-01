using Bot.Core.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Services
{
    public class ToDoReportService : IToDoReportService
    {

        private readonly IToDoRepository _toDoRepository;

        public ToDoReportService(IToDoRepository toDoRepository)
        {
            _toDoRepository = toDoRepository;
        }
        public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStats(Guid userId, CancellationToken cancellationToken)
        {
            var AllItems = await _toDoRepository.GetAllByUserId(userId, cancellationToken);

            int Total = AllItems.Count;
            int Active = AllItems.Count(i => i.State == Entities.ToDoItemState.Active);
            int Completed = AllItems.Count(i => i.State == Entities.ToDoItemState.Completed);
            return (Total, Completed, Active, DateTime.UtcNow);
        }
    }
}
