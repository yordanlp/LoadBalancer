﻿using Amazon;
using Amazon.AutoScaling;
using Amazon.AutoScaling.Model;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Runtime;
using LoadBalancer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LoadBalancer.Controllers {
    [ApiController]
    public class LoadBalancerController : ControllerBase {

        private readonly Ec2Service _ec2Service;
        private readonly IConfiguration _configuration;

        public LoadBalancerController( Ec2Service ec2Service, IConfiguration configuration )
        {
            _ec2Service = ec2Service;
            _configuration = configuration;
        }

        [Route("{*url}", Order = 999)]
        public IActionResult CatchAll(string url)
        {
            var ip = _ec2Service.GetNextIP();
            if (ip == null)
                return StatusCode(StatusCodes.Status503ServiceUnavailable);

            return Redirect($"http://{ip}/{url}");
        }

    }
}
