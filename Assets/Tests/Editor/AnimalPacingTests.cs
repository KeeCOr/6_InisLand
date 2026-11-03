using NUnit.Framework;
using UnityEngine;
using UnityEditor;
namespace IL6.Tests
{
    public class AnimalPacingTests
    {
        [Test] public void PreyCanBeCaughtWithBasePlayerMovementAndPrefabsMatchSpawns()
        {
            float speed=Resources.Load<BalanceConfig>("BalanceConfig").PlayerMoveSpeed;
            foreach(var animal in ProceduralSpawner._animals)
            {
                if(animal.IsPredator) continue;
                Assert.That(animal.FleeSpeed,Is.LessThanOrEqualTo(speed*.85f),animal.Name+" needs a chase advantage for the player");
                Assert.That(animal.FleeRadius,Is.LessThanOrEqualTo(4f),animal.Name+" must not flee from far offscreen");
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Animals/"+animal.Name.Replace("_proc","")+".prefab");
                Assert.That(prefab,Is.Not.Null,animal.Name);
                var ai=prefab.GetComponent<DeerAi>();
                Assert.That(ai.FleeSpeed,Is.EqualTo(animal.FleeSpeed));Assert.That(ai.FleeRadius,Is.EqualTo(animal.FleeRadius));
            }
        }
    }
}
