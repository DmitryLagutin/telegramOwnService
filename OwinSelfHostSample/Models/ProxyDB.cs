using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwinSelfHostSample.Models
{
    public class ProxyDB
    {
        public int Id { get; set; }
        public string IpAdress { get; set; }
        public string Port { get; set; }
        public string Login { get; set; }
        public string Pass { get; set; }
        public long ChartID { get; set; }

    }
}
