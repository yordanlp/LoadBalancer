using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoadBalancer.Models {
    public class Instance {
        public int Id { get; set; }
        public string host { get; set; }
    }
}
