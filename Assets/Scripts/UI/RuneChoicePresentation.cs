using System;
using System.Collections.Generic;
using UnityEngine;
namespace IL6
{
    public enum RuneWeaponScope { Common, Melee, Ranged }
    public enum RuneStatIcon { Damage, Duration, Health, Range, Projectiles, Chain, Chance, Slow, Rune, Wood, Stone, Food, Frostbloom }
    public readonly struct RuneStatToken
    {
        public readonly RuneStatIcon Icon;
        public readonly string Value, Hint;
        public RuneStatToken(RuneStatIcon icon,string value,string hint) {Icon=icon;Value=value;Hint=hint;}
    }
    public static class RuneChoicePresentation
    {
        public static RuneWeaponScope Scope(RuneKind kind)=>kind switch {
            RuneKind.MeleeDamageUp or RuneKind.PoisonBlade or RuneKind.LightningStrike or RuneKind.MeleeCleave=>RuneWeaponScope.Melee,
            RuneKind.RangedDamageUp or RuneKind.Pierce or RuneKind.IceArrow or RuneKind.MultiShot or RuneKind.Detonator=>RuneWeaponScope.Ranged,
            _=>RuneWeaponScope.Common};
        public static string ScopeLabel(RuneKind kind)=>Scope(kind) switch {
            RuneWeaponScope.Melee=>"근접 전용",RuneWeaponScope.Ranged=>"원거리 전용",_=>"공통"};
        public static Color ScopeColor(RuneKind kind)=>Scope(kind) switch {
            RuneWeaponScope.Melee=>new Color(.91f,.62f,.33f),RuneWeaponScope.Ranged=>new Color(.4f,.79f,.94f),_=>new Color(.52f,.83f,.67f)};
        public static string ScopeIconPath(RuneKind kind)=>"UI/rune_choices/rune-scope-"+Scope(kind).ToString().ToLowerInvariant();
        public static string CategoryLabel(RuneKind kind)=>RuneUpgradeRules.Category(kind) switch {
            RuneUpgradeCategory.Stats=>"능력치 상승",RuneUpgradeCategory.Allies=>"동료 강화",
            RuneUpgradeCategory.Melee=>"근접 무기 강화",RuneUpgradeCategory.Ranged=>"원거리 무기 강화",_=>"자원"};
        public static Color CategoryColor(RuneKind kind)=>RuneUpgradeRules.Category(kind) switch {
            RuneUpgradeCategory.Allies=>new Color(.75f,.65f,.94f),RuneUpgradeCategory.Resources=>new Color(.96f,.8f,.4f),_=>ScopeColor(kind)};
        public static string CategoryIconPath(RuneKind kind)=>RuneUpgradeRules.Category(kind) switch {
            RuneUpgradeCategory.Allies=>"UI/rune_choices/rune-choice-ally-boost",
            RuneUpgradeCategory.Resources=>"UI/rune_choices/rune-choice-resource-gift",_=>ScopeIconPath(kind)};
        public static string Name(RuneKind kind) {string title=PlayerProgression.Title(kind);return title.Substring(title.IndexOf(' ')+1);}
        public static string IconPath(RuneKind kind)
        {
            string file=kind switch {
                RuneKind.PoisonBlade=>"poison-blade",RuneKind.IceArrow=>"ice-arrow",RuneKind.LightningStrike=>"lightning-strike",
                RuneKind.MultiShot=>"multi-shot",RuneKind.Detonator=>"detonator",RuneKind.SummonDog=>"summon-dog",
                RuneKind.SummonHawk=>"summon-hawk",RuneKind.Vampirism=>"vampirism",RuneKind.Thorns=>"thorns",
                RuneKind.Pierce=>"pierce",RuneKind.AllyBoost=>"ally-boost",RuneKind.ResourceGift=>"resource-gift",
                RuneKind.DamageUp=>"damage-up",RuneKind.FireRateUp=>"fire-rate-up",RuneKind.HpUp=>"hp-up",
                RuneKind.RangeUp=>"range-up",RuneKind.MoveSpeedUp=>"move-speed-up",
                RuneKind.MeleeCleave=>"melee-cleave",RuneKind.MeleeDamageUp=>"melee-damage-up",RuneKind.RangedDamageUp=>"ranged-damage-up",_=>null};
            return file!=null ? "UI/rune_choices/rune-choice-"+file : null;
        }
        public static string Trigger(RuneKind kind,int level) => kind switch {
            RuneKind.PoisonBlade or RuneKind.IceArrow or RuneKind.LightningStrike=>"적중",
            RuneKind.Detonator or RuneKind.Vampirism=>"처치",
            RuneKind.Thorns=>"피격",RuneKind.SummonDog or RuneKind.SummonHawk=>level>=3?"정예 소환":"소환",
            RuneKind.MultiShot=>level>=3?"부채꼴":"발사",RuneKind.Pierce=>"관통",
            RuneKind.AllyBoost=>"동료",RuneKind.ResourceGift=>"즉시",_=>"강화"};
        public static List<RuneStatToken> Stats(RuneKind kind,int level)
        {
            int n=Mathf.Clamp(level,1,3);int Pick(int a,int b,int c)=>n==1?a:n==2?b:c;
            var result=new List<RuneStatToken>();
            void Add(RuneStatIcon icon,string value,string hint)=>result.Add(new RuneStatToken(icon,value,hint));
            switch(kind)
            {
                case RuneKind.PoisonBlade:Add(RuneStatIcon.Damage,Pick(5,7,18)+"/초","초당 독 피해");Add(RuneStatIcon.Duration,Pick(3,4,6)+"초","독 지속시간");break;
                case RuneKind.IceArrow:Add(RuneStatIcon.Slow,n==3?"−80%":"−50%","이동속도 감소");Add(RuneStatIcon.Duration,Pick(2,3,5)+"초","둔화 지속시간");break;
                case RuneKind.LightningStrike:Add(RuneStatIcon.Chance,Pick(50,70,100)+"%","번개 발동 확률");Add(RuneStatIcon.Damage,Pick(6,9,20).ToString(),"번개 피해");Add(RuneStatIcon.Chain,"×"+n,"추가 연쇄 횟수");break;
                case RuneKind.MultiShot:Add(RuneStatIcon.Projectiles,"+"+Pick(1,2,4),"원거리 추가 화살");break;
                case RuneKind.MeleeCleave:Add(RuneStatIcon.Chain,"+"+Pick(1,2,4),"근접 추가 타격 대상");break;
                case RuneKind.Detonator:Add(RuneStatIcon.Damage,Pick(8,12,25).ToString(),"폭발 피해");Add(RuneStatIcon.Range,n==1?"1.6칸":n==2?"1.8칸":"2.5칸","폭발 반경");break;
                case RuneKind.SummonDog:Add(RuneStatIcon.Rune,"×"+n,"사냥개 수");Add(RuneStatIcon.Damage,Pick(5,8,11).ToString(),"각 사냥개의 근접 피해");break;
                case RuneKind.SummonHawk:Add(RuneStatIcon.Rune,"×"+n,"매 수");Add(RuneStatIcon.Damage,Pick(4,7,10).ToString(),"각 매의 원거리 피해");break;
                case RuneKind.Vampirism:Add(RuneStatIcon.Health,"+"+Pick(2,4,9),"처치 시 체력 회복");break;
                case RuneKind.Thorns:Add(RuneStatIcon.Damage,Pick(5,9,20).ToString(),"반사 피해");Add(RuneStatIcon.Range,n==1?"1.5칸":n==2?"1.8칸":"2.5칸","반사 반경");break;
                case RuneKind.Pierce:Add(RuneStatIcon.Projectiles,"+"+Pick(1,2,4),"추가 관통 횟수");break;
                case RuneKind.AllyBoost:Add(RuneStatIcon.Damage,"+"+Pick(25,50,100)+"%","동료 피해 증가");break;
                case RuneKind.ResourceGift:Add(RuneStatIcon.Wood,"+"+Pick(10,20,40),"목재");Add(RuneStatIcon.Stone,"+"+Pick(5,10,20),"석재");Add(RuneStatIcon.Food,"+"+Pick(5,10,20),"식량");if(n==3) Add(RuneStatIcon.Frostbloom,"+1","서리꽃");break;
                case RuneKind.DamageUp:Add(RuneStatIcon.Damage,"+"+Pick(25,50,120)+"%","피해 증가");break;
                case RuneKind.MeleeDamageUp:Add(RuneStatIcon.Damage,"+"+Pick(25,50,120)+"%","근접 무기 피해 증가");break;
                case RuneKind.RangedDamageUp:Add(RuneStatIcon.Damage,"+"+Pick(25,50,120)+"%","원거리 무기 피해 증가");break;
                case RuneKind.FireRateUp:Add(RuneStatIcon.Duration,"+"+Pick(18,43,100)+"%","공격속도 증가");break;
                case RuneKind.HpUp:Add(RuneStatIcon.Health,"+"+Pick(25,50,120),"최대 체력 증가");break;
                case RuneKind.RangeUp:Add(RuneStatIcon.Range,"+"+Pick(1,2,5)+"칸","사거리 증가");break;
                case RuneKind.MoveSpeedUp:Add(RuneStatIcon.Rune,"+"+Pick(15,25,60)+"%","이동속도 증가");break;
            }
            return result;
        }
    }
}
