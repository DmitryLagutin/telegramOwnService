using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwinSelfHostSample.Models
{
    public class UserDB
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public byte[] Tags { get; set; } = null;
        public long ChartID { get; set; }
        //----------------------------------------
        public bool IsMayWork { get; set; } = false;
        //---------------------------------------------------------
        public DateTime StartTime { get; set; } = DateTime.MinValue;  //время старта
        public DateTime StopTime { get; set; } = DateTime.MinValue;   //время стопа

    }
}
