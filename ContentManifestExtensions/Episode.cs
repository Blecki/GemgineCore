using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ContentManifestExtensions
{
    public class Episode
    {
        public String Type { get; set; }
        public String Name { get; set; }
        public Int32 Version { get; set; }
        public List<String> Dependencies = new List<string>();

        [ContentSerializerIgnore]
        public Manifest Manifest { get; internal set; }

    }

    public class EpisodeException : ContentLoadException { }
    public class EpisodeNotFoundException : EpisodeException { }
    public class ManifestNotFoundException : EpisodeException { }

    public class EpisodeContentManager : ContentManager
    {
        public Episode Module { get; private set; }
        private bool Constructed = false;
        public Manifest Manifest { get { return Module.Manifest; } }

        private Dictionary<String, Texture2D> TextureTable = new Dictionary<string, Texture2D>();

        public Texture2D GetTexture(String name)
        {
            if (String.IsNullOrEmpty(name)) return null;
            if (!TextureTable.ContainsKey(name))
            {
                Texture2D Tex = null;
                try
                {
                    Tex = this.Load<Texture2D>(name);
                }
                catch (ContentLoadException) { }
                TextureTable.Add(name, Tex);
                return Tex;
            }
            return TextureTable[name];
        }

        public void FrontLoadTextures()
        {
            foreach (var Item in Manifest.Entries)
            {
                if (Item.Type == ContentManifestExtensions.ContentType.Texture)
                    TextureTable.Add(Item.Name, this.Load<Texture2D>(Item.Name));
            }
        }

        

        public EpisodeContentManager(IServiceProvider ServiceProvider, String RootDirectory, String Episode)
            : base(ServiceProvider, RootDirectory)
        {
            Constructed = false;

            try
            {
                Module = this.Load<Episode>(Episode + "/episode");
            }
            catch (ContentLoadException) { }
            if (Module == null) throw new EpisodeNotFoundException();

            Module.Name = Episode;
            Module.Manifest = this.Load<ContentManifestExtensions.Manifest>(Episode + "/manifest");
            if (Module.Manifest == null) throw new ManifestNotFoundException();

            foreach (var Dependency in Module.Dependencies)
            {
                var DependencyManifest = this.Load<ContentManifestExtensions.Manifest>(Dependency + "/manifest");
                if (DependencyManifest == null) continue;

                Module.Manifest.Entries = new List<Entry>(Module.Manifest.Entries.Union(DependencyManifest.Entries));
            }

            Constructed = true;
        }

        protected override System.IO.Stream OpenStream(string assetName)
        {
            if (Constructed)
            {
                if (Module == null) throw new ContentLoadException();
                try
                {
                    return base.OpenStream(Module.Name + "\\" + assetName);
                }
                catch (ContentLoadException) { }
                foreach (var Dependency in Module.Dependencies)
                {
                    try
                    {
                        return base.OpenStream(System.IO.Path.Combine(Dependency, assetName));
                    }
                    catch (ContentLoadException) { }
                }

                throw new ContentLoadException();
            }
            else return base.OpenStream(assetName);
        }

        public System.IO.TextReader OpenTextStream(string assetName) 
        { 
            return new System.IO.StreamReader(OpenStream(assetName)); 
        }

        public System.IO.TextReader OpenUnbuiltTextStream(string assetName)
        {
            if (Module == null) throw new ContentLoadException();
            try
            {
                return new System.IO.StreamReader(System.IO.Path.Combine(Module.Name, assetName));
            }
            catch (ContentLoadException)
            {
                foreach (var Dependency in Module.Dependencies)
                {
                    try
                    {
                        return new System.IO.StreamReader(System.IO.Path.Combine(Dependency, assetName));
                    }
                    catch (ContentLoadException) { }
                }
            }

            throw new ContentLoadException("Could not find raw asset " + assetName);
        }
    }
}
