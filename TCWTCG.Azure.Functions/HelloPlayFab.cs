using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PlayFab;
using PlayFab.DataModels;

namespace TCWTCG.Azure.Functions
{
    public class HelloPlayFab
    {
        private readonly ILogger<HelloPlayFab> _logger;

        [Function("HelloPlayFab")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger<HelloPlayFab> logger)
        {
            FunctionExecutionContext<dynamic> context = await req.ReadFromJsonAsync<FunctionExecutionContext<dynamic>>();

            dynamic args = context.FunctionArgument;

            var message = $"Hello {context.CallerEntityProfile.Lineage.MasterPlayerAccountId}!";
            logger.LogInformation(message);

            dynamic inputValue = null;
            if (args != null && args["inputValue"] != null)
            {
                inputValue = args["inputValue"];
            }

            logger.LogDebug($"HelloWorld: {new { input = inputValue }}");

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

            var apiResult = await api.SetObjectsAsync(
                new SetObjectsRequest
                {
                    Entity = new EntityKey
                    {
                        Id = entityProfile.Entity.Id,
                        Type = entityProfile.Entity.Type
                    },
                    Objects = new List<SetObject> {
                    new SetObject
                    {
                        ObjectName =  "obj1",
                        DataObject = new
                        {
                            foo = "some server computed value",
                            prop1 = "bar"
                        }
                    }
                }
                });

            return new { messageValue = message };
        }
    }
}