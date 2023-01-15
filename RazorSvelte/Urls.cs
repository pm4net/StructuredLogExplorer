using Newtonsoft.Json;

namespace RazorSvelte;

public class Urls
{
    [JsonProperty] 
    public const string IndexUrl = "/";
    
    public const string ErrorUrl = "/error";
    
    [JsonProperty] 
    public const string AboutUrl = "/about";
    
    [JsonProperty] 
    public const string SpaUrl = "/spa";
    
    public const string NotFoundUrl = "/404";

    public const string ApiSegment = "/api";

    public static string Json { get; private set; }

    static Urls()
    {
        Json = JsonConvert.SerializeObject(new Urls());
    }
}
