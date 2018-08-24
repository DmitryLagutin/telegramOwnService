using OwinSelfHostSample.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwinSelfHostSample.MyContext
{
    public class AppContext : DbContext
    {
        public AppContext() : base("DefaultConnection")
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<UserDB> UserDBs { get; set; }
        public DbSet<ProxyDB> ProxyDBs { get; set; }
        public DbSet<Follow> Follows { get; set; }
    }
}
