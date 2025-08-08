using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;

namespace Plumsail.DataSource.Dynamics365.CRM
{
    public class Patients(HttpClientProvider httpClientProvider, ILogger<Patients> logger)
    {
        [Function("D365-CRM-Patients")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "crm/cr174_patient/{id?}")] HttpRequest req, Guid? id)
        {
            logger.LogInformation("Dynamics365-CRM-Patients is requested.");

            try
            {
                var client = httpClientProvider.Create();

                if (!id.HasValue)
                {
                    var contactsJson = await client.GetStringAsync("cr174_patient?$select=cr174_psscid,info");
                    var contacts = JsonValue.Parse(contactsJson);
                    return new OkObjectResult(contacts?["value"]);
                }

                var contactResponse = await client.GetAsync($"cr174_patient({id})");
                if (!contactResponse.IsSuccessStatusCode)
                {
                    if (contactResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new NotFoundResult();
                    }

                    // throws Exception
                    contactResponse.EnsureSuccessStatusCode();
                }

                var contactJson = await contactResponse.Content.ReadAsStringAsync();
                return new ContentResult()
                {
                    Content = contactJson,
                    ContentType = "application/json"
                };
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "An error has occured while processing Dynamics365-CRM-Patients request.");
                return new StatusCodeResult(ex.StatusCode.HasValue ? (int)ex.StatusCode.Value : StatusCodes.Status500InternalServerError);
            }
        }
    }
}
