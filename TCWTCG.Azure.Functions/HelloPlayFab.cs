using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PlayFab;
using PlayFab.DataModels;
using System.Threading.Tasks;
using TCWTCG.Azure.PlayFab;

namespace TCWTCG.Azure.Functions
{
    public class HelloPlayFab
    {
        private readonly ILogger<HelloPlayFab> _logger;

        public HelloPlayFab(ILogger<HelloPlayFab> logger)
        {
            _logger = logger;
        }

        [Function("HelloPlayFab")]
        public async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            FunctionExecutionContext<dynamic> context = await req.ReadFromJsonAsync<FunctionExecutionContext<dynamic>>();

            dynamic args = context.FunctionArgument;

            var message = "Hello PlayFab!";
            //var message = $"Hello {(context.CallerEntityProfile.Lineage.MasterPlayerAccountId ?? "Unknown player")}!";
            _logger.LogInformation(message);

            dynamic inputValue = null;
            if (args != null && args["inputValue"] != null)
            {
                inputValue = args["inputValue"];
            }

            _logger.LogDebug($"HelloWorld: {new { input = inputValue }}");

            // The profile of the entity specified in the 'ExecuteEntityCloudScript' request.
            // Defaults to the authenticated entity in the X-EntityToken header.
            var entityProfile = context.CallerEntityProfile;

            var api = new PlayFabDataInstanceAPI(
                new PlayFabApiSettings
                {
                    TitleId = context.TitleAuthenticationContext.Id
                },
                new PlayFabAuthenticationContext
                {
                    EntityToken = context.TitleAuthenticationContext.EntityToken
                }
            );
            _ = await api.SetObjectsAsync(
                new SetObjectsRequest
                {
                    Entity = new EntityKey
                    {
                        Id = entityProfile.Entity.Id,
                        Type = entityProfile.Entity.Type
                    },
                    Objects = [
                    new() {
                        ObjectName =  "obj1",
                        DataObject = new
                        {
                            foo = "some server computed value",
                            prop1 = "bar"
                        }
                    }
                ]
                });

            return new { messageValue = message };
        }
    }
}