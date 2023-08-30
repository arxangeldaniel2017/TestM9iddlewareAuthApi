namespace TestM9iddlewareAuthApi.Middleware
{
    public interface IAppSettings
    {
        string GisApiUserName { get; set; }
        string GisApiPassword { get; set; }
        string GisApiTokenUrl { get; set; }
        string GisApiKklUrl { get; set; }
        string GisApiEsriUrl { get; set; }
    }

    public class AppSettings : IAppSettings
    {
        public string GisApiUserName { get; set; }
        public string GisApiPassword { get; set; }
        public string GisApiTokenUrl { get; set; }
        public string GisApiKklUrl { get; set; }
        public string GisApiEsriUrl { get; set; }
    }
}