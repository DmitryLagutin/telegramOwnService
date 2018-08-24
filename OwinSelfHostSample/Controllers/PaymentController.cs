using OwinSelfHostSample.Models;
using OwinSelfHostSample.MyContext;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace OwinSelfHostSample.Controllers
{
    public class PaymentController : ApiController
    {
        [Route(@"api/Payment/Go")]
        [HttpPost]
        public async Task<OkResult> Go([FromBody]PaymentModel payment)
        {



            //Удалить в боевой версии
            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            Console.WriteLine(String.Format("operation_id: {0}",payment.operation_id));
            Console.WriteLine(String.Format("odatetime: {0}", payment.datetime));
            Console.WriteLine(String.Format("label: {0}", payment.label));
            Console.WriteLine(String.Format("amount: {0}", payment.amount));
            Console.WriteLine(String.Format("withdraw_amount: {0}", payment.withdraw_amount));
            Console.WriteLine("------------------------------------------");

            Console.WriteLine(Convert.ToDateTime(payment.datetime));
            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            using (AppContext db = new AppContext())
            {
                Order order = new Order();
                order.Operation_Id = payment.operation_id;
                if (payment.label != null)
                {
                    string[] separatingChars = { "+" };
                    string[] a = payment.label.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);
                    order.UserName = a[0];
                    order.ChartID = Convert.ToInt64(a[1]);

                    await Bot.Api.SendTextMessageAsync(order.ChartID, String.Format("Вы внесли на счет {0} рублей", payment.withdraw_amount));
                    //---------
                    User CurrentUser = Helper.UserList.SingleOrDefault(p => p.ChartID == order.ChartID);
                    DateTime DatePay = CurrentUser.StopTime < DateTime.Now ? DateTime.Now : CurrentUser.StopTime;
                    CurrentUser.StopTime = DatePay + TimeSpan.FromDays(30);

                    var userDBCurrent = db.UserDBs.SingleOrDefault(p => p.UserName == CurrentUser.UserName);

                    if (userDBCurrent != null)
                        userDBCurrent.StopTime = CurrentUser.StopTime;
                    else
                        db.UserDBs.Add(CurrentUser.SetUserForDB());

                }
                order.Date = Convert.ToDateTime(payment.datetime);
                order.Amount = payment.amount;
                order.WithdrawAmount = payment.withdraw_amount;

                db.Orders.Add(order);
                await db.SaveChangesAsync();
               
            }
            return Ok();
        
        }
        

    }
}
