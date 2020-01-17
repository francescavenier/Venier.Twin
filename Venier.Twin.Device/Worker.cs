using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Venier.Twin.Data;

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
                        _configuration.GetConnectionString("connectionString"),
                        TransportType.Amqp);

            // collegamento con twin
            var twin = await deviceClient.GetTwinAsync();
            var color = twin.Properties.Desired["color"];

            // imposto il colore alla console
            Console.BackgroundColor = color;
            Console.Clear();

            // leggere report
            var reported = new TwinCollection();
            reported["lastColor"] = Console.BackgroundColor.ToString();

            //aggiorno il il reported
            await deviceClient.UpdateReportedPropertiesAsync(reported);

            while (!stoppingToken.IsCancellationRequested)
            {
                var message = await deviceClient.ReceiveAsync(TimeSpan.FromSeconds(30));
                if (message != null)
                {
                    var text = System.Text.UTF8Encoding.UTF8.GetString(message.GetBytes());
                    var json = JsonConvert.DeserializeObject<MessageMap>(text);
                    Console.WriteLine("Messaggio: " + json.Text);

                    int textColorId = json.TextColorId;
                    Console.ForegroundColor = (ConsoleColor)textColorId;
                }
                else
                {
                    await Task.Delay(2000, stoppingToken);
                }
            }
        }
    }
}
