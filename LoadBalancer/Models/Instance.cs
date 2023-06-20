using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoadBalancer.Models {
    public class Instance {
        public int Id { get; set; }
        public string InternalHost { get; set; }
        public string ExternalHost { get; set; }
    }
}
