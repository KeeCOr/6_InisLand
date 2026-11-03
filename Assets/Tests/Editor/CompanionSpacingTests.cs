using System.Reflection;
using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class CompanionSpacingTests
    {
        [Test] public void TwoFollowersUseSeparateReadableLanes()
        {
            var player=new GameObject("Formation player");var a=new GameObject("Formation ally A");var b=new GameObject("Formation ally B");
            try
            {
                var first=a.AddComponent<Companion>();var second=b.AddComponent<Companion>();first.Player=second.Player=player.transform;
                var slot=typeof(Companion).GetMethod("GetFormationSlot",BindingFlags.Instance|BindingFlags.NonPublic);
                Vector2 left=(Vector2)slot.Invoke(first,null),right=(Vector2)slot.Invoke(second,null);
                Assert.That(Vector2.Distance(left,right),Is.GreaterThanOrEqualTo(1.2f),"Followers must not share almost the same point");
                Assert.That(first.SeparationRadius,Is.GreaterThanOrEqualTo(1.1f));
            }
            finally{Object.DestroyImmediate(a);Object.DestroyImmediate(b);Object.DestroyImmediate(player);}
        }
    }
}
