using System;

namespace DG.OneDrive.Serialized.Resources
{

    public class BaseItem
    {
        public string id { get; set; }
        public IdentitySet createdBy { get; set; }
        public DateTimeOffset createdDateTime { get; set; }
        public string description { get; set; }
        public string eTag { get; set; }
        public IdentitySet lastModifiedBy { get; set; }
        public DateTimeOffset lastModifiedDateTime { get; set; }
        public string name { get; set; }
        public ItemReference parentReference { get; set; }
        public string webUrl { get; set; }

        public override string ToString()
        {
            return name;
        }
    }
}
