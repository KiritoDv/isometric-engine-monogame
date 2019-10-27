using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace isometric_engine.src.engine{
    public class TextureManager{
        
        ContentManager content;
        Dictionary<string, Texture2D> cache;
        public TextureManager(){
            this.content = Main.getInstance().Content;
            this.cache = new Dictionary<string, Texture2D>();
        }
        public Texture2D GetTexture2D(string path){
            if(!this.cache.ContainsKey(path)){
                this.cache.Add(path, this.content.Load<Texture2D>(path));
            }
            return this.cache[path];
        }

    }
}