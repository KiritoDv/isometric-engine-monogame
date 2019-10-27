using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Linq;
using isometric_engine.src.util;

namespace isometric_engine.src.engine{

    public class Map{
        List<Block> blocks;
        GraphicsDeviceManager gl;
        Material[] type;
        int baseX = 0;
        int baseY = 0;
        int mX;
        int mY;
        double xC;
        double yC;

        int currentLayer = 0;
        int currentBlock = 0;
        
        private Texture2D lineTexture;

        SpriteFont font;

        public Thread generationThread;

        public Map(){
            this.gl = Main.getInstance().graphics;
            this.blocks = new List<Block>();
            this.type = new Material[40];
            this.type[0] = Material.STONE;
            this.type[1] = Material.GRASS;
            for(int i = 2; i < this.type.Length; i++){
                //Random r = new Random(i);
                this.type[i] = Material.WOOL;//new Wool(new Color(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255)));                
            }
        }

        public void preloadMap(){
            this.font = Main.getInstance().Content.Load<SpriteFont>("Arial");
            this.lineTexture = new Texture2D(Main.getInstance().GraphicsDevice, 1, 1);
            this.lineTexture.SetData<Color>(new Color[] { new Color(0, 0, 0, 50) });

            this.terrainGenerator();            
        }
        private KeyboardState oldState;

        public void updateMap(GameTime gameTime){
            
            var delta = (int)gameTime.ElapsedGameTime.TotalSeconds;

            //this.baseX = w.Width / 2;
            //this.baseY = w.Height / 6;

            this.mX = (Mouse.GetState().X - 16) - this.baseX;
            this.mY = (Mouse.GetState().Y - 8) - this.baseY;

            this.xC = Math.Round((double)((mX / (32 / 2) + mY / (32 / 4)) / 2));
            this.yC = Math.Round((double)((mY / (32 / 4) - (mX / (32 / 2))) / 2));

            //text("X: "+xC, mouseX+20, mouseY+10, 12);
            //text("Y: "+yC, mouseX+60, mouseY+10, 12);	            
            Block[] fT = blocks.ToArray();

            List<Block> tmp = fT.Where(a => a.location.getBlockX() == xC && a.location.getBlockY() == yC).OrderByDescending(si => si.location.getBlockZ()).ToList();
            
            //var im = Array.Find<Block>(blocks.ToArray(), a => a.location.getBlockX() == xC && a.location.getBlockY() == yC && a.location.getBlockZ() == this.currentLayer);	

            if(Mouse.GetState().LeftButton == ButtonState.Pressed){
                if(tmp.Capacity > 0 && tmp[0] != null){
                    blocks.Remove(tmp[0]);
                }
            }

            if(Mouse.GetState().RightButton == ButtonState.Pressed){
                if(tmp[0] == null && xC >= 0 && yC >= 0){                    
                    this.setBlock(new Location((int)xC, (int)yC, currentLayer), this.type[this.currentBlock]);
                }
            }

            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.W)) {
                this.baseY += 10;
            }
            if (ks.IsKeyDown(Keys.S)) {
                this.baseY -= 10;
            }
            if (ks.IsKeyDown(Keys.A)) {
                this.baseX += 10;
            }
            if (ks.IsKeyDown(Keys.D)) {
                this.baseX -= 10;
            }

            KeyboardState newState = Keyboard.GetState();  // get the newest state
 
            if(oldState.IsKeyUp(Keys.P) && ks.IsKeyDown(Keys.P))
            {
                this.currentLayer++;
            }
            
            if(oldState.IsKeyUp(Keys.O) && ks.IsKeyDown(Keys.O)){
                if(this.currentLayer > 0)
                    this.currentLayer--;
            }

            if(oldState.IsKeyUp(Keys.I) && ks.IsKeyDown(Keys.I)){
                if(this.currentBlock < this.type.Length-1)
                    this.currentBlock++;
            }

            if(oldState.IsKeyUp(Keys.U) && ks.IsKeyDown(Keys.U)){
                if(this.currentBlock > 0)
                    this.currentBlock--;
            }

            if(oldState.IsKeyUp(Keys.L) && ks.IsKeyDown(Keys.L)){
                this.blocks.Clear();
                this.generationThread.Abort();
                this.terrainGenerator();
            }

