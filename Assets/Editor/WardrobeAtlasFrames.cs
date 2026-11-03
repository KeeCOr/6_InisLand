using System;
using System.IO;
using UnityEngine;

namespace IL6.EditorBuild
{
    // Generated poses can cross nominal thirds. Recorded transparent gutters retain whole boots/hair.
    internal static class WardrobeAtlasFrames
    {
        [Serializable] private sealed class Cell { public int x,y,width,height; }
        [Serializable] private sealed class Layout { public Cell[] cells; public float pixelsPerUnit; }
        public static float Density(string path) => JsonUtility.FromJson<Layout>(File.ReadAllText(Path.ChangeExtension(path,"layout.json"))).pixelsPerUnit;
        public static Rect[] Rects(Texture2D atlas,string path)
        {
            var layout=JsonUtility.FromJson<Layout>(File.ReadAllText(Path.ChangeExtension(path,"layout.json")));
            var rects=new Rect[layout.cells.Length];
            for(int i=0;i<rects.Length;i++)
            {
                var c=layout.cells[i];
                if(c.x<0 || c.y<0 || c.width<=0 || c.height<=0 || c.x+c.width>atlas.width || c.y+c.height>atlas.height)
                    throw new InvalidDataException(path);
                rects[i]=new Rect(c.x,atlas.height-c.y-c.height,c.width,c.height);
            }
            return rects;
        }
        public static Texture2D[] Load(Texture2D atlas,string path)
        {
            var layout=JsonUtility.FromJson<Layout>(File.ReadAllText(Path.ChangeExtension(path,"layout.json")));
            if(layout?.cells==null || layout.cells.Length!=9) throw new InvalidDataException(path);
            int width=0,height=0;
            var opaque=new RectInt[9];
            for(int i=0;i<9;i++)
            {
                var c=layout.cells[i];var pixels=atlas.GetPixels(c.x,atlas.height-c.y-c.height,c.width,c.height);
                int low=c.height,high=-1,left=c.width,right=-1;
                for(int y=0;y<c.height;y++) for(int x=0;x<c.width;x++)
                    if(pixels[y*c.width+x].a>.5f) { low=Math.Min(low,y);high=Math.Max(high,y);left=Math.Min(left,x);right=Math.Max(right,x); }
                if(high<low) throw new InvalidDataException("Empty wardrobe pose: "+path);
                opaque[i]=new RectInt(left,low,right-left+1,high-low+1);
                width=Math.Max(width,right-left+1);height=Math.Max(height,high-low+1);
            }
            // Keep consistent, compact canvases; source-sheet padding must not enlarge actors.
            width=height=Math.Max(width,height)+16;
            var frames=new Texture2D[9];
            for(int i=0;i<9;i++)
            {
                var c=layout.cells[i];
                if(c.x<0 || c.y<0 || c.x+c.width>atlas.width || c.y+c.height>atlas.height)
                    throw new InvalidDataException("Atlas crop outside image: "+path);
                var frame=new Texture2D(width,height,TextureFormat.RGBA32,false);
                frame.SetPixels32(new Color32[width*height]);
                var b=opaque[i];
                frame.SetPixels((width-b.width)/2,8,b.width,b.height,
                    atlas.GetPixels(c.x+b.xMin,atlas.height-c.y-c.height+b.yMin,b.width,b.height));
                frame.Apply();frames[i]=frame;
            }
            return frames;
        }
    }
}
