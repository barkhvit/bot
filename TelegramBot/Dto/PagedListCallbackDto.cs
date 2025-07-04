using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.TelegramBot.Dto
{
    public class PagedListCallbackDto : ToDoListCallbackDto
    {
        public int Page { get; set; } = 0;

        public static new PagedListCallbackDto FromString(string input)
        {
            string[] str = input.Split('|');
            int _page = 0;
            if (str.Length > 2)
            {
                if (int.TryParse(str[2], out int page))
                {
                    _page = page;
                }
            }

            Guid? guid = null;

            if (str.Length > 1)
            {
                if (Guid.TryParse(str[1], out Guid result))
                {
                    guid = result;
                }

            }

            return new PagedListCallbackDto()
            {
                Action = str[0],
                ToDoListId = guid,
                Page = _page
            };
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{Page.ToString()}";
        }


    }
}
