using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwinSelfHostSample.Models
{
    public class PaymentModel
    {
        public string operation_id { get; set; } = String.Empty;
        public string notification_type { get; set; } = String.Empty;
        public string datetime { get; set; } = String.Empty;
        public string label { get; set; } = String.Empty;
        public decimal amount { get; set; } = 0;
        public decimal withdraw_amount { get; set; } = 0;
        public bool codepro { get; set; } = false;
        public string currency { get; set; } = String.Empty;
        public string sha1_hash { get; set; } = String.Empty;
        public string sender { get; set; } = String.Empty;
    }
}
