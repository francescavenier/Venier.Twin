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
using Newtonsoft.Json;
using Venier.Twin.Data;

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

            [Required]
            public int ColorId { get; set; }

            [Required]
            public int TextColorId { get; set; }
        }


        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public void OnGet()
        {

        }

        // Scelta del colore
        public async Task<IActionResult> OnPost()
        {
            var registry = RegistryManager.CreateFromConnectionString(_configuration.GetConnectionString("connectionString"));
            var twin = await registry.GetTwinAsync(Input.DeviceId);
            twin.Properties.Desired["color"] = Input.ColorId;
 
            await registry.UpdateTwinAsync("device1", twin, twin.ETag);
            return RedirectToPage();
        }

        // Invio del messaggio
        public async Task<IActionResult> OnPostMessage()
        {
            if (ModelState.IsValid)
            {
                var serviceClient = ServiceClient.CreateFromConnectionString(
                                        _configuration.GetConnectionString("connectionString"),
                                        TransportType.Amqp);
                var temp = new MessageMap();

                temp.DeviceId = Input.DeviceId; 
                temp.TextColorId = Input.TextColorId; 
                temp.Text = Input.Text; 


                var json = JsonConvert.SerializeObject(temp);

                var text = System.Text.UTF8Encoding.UTF8.GetBytes(Input.Text);
                var message = new Message(text);

                await serviceClient.SendAsync(Input.DeviceId, message);

                Message = $"Messaggio inviato al dispositivo {Input.DeviceId} con successo";

                return RedirectToPage();
            }
            return Page();
        }
    }
}
