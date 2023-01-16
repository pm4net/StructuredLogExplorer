using Newtonsoft.Json;

namespace Client;

public class Urls
{
    public static string Json => JsonConvert.SerializeObject(new Urls());

    [JsonProperty] 
    public const string IndexUrl = "/";
    
    public const string ErrorUrl = "/error";
    
    [JsonProperty] 
    public const string AboutUrl = "/about";
    
    [JsonProperty] 
    public const string SpaUrl = "/spa";
    
    public const string NotFoundUrl = "/404";

    public const string ApiSegment = "/api";
}
