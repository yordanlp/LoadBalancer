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
        private readonly HttpClient _httpClient = new HttpClient();
        private const string HealthCheckEndpoint = "/_health";
        private readonly IConfiguration _configuration;
        private readonly string Region;
        private readonly IServiceScopeFactory _scopeFactory;

        public Ec2Service( IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            Region = configuration["Region"];
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

                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }
    }
}
