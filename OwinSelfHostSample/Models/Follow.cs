using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwinSelfHostSample.Models
{
    public class Follow
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public byte[] Listing { get; set; }
    }
}
