using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace IL6.EditorBuild
{
    // Equal bytes are not interchangeable paths when Resources.Load binds by name.
    public sealed class CharacterResourceBuildGuard : IPreprocessBuildWithReport
    {
        public int callbackOrder=>0;
        public void OnPreprocessBuild(BuildReport report)
        {
            var missing=Missing();
            if(missing.Count>0) throw new BuildFailedException("Required character/UI resources are missing. Restore these paths before replacing the final build:\n"+string.Join("\n",missing));
        }
        public static List<string> Missing()
        {
            var missing=new List<string>();
            if(Resources.Load<Sprite>(CampfireSnowMelt.SpritePath)==null)missing.Add(CampfireSnowMelt.SpritePath);
            if(Resources.Load<Texture2D>(QuestTrackerFrameSkin.FramePath)==null)missing.Add(QuestTrackerFrameSkin.FramePath);
            if(Resources.Load<Texture2D>(QuestTrackerFrameSkin.RibbonPath)==null)missing.Add(QuestTrackerFrameSkin.RibbonPath);
            if(Resources.Load<Texture2D>(HomeNavigation.IconPath)==null)missing.Add(HomeNavigation.IconPath);
            foreach(string path in new[]{RuneChoiceFrameSkin.FramePath,RuneChoiceFrameSkin.CardPath})
                if(Resources.Load<Texture2D>(path)==null)missing.Add(path);
            if(Resources.Load<Texture2D>("UI/Generated20261103/sunset_warning")==null)missing.Add("Sunset warning icon");
            foreach(RuneKind kind in System.Enum.GetValues(typeof(RuneKind)))
            {
                string path=RuneChoicePresentation.IconPath(kind);
                if(path==null || Resources.Load<Texture2D>(path)==null)missing.Add("Rune choice icon: "+kind);
                if(Resources.Load<Texture2D>(RuneChoicePresentation.ScopeIconPath(kind))==null)missing.Add("Rune scope icon: "+kind);
            }
            foreach(string key in new[]{"hero_axe","hero_bow","hero_hammer","hunter","warrior","farmer","cook","healer","aunt","uncle","child"})
                foreach(var facing in new[]{CharacterFacing.Right,CharacterFacing.Front,CharacterFacing.Back})
                    foreach(var phase in new[]{ActionPhase.Start,ActionPhase.Loop,ActionPhase.End})
                        if(CharacterActionVisual.Frame(key,facing,phase)==null)missing.Add("Character action "+key+" "+facing+" "+phase);
            if(Resources.LoadAll<Sprite>("Sprites/FX/Hit20261103/impact").Length!=3)missing.Add("Three hit impact frames");
            foreach(string material in new[]{"wood","stone","plant"})
                foreach(string phase in new[]{"start","loop","end"})
                {
                    string path="Sprites/FX/Gather20261103/"+material+"_"+phase;
                    if(Resources.Load<Sprite>(path)==null)missing.Add(path);
                }
            foreach(string key in new[]{"horizontal","vertical"})
                if(Resources.Load<Sprite>("Sprites/Props/FenceWreckage20261103/"+key)==null)missing.Add("Fence wreckage "+key);
            foreach(string key in new[]{"arrow","home","damage"})
                if(Resources.Load<Texture2D>(NavigationIconSkin.Folder+key)==null)missing.Add(NavigationIconSkin.Folder+key);
            foreach(string key in new[]{"quadruped_robot","zombie","boss_frost_zombie","boss_winter_knight","boss_iron_giant","boss_frost_lich"})
                if(Resources.Load<Sprite>("Sprites/Enemies/"+key)==null)missing.Add("Sprites/Enemies/"+key);
            foreach(string key in EnemyFacingVisual.Keys)
                foreach(var facing in new[]{CharacterFacing.Front,CharacterFacing.Back,CharacterFacing.Right})
                    if(EnemyFacingVisual.Frame(key,facing)==null)missing.Add("Enemy direction "+key+" "+facing);
            if(Resources.Load<Material>("Materials/SnowActor")==null)missing.Add("Materials/SnowActor");
            if(Resources.Load<Sprite>("Sprites/Environment/Snowfield/actor_shadow")==null)missing.Add("Sprites/Environment/Snowfield/actor_shadow");
            foreach(string key in new[]{"deer","rabbit","boar","wolf","bear","mammoth","fox","snow_hare","dog","hawk"})
                foreach(var facing in new[]{CharacterFacing.Front,CharacterFacing.Back,CharacterFacing.Right})
                {
                    if(AnimalMotionVisual.SpriteFor(key,facing,false)==null)missing.Add(key+" idle "+facing);
                    for(int frame=0;frame<2;frame++)if(AnimalMotionVisual.SpriteFor(key,facing,true,frame)==null)missing.Add(key+" move "+facing+" "+frame);
                }
            for(int variant=0;variant<RockVisualStyle.VariantCount;variant++)
                if(RockVisualStyle.SpriteFor(variant)==null)missing.Add("Natural rock variant: "+variant);
            for(int variant=0;variant<SpriteBank.TreeVariantCount;variant++)
                if(SpriteBank.TreeVariant(variant)==null)missing.Add("Natural tree variant: "+variant);
            foreach(string path in new[]{"Sprites/FX/nordic_arrow","Sprites/FX/throwing_stone","Sprites/Environment/Snowfield/snow_ground","Sprites/Environment/Snowfield/snow_contact"})
                if(Resources.Load<Sprite>(path)==null) missing.Add(path);
            foreach(string role in new[]{"Hunter","Warrior","Farmer","Cook","Healer","Aunt","Uncle","Child"})
            {
                string key=DirectionalCharacterVisual.KeyForRole(role);
                if(SpriteBank.CompanionSpriteForRole(role)==null) missing.Add("Companion idle: "+role);
                if(DialoguePortraits.ForRole(role)==null) missing.Add("UI/DialoguePortraits/"+key);
                CheckLocomotion(key,missing);
                foreach(var facing in new[]{CharacterFacing.Front,CharacterFacing.Back,CharacterFacing.Right})
                {
                    if(facing!=CharacterFacing.Right && DirectionalCharacterVisual.DirectionSprite(key,facing)==null) missing.Add(key+" direction "+facing);
                    for(int frame=0;frame<2;frame++) if(DirectionalCharacterVisual.WalkSprite(key,facing,frame)==null) missing.Add(key+" walk "+facing+" "+frame);
                }
            }
            foreach(string key in new[]{"hero_axe","hero_bow","hero_hammer"})
            {
                CheckLocomotion(key,missing);
                if(DirectionalCharacterVisual.HeroSideIdle(key)==null) missing.Add(key+" idle");
                foreach(var facing in new[]{CharacterFacing.Front,CharacterFacing.Back,CharacterFacing.Right})
                {
                    if(facing!=CharacterFacing.Right && DirectionalCharacterVisual.DirectionSprite(key,facing)==null) missing.Add(key+" direction "+facing);
                    for(int frame=0;frame<2;frame++) if(DirectionalCharacterVisual.WalkSprite(key,facing,frame)==null) missing.Add(key+" walk "+facing+" "+frame);
                }
            }
            foreach(string path in new[]{"UI/DialoguePortraits/hero","UI/Generated20261014/pill","UI/Generated20261014/resources"})
                if(Resources.Load<Texture2D>(path)==null) missing.Add(path);
            foreach(string path in new[]{"UI/Generated20261014/buttons","UI/Generated20261014/panel","UI/Generated20261017/ore","UI/Generated20261018/quest_tracker","UI/DialoguePortraits/father"})
                if(Resources.Load<Texture2D>(path)==null) missing.Add(path);
            foreach(string phase in new[]{"day","night","dawn","evening"})
                if(Resources.Load<AudioClip>("Music/"+phase)==null) missing.Add("Music/"+phase);
            foreach(BuildingKind kind in System.Enum.GetValues(typeof(BuildingKind)))
                if(ConstructionPlacement.Sprite(kind)==null) missing.Add("Building sprite: "+kind);
            foreach(string key in new[]{"waiting","storm","ruins","promise","village_return"})
                if(Resources.Load<Texture2D>("UI/StoryIllustrations/"+key)==null) missing.Add("UI/StoryIllustrations/"+key);
            if(Resources.LoadAll<Sprite>("Sprites/Story20261026/village_ruins").Length!=4) missing.Add("Sprites/Story20261026/village_ruins (four slices)");
            return missing;
        }
        private static void CheckLocomotion(string key,List<string> missing)
        {
            string pair="Sprites/Characters/Directions20261026/"+key;
            if(Resources.LoadAll<Sprite>(pair).Length!=2) missing.Add(pair+" (front/back pair)");
            foreach(string facing in new[]{"front","back","side"})
                for(int frame=0;frame<2;frame++)
                {
                    string path="Sprites/Characters/Walk20261026/"+key+"/walk_"+facing+"_"+frame;
                    if(Resources.Load<Sprite>(path)==null) missing.Add(path);
                }
        }
    }
    public static class CharacterCohesionInstaller
    {
        public static void Install()
        {
            HeroIdentityInstaller.Install();
            CompanionIdentityInstaller.Install();
            StoryResourceInstaller.Install();
        }
    }
}
