using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Library.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //// create the webHost 
            //var webHost = CreateHostBuilder(args).Build();
            //// create a method that runs our migrations and creates the database
            //RunMigrations(webHost);
            //// run the webHost
            //webHost.Run();

            CreateHostBuilder(args).Build().Run();
        }

        //private static void RunMigrations(IHost webHost)
        //{
        //    using var scope = webHost.Services.CreateScope();
        //    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        //    db.Database.Migrate();
        //}

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
