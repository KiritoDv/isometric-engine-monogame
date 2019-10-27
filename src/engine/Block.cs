using System.Security.Cryptography;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace isometric_engine.src.engine{
    public class Block : ICloneable{
        public Location location = new Location(0, 0, 0);
        public Material material;
        public Color color;

        public Block(Location loc, Material material){
            this.material = material;
            this.location = loc;
            this.randomColor();
        }
        public virtual object Clone() {
            this.randomColor();
            return this.MemberwiseClone();
        } 

        public virtual void randomColor(){
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            var byteArray = new byte[4];
            provider.GetBytes(byteArray);

            int c = new Random((int)BitConverter.ToUInt32(byteArray, 0)).Next(240, 255);
            this.color = new Color(c, c, c);
        }
    }

    
}