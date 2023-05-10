using Newtonsoft.Json;

namespace StructuredLogExplorer;

public class Urls
{
    public static string Json => JsonConvert.SerializeObject(new Urls());

    [JsonProperty] 
    public const string HomeUrl = "/";

    [JsonProperty]
    public const string FilesUrl = "/files";

    [JsonProperty]
    public const string ObjectsUrl = "/objects";

    [JsonProperty]
    public const string MapUrl = "/map";
    
    [JsonProperty]
    public const string StatisticsUrl = "/statistics";

    [JsonProperty]
    public const string CasesUrl = "/cases";

    public const string ErrorUrl = "/error";

    public const string NotFoundUrl = "/404";

    public const string ApiSegment = "/api";
}
