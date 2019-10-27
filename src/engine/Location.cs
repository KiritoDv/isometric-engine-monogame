using System;
namespace isometric_engine.src.engine
{
    public class Location{
        
        private double x;
        private double y;
        private double z;

        public Location(double x, double y, double z){
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public double getX(){
            return this.x;
        }
        public double getY(){
            return this.y;
        }
        public double getZ(){
            return this.z;
        }
        public int getBlockX(){
            return (int)this.x;
        }
        public int getBlockY(){
            return (int)this.y;
        }
        public int getBlockZ(){
            return (int)this.z;
        }

        public String toString(){
            return $"[{this.x}, {this.y}, {this.z}]";
        }
    }
}