using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace IL6.Tests
{
    public sealed class HpBarScaleTests
    {
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void LargeGatherablesAndBuildingsKeepThinBoundedWorldBars(int kind)
        {
            var parent=new GameObject("Large scaled parent");parent.transform.localScale=new Vector3(3,7,1);
            var go=new GameObject("Scaled HP target");go.SetActive(false);go.transform.SetParent(parent.transform,false);go.transform.localScale=new Vector3(2,4,1);
            var bar=go.AddComponent<HpBarUi>();
            if(kind==0)bar.Tree=go.AddComponent<TreeHealth>();
            if(kind==1 || kind==2){bar.Building=go.AddComponent<Building>();bar.Building.Initialize(kind==1?BuildingKind.House:BuildingKind.Campfire);}
            if(kind==3)bar.Rock=go.AddComponent<RockHealth>();
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            typeof(HpBarUi).GetMethod("Awake",flags).Invoke(bar,null);
            bar.Size=new Vector2(8,3);bar.Offset=new Vector2(0,.9f);
            try
            {
                foreach(float size in new[]{.25f,1f,5f})
                {
                    parent.transform.localScale=new Vector3(size,size*2,1);
                    typeof(HpBarUi).GetMethod("LateUpdate",flags).Invoke(bar,null);
                    var root=go.transform.Find("HpBar");
                    Assert.That(Mathf.Abs(root.lossyScale.x),Is.EqualTo(1.1f).Within(.001f),"World width must be bounded");
                    Assert.That(Mathf.Abs(root.lossyScale.y),Is.EqualTo(.1f).Within(.001f),"Object scale must never thicken the bar");
                    if(kind==0 || kind==2)Assert.That(root.localPosition.y,Is.EqualTo(bar.Offset.y).Within(.001f),"Existing anchor must stay unchanged");
                }
            }
            finally {Object.DestroyImmediate(parent);}
        }
    }
}
