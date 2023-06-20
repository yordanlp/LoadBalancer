using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LoadBalancer.Services {
    public class HostService {
        private List<string> _instances = new List<string>();
        private int _currentIndex = 0;
        private readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private const string HealthCheckEndpoint = "/_health";
        private readonly IServiceScopeFactory _scopeFactory;

        public HostService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            UpdateHosts();
        }

        public string GetNextHost()
        {
            if (_instances.Count == 0)
                return null;

            if (_currentIndex >= _instances.Count)
                _currentIndex = 0;

            return _instances[_currentIndex++];
        }

        private async void UpdateHosts()
        {
            while (true)
            {
                try
                {
                    List<string> instances = new List<string>();

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        foreach(var instance in context.Instances)
                        {
                            if (await IsHealthy(instance.InternalHost))
                                instances.Add(instance.ExternalHost);
                        }
                    }

                    _instances.Clear();
                    _instances.AddRange(instances);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        private async Task<bool> IsHealthy(string host)
        {
            string content = "";
            HttpResponseMessage responseMessage = null;
            try
            {
                responseMessage = await _httpClient.GetAsync($"{host}{HealthCheckEndpoint}");
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
