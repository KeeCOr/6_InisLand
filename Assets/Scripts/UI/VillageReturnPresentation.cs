using UnityEngine;
namespace IL6
{
    public readonly struct VillageReturnFrame
    {
        public readonly float CoverAlpha, IllustrationAlpha, NoticeAlpha;
        public readonly bool ReturnDue;
        public VillageReturnFrame(float cover,float illustration,float notice,bool returnDue)
        {CoverAlpha=cover;IllustrationAlpha=illustration;NoticeAlpha=notice;ReturnDue=returnDue;}
    }
    public static class VillageReturnPresentation
    {
        public static VillageReturnFrame At(float elapsed,float duration)
        {
            float t=Mathf.Clamp01(elapsed/Mathf.Max(.01f,duration));
            float cover=Mathf.SmoothStep(0,1,Mathf.Clamp01(t/.05f))*Mathf.Clamp01((1-t)/.1f);
            float illustration=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.2f,.28f,t))*cover;
            float notice=(1-Mathf.InverseLerp(.2f,.28f,t))*cover;
            return new VillageReturnFrame(cover,illustration,notice,t>=.12f);
        }
    }
}
