using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public sealed class EnemyBodyPushTests
    {
        [UnityTest] public IEnumerator EnemyMovementCannotPushPlayerOrCompanionButWallsStillBlock()
        {
            var scene=SceneManager.CreateScene("Enemy body push test",new CreateSceneParameters(LocalPhysicsMode.Physics2D));
            var physics=scene.GetPhysicsScene2D();
            try
            {
                for(int kind=0;kind<2;kind++)
                {
                    var actor=new GameObject("Protected actor");SceneManager.MoveGameObjectToScene(actor,scene);
                    actor.transform.position=new Vector3(0,kind*10,0);
                    actor.AddComponent<CircleCollider2D>().radius=.4f;
                    if(kind==0)actor.AddComponent<PlayerController>().enabled=false;
                    else actor.AddComponent<Companion>().enabled=false;
                    var body=actor.GetComponent<Rigidbody2D>();Vector2 start=body.position;
                    var enemy=new GameObject("Moving enemy");SceneManager.MoveGameObjectToScene(enemy,scene);
                    enemy.transform.position=start+Vector2.left*1.2f;
                    enemy.AddComponent<CircleCollider2D>().radius=.5f;
                    enemy.AddComponent<Zombie>().enabled=false;var enemyBody=enemy.GetComponent<Rigidbody2D>();
                    for(int step=0;step<50;step++){enemyBody.velocity=Vector2.right*3;physics.Simulate(.02f);}
                    Assert.That(Vector2.Distance(start,body.position),Is.LessThan(.01f),"Enemy locomotion must not displace actor "+kind);
                    Assert.That(enemyBody.position.x,Is.GreaterThan(start.x+.5f),"No solid contact response between opposing actors");
                    var wall=new GameObject("Solid wall");SceneManager.MoveGameObjectToScene(wall,scene);
                    wall.transform.position=start+Vector2.right*5;wall.AddComponent<BoxCollider2D>().size=new Vector2(.5f,4);
                    for(int step=0;step<150;step++){enemyBody.velocity=Vector2.right*3;body.velocity=Vector2.right*3;physics.Simulate(.02f);}
                    Assert.That(body.position.x,Is.LessThan(start.x+5),"Player/companion must still collide with walls");
                    Assert.That(enemyBody.position.x,Is.LessThan(start.x+5),"Enemy must still collide with walls");
                }
            }
            finally {SceneManager.UnloadSceneAsync(scene);}
            yield return null;
        }
    }
}
