using Amazon;
using Amazon.AutoScaling;
using Amazon.AutoScaling.Model;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LoadBalancer.Services {
    public class Ec2Service {
        private List<string> _ips = new List<string>();
        private int _currentIndex = 0;
        private AmazonAutoScalingClient _client;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string ScalingGroupName;
        private const string HealthCheckEndpoint = "/_health";
        private readonly AmazonEC2Client _ec2Client;
        private readonly IConfiguration _configuration;
        private readonly string Region;
        private readonly IServiceScopeFactory _scopeFactory;

        public Ec2Service( IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            Region = configuration["Region"];
            ScalingGroupName = configuration["AutoScalingGroupName"];
            AWSCredentials credentials = new BasicAWSCredentials(configuration["AWS_ACCESS_KEY"], configuration["AWS_SECRET_KEY"]);
            RegionEndpoint euNorth1 = RegionEndpoint.GetBySystemName(Region);
            _client = new AmazonAutoScalingClient(credentials, euNorth1);
            _ec2Client = new AmazonEC2Client(credentials, euNorth1);
            UpdateIPs();
        }

        public string GetNextIP()
        {
            if (_ips.Count == 0)
                return null;

            if (_currentIndex >= _ips.Count)
                _currentIndex = 0;

            return _ips[_currentIndex++];
        }

        private async void UpdateIPs()
        {
            while (true)
            {
                try
                {
                    IEnumerable<string> dbInstances = new List<string>();

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        dbInstances = context.Instances.Select(i => i.ip).Distinct();
                    }

                    _ips.Clear();
                    _ips.AddRange(dbInstances);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                // Sleep for a while before updating again
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }
    }
}
