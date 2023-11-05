using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DG.OneDrive.Serialized
{
    public class ListResult<T>
    {
        [JsonProperty("@odata.context")]
        private readonly string _context;

        [JsonProperty("@odata.count")]
        private readonly long _count;

        [JsonProperty("@odata.nextLink")]
        private readonly Uri _nextLink;

        [JsonProperty("value")]
        private readonly T[] _values;

        public string Context => _context;

        public long Count => _count;

        public Uri NextLink => _nextLink;

        public IReadOnlyList<T> Values => _values;
    }
}