            oldState = ks;            
        }
        public void renderMap(SpriteBatch batch, GameTime time){
            Rectangle w = Main.getInstance().windowSize;
            batch.Begin();
            for(var x = 0; x <= 40; x++){                
                this.DrawLine(batch, new Vector2(this.baseX + 16 + (-x*16), this.baseY + 16 +  (x*8)), new Vector2(this.baseX + 16 + (((40/2)*32)-(x*16)), this.baseY + 16 + (((40/2)*16)+(x*8))), Color.Black);
            }
            for(var x = 0; x <= 40; x++){
                this.DrawLine(batch, new Vector2(this.baseX + 16 + (x*16), this.baseY + 16 + (x*8)), new Vector2(this.baseX + 16 + (-((40)/2)*32)+(x*16), this.baseY + 16 + (((40)/2)*16)+(x*8)), Color.Black);
            }
            batch.End();
            
            //List<Block> fix = this.blocks.Where(a => (this.baseX + (int)Math.Round((float)((a.x - a.y) * (a.texture.Width / 2)))) >= a.texture.Width + 32 && (this.baseY + (int)Math.Round((float)((((a.x + a.y) * (a.texture.Height / 4)))))) >= a.texture.Height + 32 && (this.baseX + (int)Math.Round((float)((a.x - a.y) * (a.texture.Width / 2)))) < w.Width - 64&& (this.baseY + (int)Math.Round((float)((((a.x + a.y) * (a.texture.Height / 4)))))) <= w.Height - 64 ).ToList();
            batch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, null);
            var fixedSize = blocks.ToArray();

            for (int id = 0; id < fixedSize.Length; id++){
                var a = fixedSize[id];
                var tex = Main.getInstance().textureManager.GetTexture2D(a.material.Texture);
                var x = this.baseX + (int)Math.Round((float)((a.location.getBlockX() - a.location.getBlockY()) * (tex.Width / 2)));
			    var y = this.baseY + (int)Math.Round((float)((((a.location.getBlockX() + a.location.getBlockY()) * (tex.Height / 4))) - ((a.location.getBlockZ()) * (tex.Height / 2))));                
                if(x >= -tex.Width && y >= -tex.Height && x < w.Width && y <= w.Height){

                    float f = (float)id;
                    float f1 = (float)blocks.Capacity;
                    float layer = (a.location.getBlockX() + a.location.getBlockY() + a.location.getBlockZ()) / f1;
                    
                    batch.Draw(tex, new Rectangle(x, y, tex.Width, tex.Height), null, a.color, 0, Vector2.Zero, SpriteEffects.None, layer);
                }
            }
            batch.End();

            batch.Begin();
            var gX = this.baseX + Math.Round(((xC - yC) * (32 / 2)));
	        var gY = this.baseY + Math.Round(((xC + yC) * (32 / 4))) + 9;
            
            this.DrawLine(batch, new Vector2((int)gX+16,(int)gY+ -8), new Vector2((int)gX, (int)gY), Color.Yellow);
			this.DrawLine(batch, new Vector2((int)gX+16,(int)gY+ 8), new Vector2((int)gX, (int)gY), Color.Yellow);
			this.DrawLine(batch, new Vector2((int)gX+32,(int)gY), new Vector2((int)gX+16, (int)gY-8), Color.Yellow);
			this.DrawLine(batch, new Vector2((int)gX+32,(int)gY), new Vector2((int)gX+16, (int)gY+8), Color.Yellow);
            batch.End();

            batch.Begin();
            batch.DrawString(this.font, $"Layer: {this.currentLayer+1}", new Vector2(20, 20), Color.Black);
            batch.DrawString(this.font, $"Block: {this.currentBlock+1}", new Vector2(20, 50), Color.Black);
            batch.End();
        }
        public void terrainGenerator(){
            float scale = 0.0052f;
            int size = 500; 
            Noise.Seed = new Random().Next(int.MinValue, int.MaxValue);
            this.generationThread = new Thread(()=>{     
                float[,] map = Noise.Calc2D(size, size, scale);

                for(int c = 0; c < ((map.GetLength(0) + map.GetLength(1))/16); c++){
                    for(int x = c; c+x < map.GetLength(0)/16; x++){
                        for(int y = c; c+y < map.GetLength(1)/16; y++){
                            double noise = Math.Abs(map[x,y]);
                            if(noise >= 0){
                                int waterHeight = 2;
                                //Material m = (int)noise <= waterHeight ? (Block)new Wool(new Color(46, 124, 248, 150)) : new Grass();
                                Material m = (int)noise <= waterHeight ? Material.WOOL : Material.GRASS;
                                this.setBlock(new Location(x, y, (int)noise <= waterHeight ? waterHeight : noise), m);
                            }
                        }
                    }
                }                
            });
            this.generationThread.Priority = ThreadPriority.Highest;
            this.generationThread.IsBackground = true;
            this.generationThread.Start();
        }

        public void setBlock(Location loc, Material m){
            Block b = new Block(loc, m);
            if(!this.blocks.Contains(b))
                this.blocks.Add(b);
        }

        public void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end, Color color){
            //this.lineTexture.SetData<Color>(new Color[] { color });
            Vector2 edge = end - start;
            // calculate angle to rotate line
            float angle = (float)Math.Atan2(edge.Y , edge.X);

            sb.Draw(this.lineTexture,
                new Rectangle(// rectangle defines shape of line and position of start of line
                    (int)start.X,
                    (int)start.Y,
                    (int)edge.Length(), //sb will strech the texture to fill this rectangle
                    2), //width of line, change this to make thicker line
                null,
                color,
                angle,     //angle of line (calulated above)
                new Vector2(0, 0), // point in line about which to rotate
                SpriteEffects.None,
                0);

        }
    }
}