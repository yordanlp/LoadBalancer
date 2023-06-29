using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using LoadBalancer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LoadBalancer {
    public class Startup {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "LoadBalancer", Version = "v1" });
            });
            services.AddSingleton<HostService>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDbContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LoadBalancer v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            context.Database.Migrate();

            LoadConfigurationFromS3(context).Wait();
        }

        private async Task LoadConfigurationFromS3(AppDbContext context)
        {
            var s3Client = new AmazonS3Client(RegionEndpoint.EUCentral1);

            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = "coffee-shop-config",
                Key = "instances.json"
            };

            using GetObjectResponse response = await s3Client.GetObjectAsync(request);
            using Stream responseStream = response.ResponseStream;
            using StreamReader reader = new StreamReader(responseStream);
            string responseBody = reader.ReadToEnd();

            List<LoadBalancer.Models.Instance> configs = JsonConvert.DeserializeObject<List<LoadBalancer.Models.Instance>>(responseBody);

            foreach (var config in configs)
            {
                context.Instances.Add(config);
            }
            await context.SaveChangesAsync();
        }

    }
}
