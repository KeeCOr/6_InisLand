using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace IL6.EditorBuild
{
    public static class PlayerActionAnimationGenerator
    {
        private const string Root="Assets/Resources/Sprites/Player/RedHairGirl";
        public static void Generate()
        {
            Directory.CreateDirectory(Root+"/Actions");
            AssetDatabase.Refresh();
            string path=Root+"/logging.png";
            var pixels=new Texture2D(2,2);pixels.LoadImage(File.ReadAllBytes(path));
            int width=pixels.width/4,height=pixels.height/3;
            var slices=new SpriteMetaData[12];
            int firstHeight=0;
            for(int i=0;i<12;i++)
            {
                int x=(i%4)*width,y=(2-i/4)*height,minY=height,maxY=0;
                for(int py=0;py<height;py++)
                    for(int px=0;px<width;px++)
                        if(pixels.GetPixel(x+px,y+py).a>.5f) { minY=Mathf.Min(minY,py);maxY=Mathf.Max(maxY,py); }
                if(i==0) firstHeight=maxY-minY+1;
                slices[i]=new SpriteMetaData {name="redhair_logging_"+i,rect=new Rect(x,y,width,height),alignment=9,pivot=new Vector2(.5f,(float)minY/height)};
            }
            Object.DestroyImmediate(pixels);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.spritePixelsPerUnit=firstHeight/.95f;
            importer.wrapMode=TextureWrapMode.Clamp;importer.spritesheet=slices;importer.SaveAndReimport();
            var logging=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Resources/Sprites/Player/Player.controller");
            foreach(string weapon in new[]{"axe","bow"})
            {
                var body=AssetDatabase.LoadAllAssetsAtPath(Root+"/"+weapon+".png").OfType<Sprite>().ToArray();
                var overrides=AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(Root+"/RedHairGirl"+(weapon=="axe"?"Axe":"Bow")+".overrideController");
                foreach(string action in new[]{"Idle","Move","Attack","Logging","Gather","Dead"})
                    foreach(string phase in new[]{"Start","Loop","End"})
                    {
                        int[] frames;
                        float fps=10;
                        if(action=="Logging") { int start=phase=="Start"?0:phase=="Loop"?4:8;frames=new[]{start,start+1,start+2,start+3};fps=phase=="Loop"?4:10; }
                        else if(action=="Idle") { frames=phase=="Loop"?new[]{0,1,2,3,2,1}:new[]{0};fps=5; }
                        else if(action=="Move") { frames=phase=="Start"?new[]{4}:phase=="Loop"?new[]{5,6,7,4}:new[]{0};fps=10; }
                        else if(action=="Attack") frames=phase=="Start"?new[]{8,9}:phase=="Loop"?new[]{10}:new[]{11,0};
                        else if(action=="Gather") frames=phase=="Start"?new[]{0,8}:phase=="Loop"?new[]{9,10}:new[]{11,0};
                        else {frames=phase=="Start"?new[]{12,13}:phase=="Loop"?new[]{14}:new[]{15};fps=8;}
                        string name=weapon+"_"+action+"_"+phase;
                        var clip=MakeClip(name,action=="Logging"?logging:body,action=="Logging"?"redhair_logging_":"redhair_"+weapon+"_",frames,fps,phase=="Loop");
                        string stateName=action+"_"+phase;
                        var machine=controller.layers[0].stateMachine;
                        var state=machine.states.Select(s=>s.state).FirstOrDefault(s=>s.name==stateName);
                        if(state==null) state=machine.AddState(stateName);
                        if(weapon=="axe") state.motion=clip;
                        else overrides["axe_"+action+"_"+phase]=clip;
                    }
                EditorUtility.SetDirty(overrides);
            }
            EditorUtility.SetDirty(controller);AssetDatabase.SaveAssets();
            Debug.Log("[PlayerActionAnimationGenerator] 36 clips: all player actions Start/Loop/End; logging atlas sliced 4x3.");
        }
        private static AnimationClip MakeClip(string name,Sprite[] sprites,string prefix,int[] indices,float fps,bool loop)
        {
            string path=Root+"/Actions/"+name+".anim";
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if(clip==null) {clip=new AnimationClip();AssetDatabase.CreateAsset(clip,path);}
            clip.name=name;clip.frameRate=fps;
            var keys=new ObjectReferenceKeyframe[indices.Length+1];
            for(int i=0;i<keys.Length;i++) keys[i]=new ObjectReferenceKeyframe {time=i/fps,value=sprites.Single(s=>s.name==prefix+indices[i<indices.Length?i:loop?0:indices.Length-1])};
            AnimationUtility.SetObjectReferenceCurve(clip,new EditorCurveBinding {path="",type=typeof(SpriteRenderer),propertyName="m_Sprite"},keys);
            var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=loop;AnimationUtility.SetAnimationClipSettings(clip,settings);
            EditorUtility.SetDirty(clip);return clip;
        }
    }
}
