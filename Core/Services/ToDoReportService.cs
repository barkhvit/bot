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
        public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
        {
            int Total = _toDoRepository.GetAllByUserId(userId).Count;
            int Active = _toDoRepository.GetActiveByUserId(userId).Count;
            int Completed = _toDoRepository.GetAllByUserId(userId).Count(t => t.State == Entities.ToDoItemState.Completed);
            DateTime dateTime = DateTime.UtcNow;
            return (Total, Completed, Active, dateTime);
        }
    }
}
