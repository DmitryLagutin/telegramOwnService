using Microsoft.Owin.Hosting;
using OwinSelfHostSample.Models;
using OwinSelfHostSample.MyContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace OwinSelfHostSample
{
    public class Program
    {
        public static Timer timerFollow = new Timer(90000);

        static void Main(string[] args)
        {

            if (Helper.UserList.Count == 0)
            {
                using (var db = new AppContext())
                {
                    if (db.UserDBs.Count() != 0)
                    {
                        foreach (var item in db.UserDBs.ToArray())
                        {
                            var user = item.GetUserFromBD();
                            Helper.UserList.Add(user);
                        }
                    }
                }
            }

            using (WebApp.Start<Startup>(url: "http://localhost:9000/"))
            {

                string path = "https://1fc6c50a.eu.ngrok.io";

                Helper.D = new Dictionary<string, string>();
                Helper.D.Add("/start", "0");
                Helper.D.Add("Главное меню", "0");

                Helper.D.Add("/Edit_Bot", "1");
                Helper.D.Add("Edit_Bot", "1");
                Helper.D.Add("/1/Edit_Bot", "11");
                Helper.D.Add("/2/Edit_Bot", "111");
                Helper.D.Add("/3/Edit_Bot", "1111");
                Helper.D.Add("Младше", "1111");
                Helper.D.Add("/3.1/Edit_Bot", "1112");
                Helper.D.Add("Старше", "1112");


                Helper.D.Add("/Start_Bot", "2");
                Helper.D.Add("Start_Bot", "2");


                Helper.D.Add("/Stop_Bot", "3");
                Helper.D.Add("Stop_Bot", "3");

                Helper.D.Add("/Payment", "4");
                Helper.D.Add("Payment", "4");

                Helper.D.Add("/Status", "5");
                Helper.D.Add("Status", "5");

                Helper.D.Add("Назад", "77777");


                Bot.Api.SetWebhookAsync(String.Format("{0}/api/Message/Update", path)).Wait();
                HttpClient client = new HttpClient();

                //timerFollow.Elapsed += async (send, e) => await FollowTimermethod();
                //timerFollow.Start();
                
                Task MyTask = Task.Factory.StartNew(MainAcync);

                //Console.ReadKey();

                while (true)
                {
                    string str = Console.ReadLine();
                    Console.WriteLine(str);
                }
            } 
        }

        public static void MainAcync()
        {
            timerFollow.Elapsed += async (send, e) => await FollowTimermethod();
            timerFollow.Start();
        }


        public static async Task FollowTimermethod()
        {       
             try
             {
                 if (Helper.UserList.ToArray().Count() != 0)
                 {
                     foreach (var user in Helper.UserList.ToArray())
                     {
                         Console.WriteLine(DateTime.Now);

                         if (user.IsMayWork)
                         {
                             if (user.Api.IsUserAuthenticated)
                                 await user.FollowUsers();
                             else
                                 await user.LogIn();
                         }
                     }
                 }
             }
             catch (Exception ex)
             {
                 Console.WriteLine(ex.Message);
             }
        }
    }
}
