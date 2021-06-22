using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using AdvertAPI.Services;
using AdvertAPI.HealthChecks;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Models;
using Amazon.Util;
using Amazon.ServiceDiscovery;
using Amazon.ServiceDiscovery.Model;

namespace AdvertAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));
            services.AddTransient<IAdvertStorageService, DynamoDBAdvertStorage>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddHealthChecks().AddCheck<StorageHealthCheck>("Storage");
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Web Advertisement Apis",
                    Version = "version 1",
                    Contact = new OpenApiContact
                    {
                        Name = "Adrita Sharma",
                        Email = "adritasharma@gmail.com"
                    }
                });
            });

            //services.AddHealthChecks(checks =>
            //{
            //    checks.AddCheck<StorageHealthCheck>("Storage", new System.TimeSpan(0, 1, 0);
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async Task Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseHealthChecks("/health");
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web Advert Api");
            });
            app.UseStaticFiles();

            await RegisterToCloudMap();

            app.UseMvc();
        }

        public async Task RegisterToCloudMap()
        {
            string serviceId = Configuration.GetValue<string>("CloudMapNamespaceSeviceId");

            var instanceId = EC2InstanceMetadata.InstanceId;
            if(!string.IsNullOrEmpty(instanceId))
            {
                var ipv4 = EC2InstanceMetadata.PrivateIpAddress;

                var client = new AmazonServiceDiscoveryClient();

                await client.RegisterInstanceAsync(new RegisterInstanceRequest()
                {
                    InstanceId = instanceId,
                    ServiceId = serviceId,
                    Attributes = new Dictionary<string, string>()
                    {
                        {"AWS_INSTANCE_IPV4", ipv4 },
                        {"AWS_INSTANCE_PORT", "80" },
                    }
                });
            }

        }
    }
}
