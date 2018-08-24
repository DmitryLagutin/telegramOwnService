using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using OwinSelfHostSample.Models;
using Telegram.Bot.Types.Enums;
using System.Net.Http;
using Telegram.Bot.Types.InlineKeyboardButtons;

namespace OwinSelfHostSample.Models
{
    public static class MessageReciver
    {
        public static async Task MainMessage(this TelegramBotClient bot, Message msg, User user)
        {
            try
            {
                string xxx0 = msg.Text;
                if (xxx0.Contains("+"))
                {
                    xxx0 = "/1/Edit_Bot";
                }
                else if (xxx0.Contains("#"))
                {
                    xxx0 = "/2/Edit_Bot";
                }

                string xxx = xxx0.FromDictionary(Helper.D);
                user.dd = xxx.Hydra(user.dd);


                int x = user.dd[0];
                int y = user.dd[1];
                int z = user.dd[2];
                int w = user.dd[3];

                Console.WriteLine(String.Format("{0} {1} {2} {3}", x, y, z, w));

                switch (x)
                {
                    // /start
                    case 0:
                    default:
                        {
                            await bot.CaseStart_0(msg, user);
                            break;
                        }
                    // /Edit_Bot
                    case 1:
                        {
                            switch (y)
                            {
                                //Вопрос
                                default:
                                    {
                                        await bot.CaseEdit_1_Defualt(msg, user);
                                        break;
                                    }
                                //Ответ1
                                case 11:
                                    {
                                        switch (z)
                                        {
                                            //Вопрос
                                            default:
                                                {
                                                    await bot.CaseEdit_1_11_Default(msg, user);
                                                    break;
                                                }
                                            //Ответ1
                                            case 111:
                                                {
                                                    switch (w)
                                                    {
                                                        //Вопрос
                                                        default:
                                                            {
                                                                await bot.CaseEdit_1_11_111_Default(msg, user);
                                                                break;
                                                            }
                                                        //Ответ1
                                                        case 1111:
                                                            {
                                                                await bot.CaseEdit_1_11_111_1111(msg, user);
                                                                break;
                                                            }
                                                        //Ответ2
                                                        case 1112:
                                                            {
                                                                await bot.CaseEdit_1_11_111_1112(msg, user);
                                                                break;
                                                            }
                                                    }
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                    // /Start_Bot 
                    case 2:
                        {
                            await bot.CaseStartBot_2(msg, user);
                            break;
                        }
                    // /Stop_Bot
                    case 3:
                        {
                            await bot.CaseStopBot_3(msg, user);
                            break;
                        }
                    // /Payment
                    case 4:
                        {

                            await bot.CasePayment_4(msg, user);
                            break;
                        }
                    // /Statistic
                    case 5:
                        {
                            await bot.CaseStatistic_5(msg, user);
                            break;
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
