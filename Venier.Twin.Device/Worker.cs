using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Venier.Twin.Device
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;


        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Start device: {time}", DateTimeOffset.Now);

            var deviceClient =
                    DeviceClient.CreateFromConnectionString(
                        _configuration.GetConnectionString("iothub"),
                        TransportType.Amqp);

            //var twin = await deviceClient.GetTwinAsync();
            //var color = twin.Properties.Desired["color"];
            //var x = (int)ConsoleColor.Yellow;
            //Console.BackgroundColor = (ConsoleColor)14;
            //twin.Properties.Reported["status"] = "ok";
            //twin.Properties.Reported["dateChancheStatus"] = DateTime.Now.ToString();
            //await deviceClient.UpdateReportedPropertiesAsync(twin.Properties.Reported);

            while (!stoppingToken.IsCancellationRequested)
            {
                var message = await deviceClient.ReceiveAsync(TimeSpan.FromSeconds(30));
                if (message != null)
                {
                    var text = System.Text.UTF8Encoding.UTF8.GetString(message.GetBytes());
                    Console.WriteLine("Messaggio: " + text);
                }
                else
                {
                    await Task.Delay(2000, stoppingToken);
                }
            }
        }
    }
}
