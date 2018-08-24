using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwinSelfHostSample.Models
{
    public class Order
    {
        public int Id { get; set; } //ID необходимо для Entety Framework
        public string UserName { get; set; } = string.Empty;// id пользователя в системе, который сделал заказ
        public long ChartID { get; set; } = 0;   // id чата
        public string Operation_Id { get; set; } // id операции в ЯД
        public DateTime Date { get; set; } // дата
        public decimal Amount { get; set; } // сумма, которую заплатали с учетом комиссии
        public decimal WithdrawAmount { get; set; } // сумма, которую заплатали без учета комиссии
    }
}
