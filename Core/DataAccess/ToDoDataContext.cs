using Bot.Core.DataAccess.Models;
using LinqToDB;
using LinqToDB.Data;

namespace Bot.Core.DataAccess
{
    public class ToDoDataContext : DataConnection
    {
        public ToDoDataContext(string connectionString) : base(ProviderName.PostgreSQL, connectionString) { }

        public ITable<ToDoUserModel> ToDoUsers => this.GetTable<ToDoUserModel>();
        public ITable<ToDoListModel> ToDoLists => this.GetTable<ToDoListModel>();
        public ITable<ToDoItemModel> ToDoItems => this.GetTable<ToDoItemModel>();
    }
}
