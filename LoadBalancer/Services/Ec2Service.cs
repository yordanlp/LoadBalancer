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
        private readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private const string HealthCheckEndpoint = "/_health";
        private readonly IServiceScopeFactory _scopeFactory;

        public Ec2Service(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
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
                    List<string> dbInstances = new List<string>();

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var ips = context.Instances.Select(i => i.ip).Distinct().ToList();
                        foreach(var ip in ips)
                        {
                            if (await IsHealthy(ip))
                                dbInstances.Add(ip);
                        }
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

        private async Task<bool> IsHealthy(string ip)
        {
            string content = "";
            HttpResponseMessage responseMessage = null;
            try
            {
                responseMessage = await _httpClient.GetAsync($"http://{ip}{HealthCheckEndpoint}");
                content = await responseMessage.Content.ReadAsStringAsync();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            
            return responseMessage.IsSuccessStatusCode && content == "Healthy";
        }
    }
}
