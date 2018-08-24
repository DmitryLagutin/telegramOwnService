using OwinSelfHostSample.Models;
using OwinSelfHostSample.MyContext;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OwinSelfHostSample
{
    public class MessageController : ApiController
    {

        [Route(@"api/Message/Update")]
        public async  Task<OkResult> Update([FromBody]Update update)
        {
           Message msg = update.Message;
           
           if (msg == null || msg.Type != MessageType.TextMessage)
           {
               await Bot.Api.SendTextMessageAsync(msg.Chat.Id, "Неверный тип сообщения. Я обрабатываю только текст");
           }
           else
           {
               if (Helper.UserList.SingleOrDefault(p => p.ChartID == msg.Chat.Id) != null)
               {
                   Models.User CurrentUser = Helper.UserList.SingleOrDefault(p => p.ChartID == msg.Chat.Id);
                   await Bot.Api.MainMessage(msg, CurrentUser);
               }
               else
               {
                   Models.User user = new Models.User() { ChartID = msg.Chat.Id };
                   Helper.UserList.Add(user);
                   await Bot.Api.MainMessage(msg, user);
               }
           }
           
           return Ok();
           
        }

    }
}
