using System.Collections.Generic;
using UnityEngine;

// Small deterministic pixel assets. All generated textures are shared, with no external art dependency.
public static class PrototypeArt
{
    private static readonly Dictionary<string,Sprite> sprites = new Dictionary<string,Sprite>();
    private static PrototypeVisualTheme theme;
    public static PrototypeVisualTheme Theme => theme != null ? theme : theme = Resources.Load<PrototypeVisualTheme>("PrototypeVisualTheme");
    public static bool Enabled => Theme == null || Theme.usePrototypeVisuals;
    public static readonly Color Cyan = new Color32(79,211,208,255);
    public static readonly Color Amber = new Color32(255,184,83,255);
    public static readonly Color Red = new Color32(239,100,104,255);
    private static readonly Color Ink = new Color32(16,24,33,255);
    private static readonly Color Steel = new Color32(54,84,99,255);
    private static readonly Color Light = new Color32(224,239,230,255);
    private static Color Hex(byte r,byte g,byte b) => new Color32(r,g,b,255);
    private sealed class Pixels
    {
        public readonly int width,height;
        public readonly Color[] data;
        public Pixels(int w,int h){width=w;height=h;data=new Color[w*h];}
        public void Dot(int x,int y,Color color){if(x>=0&&y>=0&&x<width&&y<height)data[y*width+x]=color;}
        public void Box(int x,int y,int w,int h,Color color){for(int yy=y;yy<y+h;yy++)for(int xx=x;xx<x+w;xx++)Dot(xx,yy,color);}
        public void Diamond(int x,int y,int radius,Color color){for(int yy=-radius;yy<=radius;yy++)for(int xx=-radius;xx<=radius;xx++)if(Mathf.Abs(xx)+Mathf.Abs(yy)<=radius)Dot(x+xx,y+yy,color);}
    }
    private static Sprite Bake(string key,Pixels pixels,float ppu=24)
    {
        var texture=new Texture2D(pixels.width,pixels.height,TextureFormat.RGBA32,false){name="Prototype "+key,filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Repeat};
        texture.SetPixels(pixels.data);texture.Apply(false,true);
        var sprite=Sprite.Create(texture,new Rect(0,0,pixels.width,pixels.height),Vector2.one*0.5f,ppu,0,SpriteMeshType.FullRect);
        sprite.name="Prototype "+key;sprites[key]=sprite;return sprite;
    }
    public static Sprite Actor(bool player,int frame,int variant=0)
    {
        Sprite[] custom=Theme!=null?(player?Theme.playerWalk:Theme.monsterWalk):null;
        if(custom!=null&&custom.Length>0&&custom[frame%custom.Length]!=null)return custom[frame%custom.Length];
        string key=$"actor-{player}-{frame}-{variant}";if(sprites.TryGetValue(key,out Sprite cached))return cached;
        var p=new Pixels(24,24);int step=frame==1?1:frame==3?-1:0;
        if(player)
        {
            p.Box(7,2+Mathf.Max(0,step),4,5,Ink);p.Box(14,2+Mathf.Max(0,-step),4,5,Ink);
            p.Box(8,3+Mathf.Max(0,step),3,4,Steel);p.Box(14,3+Mathf.Max(0,-step),3,4,Steel);
            p.Box(6,6,13,9,Ink);p.Box(7,7,11,7,Hex(37,111,121));p.Box(9,9,7,4,Cyan);
            p.Box(5,7,3,6,Steel);p.Box(17,7,3,6,Steel);p.Box(18,6,2,3,Hex(226,183,141));
            p.Box(8,13,11,2,Amber);p.Box(6,11,3,3,Hex(206,103,49));
            p.Box(7,15,12,7,Ink);p.Box(8,18,10,3,Steel);p.Box(9,21,7,1,Cyan);
            p.Box(9,15,9,3,Hex(218,174,134));p.Box(11,17,8,2,Ink);p.Box(12,18,6,1,Cyan);p.Dot(17,18,Light);
            p.Box(7,16,2,3,Hex(90,111,109));p.Box(8,7,2,3,Light);
        }
        else if(variant==1) // ranged drone
        {
            p.Box(4,7,16,11,Ink);p.Box(6,9,12,7,Steel);p.Box(8,10,8,5,Hex(118,99,156));
            p.Box(3,4+step,4,6,Ink);p.Box(17,4-step,4,6,Ink);p.Box(9,15,6,3,Red);p.Box(11,16,2,1,Light);
            p.Box(1,11,4,3,Steel);p.Box(19,11,4,3,Steel);p.Dot(5,7,Amber);p.Dot(18,7,Amber);
        }
        else if(variant==2) // volatile crawler
        {
            p.Diamond(12,12,10,Ink);p.Diamond(12,12,8,Hex(110,78,69));p.Diamond(12,13,5,Amber);
            p.Diamond(12,13,3,Red);p.Box(9,13,6,2,Ink);p.Dot(10,14,Light);p.Dot(14,14,Light);
            p.Box(4,2+Mathf.Max(0,step),5,5,Ink);p.Box(15,2+Mathf.Max(0,-step),5,5,Ink);
        }
        else
        {
            p.Box(5,4+Mathf.Max(0,step),5,5,Ink);p.Box(14,4+Mathf.Max(0,-step),5,5,Ink);
            p.Box(4,8,16,10,Ink);p.Box(6,9,12,8,Hex(111,85,99));p.Box(8,10,8,5,Hex(163,105,119));
            p.Box(6,16,12,5,Ink);p.Box(7,17,10,3,Hex(102,74,93));p.Box(8,16,8,2,Red);p.Dot(9,17,Light);p.Dot(15,17,Light);
            p.Box(2,10,4,5,Steel);p.Box(18,10,4,5,Steel);
        }
        return Bake(key,p);
    }
    public static Sprite Weapon(WeaponAttackType type)
    {
        string key="weapon-"+type;if(sprites.TryGetValue(key,out Sprite cached))return cached;
        var p=new Pixels(24,12);p.Box(2,3,19,6,Ink);p.Box(4,4,13,4,Steel);p.Box(14,5,9,2,Light);
        p.Box(5,1,4,4,Ink);p.Box(8,3,7,2,Amber);p.Dot(18,7,Cyan);
        if(type==WeaponAttackType.Melee){p=new Pixels(24,12);p.Box(3,2,14,2,Hex(156,113,73));p.Box(14,3,7,8,Ink);p.Box(15,5,5,5,Light);}
        else if(type==WeaponAttackType.ExplosiveProjectile){p.Box(2,2,17,8,Ink);p.Box(3,4,15,4,Hex(89,115,92));p.Box(17,3,5,6,Amber);}
        else if(type==WeaponAttackType.TargetExplosion||type==WeaponAttackType.BlackHole||type==WeaponAttackType.MagicRapidProjectile||type==WeaponAttackType.HomingProjectile)
        {p.Box(5,4,14,4,Hex(94,82,135));p.Diamond(17,6,4,Cyan);p.Diamond(17,6,2,Light);}
        return Bake(key,p);
    }
    public static Sprite Item(int type)
    {
        string key="item-"+type;if(sprites.TryGetValue(key,out Sprite cached))return cached;
        var p=new Pixels(16,16);
        if(type==0){p.Diamond(8,8,7,Ink);p.Diamond(8,8,6,Cyan);p.Diamond(7,10,3,Light);p.Diamond(9,6,3,Hex(36,128,160));}
        else {p.Box(5,11,6,4,Ink);p.Box(6,12,4,2,Amber);p.Box(3,2,10,10,Ink);p.Box(4,3,8,8,type==1?Red:Cyan);p.Box(5,5,2,5,Light);}
        return Bake(key,p,16);
    }
    public static Sprite Shape(string name)
    {
        if(sprites.TryGetValue(name,out Sprite cached))return cached;
        var p=new Pixels(32,32);
        for(int y=0;y<32;y++)for(int x=0;x<32;x++)
        {
            float dx=x-15.5f,dy=y-15.5f,d=Mathf.Sqrt(dx*dx+dy*dy);
            if(name=="shadow"&&dx*dx/210f+dy*dy/24f<1)p.Dot(x,y,new Color(0.01f,0.02f,0.03f,0.48f));
            if(name=="ring"&&d>12&&d<14)p.Dot(x,y,Color.white);
            if(name=="flash"&&(Mathf.Abs(dx)<2||Mathf.Abs(dy)<2||Mathf.Abs(dx)+Mathf.Abs(dy)<10)&&d<14)p.Dot(x,y,Color.white);
            if(name=="slash"&&d>11&&d<15&&dx>1)p.Dot(x,y,Color.white);
            if(name=="pixel")p.Dot(x,y,Color.white);
            if(name=="bullet"&&x>4&&x<28&&Mathf.Abs(dy)<3)p.Dot(x,y,x>18?Light:Amber);
        }
        return Bake(name,p,32);
    }
    public static Sprite Tile(bool wall)
    {
        if(Theme!=null&&(wall?Theme.wallTile:Theme.floorTile)!=null)return wall?Theme.wallTile:Theme.floorTile;
        string key=wall?"wall":"floor";if(sprites.TryGetValue(key,out Sprite cached))return cached;
        var p=new Pixels(32,32);
        for(int y=0;y<32;y++)for(int x=0;x<32;x++)
        {
            int grain=(x*17+y*31+x*y*7)%7;
            Color color=wall?Hex((byte)(38+grain),(byte)(54+grain),(byte)(64+grain)):Hex((byte)(29+grain),(byte)(41+grain),(byte)(49+grain));
            if(x==0||y==0)color=Ink;
            if(x==1||y==30)color=wall?Steel:Hex(53,66,72);
            if(wall&&y<5)color=Ink;
            if((x==4||x==27)&&(y==4||y==27))color=wall?Light:Steel;
            p.Dot(x,y,color);
        }
        return Bake(key,p,32);
    }
    public static Sprite Terminal()
    {
        const string key="terminal";if(sprites.TryGetValue(key,out Sprite cached))return cached;
        var p=new Pixels(24,24);p.Box(3,3,18,18,Ink);p.Box(5,5,14,14,Steel);p.Box(6,10,12,8,Cyan);
        p.Box(7,11,10,6,Hex(21,59,71));p.Box(8,14,7,1,Cyan);p.Box(8,12,4,1,Light);p.Box(6,6,11,2,Amber);
        return Bake(key,p);
    }
}
