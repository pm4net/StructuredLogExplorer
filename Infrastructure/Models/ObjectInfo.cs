using Newtonsoft.Json;

namespace Infrastructure.Models
{
    public class ObjectInfo
    {
        [JsonProperty("id")]
        public string Name { get; set; } = string.Empty;

        public int UniqueInstances { get; set; }

        public int ReferencingEvents { get; set; }

        public IList<ObjectOccurrence> ObjectOccurrences { get; set; } = new List<ObjectOccurrence>();
    }

    public class ObjectOccurrence
    {
        public string Activity { get; init; } = string.Empty;

        public string? Namespace { get; init; }

        public string? SourceFile { get; init; }

        public long? LineNumber { get; init; }

        public string? CodeSnippet { get; set; }

        protected bool Equals(ObjectOccurrence other)
        {
            return Activity == other.Activity && 
                   Namespace == other.Namespace && 
                   SourceFile == other.SourceFile && 
                   LineNumber == other.LineNumber && 
                   CodeSnippet == other.CodeSnippet;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ObjectOccurrence)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Activity.GetHashCode();
                hashCode = (hashCode * 397) ^ (Namespace != null ? Namespace.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SourceFile != null ? SourceFile.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ LineNumber.GetHashCode();
                hashCode = (hashCode * 397) ^ (CodeSnippet != null ? CodeSnippet.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
