using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using OwinSelfHostSample.MyContext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;

namespace OwinSelfHostSample.Models
{
    public static class Helper
    {       
        public static IList<string> Tags = new List<string>() { "msk", "spb", "london" };
        public static IList<User> UserList = new List<User>();
        public static IDictionary<string, string> D = new Dictionary<string, string>();
        //---------------------------------------------------------------------------------------------------

        //Записать в лог
        public static void WrFileAdd(string path, string str, params object[] args)
        {
            string dataString = String.Format(str, args);
            FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(dataString);
            sw.Flush();
            sw.Close();

        }

        //Сериализация в байты
        public static byte[] SeriaBinar(this Object obj)
        {
            byte[] bytes;
            IFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                bytes = stream.ToArray();
            }
            return bytes;
        }

        //Десериализация
        public static object DeseriaBinar(this byte[] buffer)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (MemoryStream stream = new MemoryStream(buffer))
            {
                object newPerson = formatter.Deserialize(stream);
                return newPerson;
            }
        }

        //Асинхронная сериализация
        public static Task<byte[]> SeriaBinarAsync(this Object obj)
        {
            return Task.Run(() =>
            {
                byte[] bytes;
                IFormatter formatter = new BinaryFormatter();
                using (MemoryStream stream = new MemoryStream())
                {
                    formatter.Serialize(stream, obj);
                    bytes = stream.ToArray();
                }
                return bytes;
            });
        }

        //Асинхронная десериализация
        public static Task<object> DeseriaBinarAsync(this byte[] buffer)
        {
            return Task.Run(() =>
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (MemoryStream stream = new MemoryStream(buffer))
                {
                    object newPerson = formatter.Deserialize(stream);
                    return newPerson;
                }
            });
        }


        //-----------------------------------
        public static void AddEvent(string message, string ChartId,  string message1)
        {
            string str = String.Format("{0} | {1} | {2} | {3}", DateTime.Now, message, ChartId,  message1);
            Console.WriteLine(str);
            WrFileAdd(@"..\..\log.txt", str);

        }

        //------------------------------------
 
        public static int[] Hydra(this string xxx, int[] inputArr)
        {
            if (xxx == "Ex")
            {

            }
            else
            {
                int x = Convert.ToInt32(xxx);

                if (xxx.Length == 1)
                {
                    inputArr = new int[6] { 0, 0, 0, 0, 0, 0 };
                    inputArr[0] = x;
                }
                else if (xxx.Length == 2)
                {
                    inputArr[1] = x;
                }
                else if (xxx.Length == 3)
                {
                    inputArr[2] = x;
                }
                else if (xxx.Length == 4)
                {
                    inputArr[3] = x;
                }
                else if (xxx.Length == 5)
                {
                    if(inputArr[0] == 0)
                    {
                        inputArr[0] = 0;
                    }
                    else
                    {
                        for (int i = 0; i < inputArr.Length; i++)
                        {
                            if (inputArr[i] == 0)
                            {
                                inputArr[i - 1] = 0;
                                break;
                            }
                        }
                    }

                    

                    Console.WriteLine(String.Format("------ {0} {1} {2} {3} {4}", inputArr[0], inputArr[1], inputArr[2], inputArr[3], inputArr[4]));
                }
            }


            return inputArr;
        }

        public static string FromDictionary(this string str, IDictionary<string, string> D)
        {
            string ddd1 = String.Empty;
            var ddd = D.TryGetValue(str, out ddd1);

            if (ddd == true)
            {
                return ddd1;
            }
            else
            {
                return "Ex";
            }
        }
 
        //-----------------------------------

        public static User GetUserFromBD(this UserDB userDB)
        {
            IInstaApi Api = null;
            using (var db = new AppContext())
            {
                var CurrentProxyDB = db.ProxyDBs.SingleOrDefault(p => p.ChartID == userDB.ChartID);

                if (userDB.UserName != null)
                {
                    var userSession = new UserSessionData { UserName = userDB.UserName, Password = userDB.Password };
                    var httpHndler = new HttpClientHandler();
                    httpHndler.Proxy = new WebProxy(CurrentProxyDB.IpAdress, Convert.ToInt32(CurrentProxyDB.Port));
                    httpHndler.Proxy.Credentials = new NetworkCredential(CurrentProxyDB.Login, CurrentProxyDB.Pass);
                    HttpClient client = new HttpClient(httpHndler) { BaseAddress = new Uri(String.Format("http://{0}:{1}/", CurrentProxyDB.IpAdress, Convert.ToInt32(CurrentProxyDB.Port))) };

                    Api = new InstaApiBuilder()
                    .SetUser(userSession)
                    .UseHttpClient(client)
                    .UseHttpClientHandler(httpHndler)
                    .Build();
                }

            }

               
            return new User()
            {
                Api = Api,
                UserName = userDB.UserName,
                Password = userDB.Password,
                ChartID = userDB.ChartID,
                IsMayWork = userDB.IsMayWork,
                StartTime = userDB.StartTime,
                StopTime = userDB.StopTime,
                dd = new int[6] { 0, 0, 0, 0, 0, 0 },
                Tags = (List<string>)userDB.Tags.DeseriaBinar()
                
            };

        }

        public static UserDB SetUserForDB(this User user)
        {
            return new UserDB()
            {
                UserName = user.UserName,
                Password = user.Password,
                ChartID = user.ChartID,
                IsMayWork = user.IsMayWork,
                StartTime = user.StartTime,
                StopTime = user.StopTime,
                Tags = user.Tags.SeriaBinar()
            };
        }

        public static async void SaveUserInMain(this User user)
        {
            using (var db = new AppContext())
            {
                var userDBCurrent = db.UserDBs.SingleOrDefault(p => p.UserName == user.UserName);

                if (userDBCurrent != null)
                {
                    userDBCurrent.UserName = user.UserName;
                    userDBCurrent.Password = user.Password;
                    userDBCurrent.ChartID = user.ChartID;
                    userDBCurrent.IsMayWork = user.IsMayWork;
                    userDBCurrent.StartTime = user.StartTime;
                    userDBCurrent.StopTime = user.StopTime;
                    userDBCurrent.Tags = user.Tags.SeriaBinar();
                }
                else
                    db.UserDBs.Add(user.SetUserForDB());

                await db.SaveChangesAsync();
            }
        }

        //=======================================================

        public static async Task CaseStart_0(this TelegramBotClient bot, Message msg, User user)
        {
            var keyboard = new ReplyKeyboardMarkup() { ResizeKeyboard = true };
            keyboard.Keyboard = new[]
            {
                            new KeyboardButton[]
                            {
                                new KeyboardButton("Edit_Bot"),
                                new KeyboardButton("Start_Bot")
                            },
                             new KeyboardButton[]
                            {
                                new KeyboardButton("Stop_Bot"),
                                new KeyboardButton("Payment")
                            },
                              new KeyboardButton[]
                            {
                                new KeyboardButton("Status")
                            },
                        };

            await bot.SendTextMessageAsync(msg.Chat.Id, "/start\t - таблица команд\n\n/Edit_Bot\t - настроить бота\n\n/Start_Bot\t - запустить бота\n\n/Stop_Bot\t - остановить бота\n\n/Payment\t - оплатить\n\n/Status\t - состояние бота", replyMarkup: keyboard);
        }

        public static async Task CaseEdit_1_Defualt(this TelegramBotClient bot, Message msg, User user)
        {
            var keyboard = new ReplyKeyboardMarkup() { ResizeKeyboard = true };
            keyboard.Keyboard = new[]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Назад")
                }
            };
            //----------------------------
            var ss = await bot.SendTextMessageAsync(msg.Chat.Id, "<strong>Введите логин и пароль аккаунта\nв таком виде(через знак +):</strong>\n<pre>login+password</pre>", parseMode: ParseMode.Html, replyMarkup: keyboard);
            Console.WriteLine(String.Format("ID чата {0}, ID сообщения {1}, ID ответа {2}", msg.Chat.Id, msg.MessageId, ss.MessageId));

        }
        //!!!!!   СБОР
        public static async Task CaseEdit_1_11_Default(this TelegramBotClient bot, Message msg, User user)
        {
            var keyboard = new ReplyKeyboardMarkup() { ResizeKeyboard = true };
            keyboard.Keyboard = new[]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Назад")
                }
            };
            //------------------------------------
            if (msg.Text.Contains("+"))
            {
                string[] separatingChars = { "+" };
                string[] a = msg.Text.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);
                using (var db = new AppContext())
                {
                    
                    if (user.Api != null)
                    {

                        var CurrentUserDB = db.UserDBs.SingleOrDefault(p => p.UserName == user.UserName);
                        await bot.SendTextMessageAsync(msg.Chat.Id, String.Format("Работа над аккаунтом {0}\nостановлена\n<strong>Вы ввели:</strong>\nlogin:\t\t\t\t\t<strong>{1}</strong>\npassword:\t\t\t\t\t<strong>{2}</strong>\n<strong>Введите хештеги, по которым бот будет Вас продвигать(до 200 хештегов)\nв таком виде(через знак #):</strong>\n<pre>#tag1#tag2#tag3#...#tagN</pre>", user.UserName, a[0], a[1]), parseMode: ParseMode.Html, replyMarkup: keyboard);
                        user.IsMayWork = false;
                        user.UserName = a[0];
                        user.Password = a[1];
                        //если новый аккаунт !!!!!!!!!!!!!!!!!!!!!!
                        if (user.StartTime == DateTime.MinValue)
                        {
                            user.StartTime = DateTime.Now;
                            user.StopTime = user.StartTime + TimeSpan.FromHours(12); // DEMO ПЕРИОД           
                        }

                        CurrentUserDB.IsMayWork = user.IsMayWork;
                        CurrentUserDB.UserName = user.UserName;
                        CurrentUserDB.Password = user.Password;
                        CurrentUserDB.StartTime = user.StartTime;
                        CurrentUserDB.StopTime = user.StopTime;

                        await db.SaveChangesAsync();

                    }
                    else
                    {
                        user.UserName = a[0];
                        user.Password = a[1];
                        user.ChartID = msg.Chat.Id;
                        //если новый аккаунт !!!!!!!!!!!!!!!!!!!!!!
                        if (user.StartTime == DateTime.MinValue)
                        {
                            user.StartTime = DateTime.Now;
                            user.StopTime = user.StartTime + TimeSpan.FromHours(12); // DEMO ПЕРИОД           
                        }
                        user.Tags = new List<string>() { "msk" };
                        db.UserDBs.Add(user.SetUserForDB());
                        await db.SaveChangesAsync();
                        await bot.SendTextMessageAsync(msg.Chat.Id, String.Format("<strong>Вы ввели:</strong>\nlogin:\t\t\t\t\t<strong>{0}</strong>\npassword:\t\t\t\t\t<strong>{1}</strong>\n<strong>Введите хештеги, по которым бот будет Вас продвигать(до 200 хештегов)\nв таком виде(через знак #):</strong>\n<pre>#tag1#tag2#tag3#...#tagN</pre>", user.UserName, user.Password), parseMode: ParseMode.Html, replyMarkup: keyboard);
                    }
                }
            }
            //!!!!!   СБОР
            user.Biueld();
            
        }

        public static async Task CaseEdit_1_11_111_Default(this TelegramBotClient bot, Message msg, User user)
        {
            if (msg.Text.Contains("#"))
            {
                string[] separatingChars = { "#", " ", ", " };
                string[] a = msg.Text.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);
                user.Tags = a.ToList();
                user.Tags = a.ToList().Count < 3 ? new List<string>() { "moscow", "msk", "spb" } : a.ToList();
            }

            var keyboard = new ReplyKeyboardMarkup() { ResizeKeyboard = true };
            keyboard.Keyboard = new[]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Младше"),
                    new KeyboardButton("Старше")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Назад")
                }
            };

            await bot.SendTextMessageAsync(msg.Chat.Id, String.Format("<strong>Вы ввели {0} хештега(-ов)\nУкажите, старше ли ваш аккаунт 1 года?</strong>\n<pre>Да/Нет</pre>", user.Tags.Count), parseMode: ParseMode.Html, replyMarkup: keyboard);

        }

        public static async Task CaseEdit_1_11_111_1111(this TelegramBotClient bot, Message msg, User user)
        {
            var keyboard = new ReplyKeyboardMarkup() { ResizeKeyboard = true };
            keyboard.Keyboard = new[]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Start_Bot"),
                    new KeyboardButton("Главное меню")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Назад")
                }
            };

            await bot.SendTextMessageAsync(msg.Chat.Id, "Все готово. Все будет сделано по лучщему разряду", replyMarkup: keyboard);

        }

        public static async Task CaseEdit_1_11_111_1112(this TelegramBotClient bot, Message msg, User user)
        {
            await bot.SendTextMessageAsync(msg.Chat.Id, "/start");

        }

        public static async Task CaseStartBot_2(this TelegramBotClient bot, Message msg, User user)
        {
            if(user.Api != null)
            {
                if (user.IsMayWork == false)
                {
                    await user.LogIn();
                    if (user.Api.IsUserAuthenticated)
                    {
                        
                        //Важная строка кода!!!!!!!!!!!!
                        user.IsMayWork = true;


                        var keyboard = new ReplyKeyboardMarkup() { ResizeKeyboard = true };
                        keyboard.Keyboard = new[]
                        {
                    new KeyboardButton[]
                    {
                        new KeyboardButton("Stop_Bot"),
                        new KeyboardButton("Главное меню")
                    }
                };

                        await bot.SendTextMessageAsync(msg.Chat.Id, "Бот успешно запущен", replyMarkup: keyboard);

                    }
                    else
                    {
                        var keyboard = new ReplyKeyboardMarkup() { ResizeKeyboard = true };
                        keyboard.Keyboard = new[]
                        {
                         new KeyboardButton[]
                         {
                             new KeyboardButton("Главное меню")
                         }
                    };

                        await bot.SendTextMessageAsync(msg.Chat.Id, "Что то пошло не так.\nВам необходимо:\n- Проверить правильность логина и пароля;\n- В приложении Instagram подтвердить что это вы;\nПосле этого поробовать еще раз", replyMarkup: keyboard);
                    }

                    user.SaveUserInMain();
                }
                else
                {
                    await bot.SendTextMessageAsync(msg.Chat.Id, "Ваш бот УЖЕ В РАБОТЕ");
                }
            }
            else
            {
                var keyboard = new ReplyKeyboardMarkup() { ResizeKeyboard = true };
                keyboard.Keyboard = new[]
                {
                            new KeyboardButton[]
                            {
                                new KeyboardButton("Edit_Bot"),
                                new KeyboardButton("Start_Bot")
                            },
                             new KeyboardButton[]
                            {
                                new KeyboardButton("Stop_Bot"),
                                new KeyboardButton("Payment")
                            },
                              new KeyboardButton[]
                            {
                                new KeyboardButton("Status")
                            },
                        };

                await bot.SendTextMessageAsync(msg.Chat.Id, "Вам необходимо настроить бота перед ЗАПУСКОМ", replyMarkup: keyboard);
            }

             

        }

        public static async Task CaseStopBot_3(this TelegramBotClient bot, Message msg, User user)
        {

            if(user.IsMayWork == true)
            {
                user.IsMayWork = false;
                Console.WriteLine("Бот отсновлен");

                var keyboard = new ReplyKeyboardMarkup() { ResizeKeyboard = true };
                keyboard.Keyboard = new[]
                {
                new KeyboardButton[]
                {
                        new KeyboardButton("Start_Bot"),
                        new KeyboardButton("Главное меню")
                    }
                };

                await bot.SendTextMessageAsync(msg.Chat.Id, "Бот успешно остановлен", replyMarkup: keyboard);

                user.SaveUserInMain();
            }
            else
            {
                await bot.SendTextMessageAsync(msg.Chat.Id, "Бот и так НЕ В РАБОТЕ");
            }
           

        }

        public static async Task CasePayment_4(this TelegramBotClient bot, Message msg, User user)
        {
            if(user.Api != null)
            {
                HttpClient client = new HttpClient();
                var values = new Dictionary<string, string>
                {
                   { "receiver", "410012611988533" },
                   { "label", String.Format("{0}+{1}", user.UserName, msg.Chat.Id)},
                   { "quickpay-form", "shop" },
                   { "sum", "10" },
                   { "paymentType", "AC" },
                   { "targets", String.Format("Транзакция №{0} от {1}", user.ChartID, DateTime.Now.ToString()) },
                   { "successURL", "https://yandex.ru/support/direct/efficiency/vcards.html#vcards__edit" },
                };

                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync("https://money.yandex.ru/quickpay/confirm.xml", content);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var keyboard = new InlineKeyboardMarkup();

                    keyboard.InlineKeyboard = new[]
                    {
                     new InlineKeyboardButton[]
                     {
                         new InlineKeyboardUrlButton("Оплатить 300 руб.", response.RequestMessage.RequestUri.AbsoluteUri),
                     },
                };

                    DateTime datePay = user.StopTime < DateTime.Now ? DateTime.Now + TimeSpan.FromDays(30) : user.StopTime + TimeSpan.FromDays(30);
                    string str = String.Format("<strong>Платеж:</strong>\nОплата работы бота до:\n{0}\n+ 30 дней", datePay.ToString());

                    await bot.SendTextMessageAsync(msg.Chat.Id, str, replyMarkup: keyboard, parseMode: ParseMode.Html);
                }
            }
            else
            {
                var keyboard = new ReplyKeyboardMarkup() { ResizeKeyboard = true };
                keyboard.Keyboard = new[]
                {
                            new KeyboardButton[]
                            {
                                new KeyboardButton("Edit_Bot"),
                                new KeyboardButton("Start_Bot")
                            },
                             new KeyboardButton[]
                            {
                                new KeyboardButton("Stop_Bot"),
                                new KeyboardButton("Payment")
                            },
                              new KeyboardButton[]
                            {
                                new KeyboardButton("Status")
                            },
                        };

                await bot.SendTextMessageAsync(msg.Chat.Id, "Вам необходимо настроить бота перед ОПЛАТОЙ", replyMarkup: keyboard);
            }
           

        }

        public static async Task CaseStatistic_5(this TelegramBotClient bot, Message msg, User user)
        {
            if (user.Api != null)
            {
                IList<string> tags = user.Tags;

                string yyy = String.Empty;
                if (tags.Count == 0)
                {
                    yyy = "нет тегов";
                }
                else if (tags.Count <= 5)
                {
                    foreach (var item in tags)
                    {
                        yyy = yyy + "\n#" + item;
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        yyy = yyy + "\n#" + tags[i];
                    }
                    yyy = yyy + String.Format(" + еще {0}", tags.Count - 5);
                }

                string status = user.IsMayWork == true ? "работает" : "не работает";
                string LastPay = "Демо режим до\nОплатите продолжение работы";


                await bot.SendTextMessageAsync(msg.Chat.Id, String.Format("<strong>Ваш аккаунт:</strong>\nUserName: {0}\nTags: {1}\nStatus: {2}\nВремя начала:\n{3}\nОплачено до:\n{4}\nПоследний платеж:\n{5}", user.UserName, yyy, status, user.StartTime, user.StopTime, LastPay), parseMode: ParseMode.Html);
            }
            else
            {
                await bot.SendTextMessageAsync(msg.Chat.Id, "Вы пока не подключили аккаунта");
            }

        }

    }
}

