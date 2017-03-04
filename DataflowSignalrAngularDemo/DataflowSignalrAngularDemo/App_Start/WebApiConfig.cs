using Newtonsoft.Json;
using System.Web.Http;

namespace ApiSignalrAngularHub
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new { id = RouteParameter.Optional });

            //JsonSerializerSettings serializerSettings = GlobalConfiguration.Configuration
            //.Formatters.JsonFormatter.SerializerSettings;
            //serializerSettings.TypeNameHandling = TypeNameHandling.Auto;
            config.Formatters.JsonFormatter.SerializerSettings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
        }
    }
}