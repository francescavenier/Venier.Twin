using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Venier.Twin.WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;

        public string[] Devices = new string[] { "device1", "device2" };

        [TempData]
        public string Message { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string DeviceId { get; set; }

            [Required]
            public string Text { get; set; }
        }


        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public void OnGet()
        {



        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                var serviceClient = ServiceClient.CreateFromConnectionString(
                                        _configuration.GetConnectionString("iothub"),
                                        TransportType.Amqp);
                var text = System.Text.UTF8Encoding.UTF8.GetBytes(Input.Text);
                var message = new Message(text);

                await serviceClient.SendAsync(Input.DeviceId, message);

                Message = $"Messaggio inviato al dispositivo {Input.DeviceId} con successo";

                return RedirectToPage();
            }


            //var registry = RegistryManager.CreateFromConnectionString(_configuration.GetConnectionString("iothub"));
            //var twin = await registry.GetTwinAsync("test1");
            //twin.Properties.Desired["temperatura"] = 50;
            //var status = twin.Properties.Reported["status"] as string;
            //await registry.UpdateTwinAsync("test1", twin, twin.ETag);

            return Page();
        }
    }
}
