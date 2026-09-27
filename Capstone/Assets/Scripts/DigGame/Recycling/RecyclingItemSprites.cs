using UnityEngine;
namespace DigGame
{
    public static class RecyclingItemSprites
    {
        static Sprite _paper, _foil;
        public static Sprite For(LootItem item)
        {
            if (item.Category == RecyclingCategory.Paper)
                return _paper != null ? _paper : (_paper = Build(false));
            if (item.Name != null && item.Name.Contains("포일"))
                return _foil != null ? _foil : (_foil = Build(true));
            return null;
        }
        // Original 32px illustrations, cached once. No external artwork dependency.
        static Sprite Build(bool foil)
        {
            var texture=new Texture2D(32,32,TextureFormat.RGBA32,false) { filterMode=FilterMode.Point, name=foil ? "RecyclingFoil" : "RecyclingPaper" };
            var pixels=new Color32[32*32];
            for (int y=4;y<28;y++) for(int x=5;x<27;x++)
            {
                bool edge=x==5||x==26||y==4||y==27;
                Color32 color;
                if (foil)
                    color=edge ? new Color32(82,108,126,255) : ((x+y*2)%9<3 ? new Color32(235,247,252,255) : new Color32(154,181,197,255));
                else
                    color=edge ? new Color32(131,116,92,255) : (y%5==0 && x>8 && x<23 ? new Color32(156,151,133,255) : new Color32(244,236,208,255));
                pixels[y*32+x]=color;
            }
            texture.SetPixels32(pixels); texture.Apply();
            return Sprite.Create(texture,new Rect(0,0,32,32),new Vector2(.5f,.5f),32);
        }
    }
}
