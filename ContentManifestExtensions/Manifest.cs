using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContentManifestExtensions
{
    public enum ContentType
    {
        Unknown,
        Texture,
        XML,
    }

    public class Entry
    {
        public ContentType Type { get; set; }
        public String Name { get; set; }
    }

    public class Manifest
    {
        public List<Entry> Entries = new List<Entry>();

    }
}
