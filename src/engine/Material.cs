using System;
namespace isometric_engine.src.engine{
    public class Material{
        public static Material GRASS = new Material("Grass");
        public static Material STONE = new Material("Stone");
        public static Material WOOL = new Material("Wool_Base");

        public string Texture;

        public Material(string Texture){
            this.Texture = Texture;
        }
    }
}