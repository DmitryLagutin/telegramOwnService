using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Classes.Models;
using OwinSelfHostSample.MyContext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OwinSelfHostSample.Models
{
    public class User
    {
        //при создании нового класса все свойства должны быть заподнены по умолчанию
        public IInstaApi Api { get; set; } = null;
        public string UserName { get; set; } = null;
        public string Password { get; set; } = null;
        public IList<string> Tags { get; set; }
        public long ChartID { get; set; }
        //----------------------------------------
        public bool IsMayWork { get; set; } = false;
        public bool IsUp { get; set; } = true;

        public int[] dd { get; set; } = new int[6] { 0, 0, 0, 0, 0, 0 };
        //---------------------------------------------------------
        public DateTime StartTime { get; set; } = DateTime.MinValue;  //время старта
        public DateTime StopTime { get; set; } = DateTime.MinValue;   //время стопа


        //Biulder
        public void Biueld()
        {
            var userSession = new UserSessionData { UserName = UserName, Password = Password };
            //Api = new InstaApiBuilder().SetUser(userSession).Build();
            

            using (var db = new AppContext())
            {
                var CurrentProxyDB = db.ProxyDBs.SingleOrDefault(p => p.ChartID == ChartID);
                if(CurrentProxyDB == null)
                {
                    var CurrentProxyDB1 = db.ProxyDBs.Where(p => p.ChartID == 0).FirstOrDefault();
                    if (CurrentProxyDB1 == null)
                    {
                        Helper.AddEvent("Внимание", ChartID.ToString(),  "Кончились прокси");
                    }
                    else
                    {
                        CurrentProxyDB1.ChartID = ChartID;
                        //--------------------------------
                        var httpHndler = new HttpClientHandler();
                        httpHndler.Proxy = new WebProxy(CurrentProxyDB1.IpAdress, Convert.ToInt32(CurrentProxyDB1.Port));
                        httpHndler.Proxy.Credentials = new NetworkCredential(CurrentProxyDB1.Login, CurrentProxyDB1.Pass);
                        HttpClient client = new HttpClient(httpHndler) { BaseAddress = new Uri(String.Format("http://{0}:{1}/", CurrentProxyDB1.IpAdress, Convert.ToInt32(CurrentProxyDB1.Port))) };
                        
                        Api = new InstaApiBuilder()
                        .SetUser(userSession)
                        .UseHttpClient(client)
                        .UseHttpClientHandler(httpHndler)
                        .Build();

                    }

                }
                else
                {


                    var httpHndler = new HttpClientHandler();
                    httpHndler.Proxy = new WebProxy(CurrentProxyDB.IpAdress, Convert.ToInt32(CurrentProxyDB.Port));
                    httpHndler.Proxy.Credentials = new NetworkCredential(CurrentProxyDB.Login, CurrentProxyDB.Pass);
                    HttpClient client = new HttpClient(httpHndler) { BaseAddress = new Uri(String.Format("http://{0}:{1}/", CurrentProxyDB.IpAdress, Convert.ToInt32(CurrentProxyDB.Port))) };

                    Api = new InstaApiBuilder()
                    .SetUser(userSession)
                    .UseHttpClient(client)
                    .UseHttpClientHandler(httpHndler)
                    .Build();
                    //Используем CurrentProxyDB
                }

                db.SaveChanges();
            }

        }

        //метод логирования
        public async Task LogIn()
        {
            if(Api == null)
            {
                Biueld();
            }

            if (Api.IsUserAuthenticated)
            {
                Helper.AddEvent("Событие", ChartID.ToString(), String.Format("Аккаунт {0} уже залогирован!!!", UserName));
            }
            else
            {
                var eee = await Api.LoginAsync();
                if (eee.Succeeded)
                    Helper.AddEvent("Событие", ChartID.ToString(), String.Format("Авторизация аккаунта {0} прошла успешно", UserName));
                else
                    Helper.AddEvent("Внимание", ChartID.ToString(), String.Format("При авторизации аккаунта {0} произошла ошибка: {1}", UserName, eee.Info.Message));  
            }
        }

        //метод ЗАДАЧИ добавление юзеров
        public async Task FollowUsers()
        {
            try
            {
                using (var db = new AppContext())
                {
                    var Follow = db.Follows.SingleOrDefault(p => p.UserName == UserName);
                    IList<string> UsersDB = new List<string>();
                    if (Follow != null)
                    {
                        UsersDB = (List<string>)Follow.Listing.DeseriaBinar();
                    }
                    else
                    {
                        db.Follows.Add(new Follow() { UserName = UserName, Listing = (new List<string>()).SeriaBinar() });
                        await db.SaveChangesAsync();
                        Follow = db.Follows.SingleOrDefault(p => p.UserName == UserName);
                    }
                    Console.WriteLine(UsersDB.Count); //!!!!!!!!!!!!!!!!!!! удалить потом
                    if (UsersDB.Count >= 50)
                        IsUp = false;
                    else if (UsersDB.Count == 0)
                        IsUp = true;
                      
                    if(IsUp == true)
                    {
                        string Pk_toFollow = String.Empty;
                        string UserName_toFollow = String.Empty;

                        foreach (var tag in Tags)
                        {
                            var resultTag = await Api.GetTagFeedAsync(tag, 3);
                            IList<InstaUserShort> Users = resultTag.Succeeded ? resultTag.Value.Medias.Select(p => p.User).Distinct().ToList() : new List<InstaUserShort>();

                            foreach (var user in Users)
                            {
                                if (UsersDB.SingleOrDefault(p => p == user.Pk) == null || UsersDB.Count == 0)
                                {
                                    Pk_toFollow = user.Pk;
                                    UserName_toFollow = user.UserName;
                                    break;
                                }
                                else
                                    break;
                            }
                            if (Pk_toFollow != String.Empty)
                                break;
                        }

                        Tags.Add(Tags.First());
                        Tags.Remove(Tags.First());

                        var resultAdd = await Api.FollowUserAsync(Convert.ToInt64(Pk_toFollow));

                        if (resultAdd.Succeeded)
                        {
                            Helper.AddEvent("Событие", ChartID.ToString(), String.Format("Мой юзер {0} добавил юзера {1} ({2}) + таг: {3}", UserName, UserName_toFollow, Pk_toFollow,  Tags.First()));
                            UsersDB.Add(Pk_toFollow);
                            Follow.Listing = UsersDB.SeriaBinar();
                            await db.SaveChangesAsync();
                        }
                        else
                            Helper.AddEvent("Внимание", ChartID.ToString(), String.Format("При добавлении аккаунта {0} произошла ошибка: {1}", UserName, resultAdd.Info.Message));

                        //-----------------------------
                        await Task.Delay(2000);        //--------@@@@@@@@@@@@@@@@@@@
                        //-----------------------------
                        //ЛАЙКИ
                        var mediaToLike = await Api.GetUserMediaAsync(UserName_toFollow, 3);
                        var mediaToLikeArray = mediaToLike.Value.ToArray();

                        if (mediaToLikeArray.Count() != 0)
                        {
                            var resultLike = await Api.LikeMediaAsync(mediaToLikeArray.First().InstaIdentifier);
                            if (resultLike.Succeeded)
                                Helper.AddEvent("Событие", ChartID.ToString(), String.Format("Мой пользователь {0} лайкнул {1} пользоваеля {2}", UserName, mediaToLikeArray.First().InstaIdentifier, UserName_toFollow));
                            //else
                            //    Helper.AddEvent("Событие", ChartID.ToString(), String.Format("При лайке аккаунта {0} произошла ошибка: {1}", UserName, resultLike.Info.Message));
                        }
                        else
                        {
                            Helper.AddEvent("Внимание", ChartID.ToString(), String.Format("У пользователя {0} нет медия в акаунте. Возможно это фейковый аккаунт", UserName_toFollow));
                        }

                    }
                    else
                    {
                        var followers = await Api.GetCurrentUserFollowersAsync();

                        string UnfollowUser = String.Empty;
                        foreach (var item1 in UsersDB.ToArray())
                        {
                            UnfollowUser = item1;
                            var result = await Api.UnFollowUserAsync(Convert.ToInt64(item1));
                            if (result.Succeeded)
                            {
                                Helper.AddEvent("Событие", ChartID.ToString(), String.Format("Мой юзер {0} отписался от {1}", UserName, item1));
                                UsersDB.Remove(item1);
                                Follow.Listing = UsersDB.SeriaBinar();
                                await db.SaveChangesAsync();
                            }
                            else
                                Helper.AddEvent("Внимание", ChartID.ToString(), String.Format("Во время отписки юзера {0} произошла ошибка: {1}", UserName, result.Info.Message));
                            break;
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Helper.AddEvent("Внимание", ChartID.ToString(), String.Format("Произошла ошибка в сценарии добавления/удалегия/лайка пользователя: " + ex.Message));
            }

        }
    }
}
