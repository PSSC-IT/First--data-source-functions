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
                    var patientsJson = await client.GetStringAsync("cr174_patient?$select=cr174_psscid,info");
                    var patients = JsonValue.Parse(patientsJson);
                    return new OkObjectResult(patients?["value"]);
                }

                var patientResponse = await client.GetAsync($"cr174_patient({id})");
                if (!patientResponse.IsSuccessStatusCode)
                {
                    if (patientResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new NotFoundResult();
                    }

                    // throws Exception
                    patientResponse.EnsureSuccessStatusCode();
                }

                var patientJson = await contactResponse.Content.ReadAsStringAsync();
                return new ContentResult()
                {
                    Content = patientJson,
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
