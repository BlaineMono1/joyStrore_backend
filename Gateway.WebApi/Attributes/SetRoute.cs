using Microsoft.AspNetCore.Mvc;

namespace Gateway.WebApi.Attributes
{
    public class SetRoute : RouteAttribute
    {
        public SetRoute(string template) : base(AddPrefixToRoute(template))
        {

        }
        public static string AddPrefixToRoute(string template)
        {

            var configeraion = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var prefix = configeraion["RoutePrefix:RouteTracker.WebApi"];
            return $"{prefix}/{template}";
        }
    }
}
