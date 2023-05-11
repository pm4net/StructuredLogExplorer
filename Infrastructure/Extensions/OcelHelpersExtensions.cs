using OCEL.CSharp;
using pm4net.Utilities;

namespace Infrastructure.Extensions
{
    public static class OcelHelpersExtensions
    {
        public static string? Namespace(this OcelEvent @event)
        {
            var ns = OcelHelpers.GetNamespace(@event.ToFSharpOcelEvent());
            return ns.IsSome() ? ns.Value : null;
        }

        public static string? SourceFile(this OcelEvent @event)
        {
            @event.VMap.TryGetValue("pm4net_SourceFile", out var val); // TODO: Read prefix from somewhere (perhaps the logger could add an attribute to the global section to indicate the prefix used)
            if (val != null && val is OcelString str)
            {
                return str.Value;
            }
            return null;
        }

        public static long? LineNumber(this OcelEvent @event)
        {
            @event.VMap.TryGetValue("pm4net_LineNumber", out var val); // TODO: Read prefix from somewhere (perhaps the logger could add an attribute to the global section to indicate the prefix used)
            if (val != null && val is OcelInteger ln)
            {
                return ln.Value;
            }
            return null;
        }
    }
}
