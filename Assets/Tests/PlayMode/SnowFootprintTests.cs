using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace IL6.Tests
{
    public class SnowFootprintTests
    {
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
        private static Type FieldType=>typeof(SnowEmitter).Assembly.GetType("IL6.SnowFootprintField");
        private static int Count(Component field)=>(int)FieldType.GetProperty("Count").GetValue(field);
        private static bool Stamp(Component field,Vector2 p)=> (bool)FieldType.GetMethod("Stamp").Invoke(field,new object[]{p,Vector2.up,true});
        private static Vector3 Center(Component field,int index)
        {
            var v=field.GetComponentInChildren<MeshFilter>().sharedMesh.vertices;int i=index*4;
            return (v[i]+v[i+1]+v[i+2]+v[i+3])/4;
        }
        [UnityTest] public IEnumerator MovementLeavesAlternatingWorldMarksThatClearBeforeNight()
        {
            Assert.That(FieldType,Is.Not.Null,"Daytime footprint field is missing");
            Time.timeScale=1;InputReader.HudBlocked=false;
            var fieldGo=new GameObject("footprint field test");var field=fieldGo.AddComponent(FieldType);
            EventBus.Instance.Emit(new DayStartedPayload(1));
            var actor=new GameObject("walking test");var sr=actor.AddComponent<SpriteRenderer>();sr.sprite=SpriteBank.PlayerIdle();
            actor.AddComponent<DirectionalCharacterVisual>().Configure("Hero");yield return null;
            Assert.That(actor.GetComponent(typeof(SnowEmitter).Assembly.GetType("IL6.SnowFootprintTrail")),Is.Not.Null);
            yield return null;Assert.That(Count(field),Is.Zero,"Standing still must not stamp");
            for(int i=0;i<10;i++){actor.transform.position+=Vector3.right*.2f;yield return null;}
            Assert.That(Count(field),Is.GreaterThanOrEqualTo(4));
            var first=Center(field,0);var second=Center(field,1);Assert.That(first.y*second.y,Is.LessThan(0),"Left and right boots alternate across the path");
            int count=Count(field);var cameraGo=new GameObject("moving camera");cameraGo.AddComponent<Camera>();cameraGo.transform.position=new Vector3(100,100,-10);
            Time.timeScale=0;actor.transform.position+=Vector3.right;yield return new WaitForSecondsRealtime(.1f);
            Assert.That(Count(field),Is.EqualTo(count));Assert.That(Center(field,0),Is.EqualTo(first),"Pause/camera movement must not erase or move old marks");
            Time.timeScale=1;yield return null;Assert.That(Count(field),Is.EqualTo(count),"Paused displacement must not be backfilled");
            sr.enabled=false;actor.transform.position+=Vector3.right;yield return null;Assert.That(Count(field),Is.EqualTo(count));
            sr.enabled=true;actor.transform.position+=Vector3.right*20;yield return null;Assert.That(Count(field),Is.EqualTo(count),"Teleport must not draw a trail");
            EventBus.Instance.Emit(new DayStartedPayload(1));Assert.That(Count(field),Is.EqualTo(count),"Duplicate daytime event preserves footprints");
            EventBus.Instance.Emit(new EveningStartedPayload(1));Assert.That(Count(field),Is.Zero);Assert.That(Stamp(field,Vector2.zero),Is.False);
            foreach(var mesh in field.GetComponentsInChildren<MeshFilter>())Assert.That(mesh.sharedMesh.vertexCount,Is.Zero);
            foreach(var renderer in field.GetComponentsInChildren<MeshRenderer>())Assert.That(renderer.enabled,Is.False);
            EventBus.Instance.Emit(new NightStartedPayload(1));Assert.That(Count(field),Is.Zero);Assert.That(Stamp(field,Vector2.zero),Is.False);
            EventBus.Instance.Emit(new DayStartedPayload(2));Assert.That(Stamp(field,Vector2.zero),Is.True);yield return null;
            EventBus.Instance.Emit(new DayStartedPayload(3));Assert.That(Count(field),Is.Zero,"A new day starts clean");
            Object.Destroy(actor);Object.Destroy(fieldGo);Object.Destroy(cameraGo);yield return null;
        }
        [UnityTest] public IEnumerator FieldUsesBoundedGroundBatchesWithoutEvictingDaytimeMarks()
        {
            Assert.That(FieldType,Is.Not.Null);Time.timeScale=1;InputReader.HudBlocked=false;
            var go=new GameObject("bounded footprint field");var field=go.AddComponent(FieldType);EventBus.Instance.Emit(new DayStartedPayload(1));
            int capacity=(int)FieldType.GetField("Capacity").GetRawConstantValue();
            for(int i=0;i<capacity+5;i++)Stamp(field,new Vector2(i*.4f,0));
            yield return null;Assert.That(Count(field),Is.EqualTo(capacity));Assert.That(Center(field,0).x,Is.EqualTo(0).Within(.001f));
            Assert.That(go.GetComponentsInChildren<MeshRenderer>().Length,Is.LessThanOrEqualTo(32));
            Assert.That(go.GetComponentsInChildren<Collider2D>(),Is.Empty);
            foreach(var renderer in go.GetComponentsInChildren<MeshRenderer>())Assert.That(renderer.sortingOrder,Is.EqualTo(-9400),"Above snow patches (-9500), below Y-sorted actors (>=-9000)");
            Object.Destroy(go);yield return null;
        }
        [UnityTest] public IEnumerator SnowfieldInstallsReadableSnowAndWalkingTracks()
        {
            Assert.That(FieldType,Is.Not.Null);Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(1);
            var hud=Object.FindFirstObjectByType<SimpleHud>();typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(hud,null);
            GameSession.Instance.Cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Day,elapsedInPhase=0});EventBus.Instance.Emit(new DayStartedPayload(1));
            var field=Object.FindObjectOfType(FieldType) as Component;Assert.That(field,Is.Not.Null);
            var snow=Object.FindFirstObjectByType<SnowEmitter>();Assert.That(snow.Rate,Is.GreaterThanOrEqualTo(72));Assert.That(snow.ParticleSize,Is.GreaterThanOrEqualTo(.2f));
            Assert.That(Resources.Load<Texture2D>("Sprites/FX/Snowfall20261103/flake"),Is.Not.Null);Assert.That(Resources.Load<Texture2D>("Sprites/FX/Snowfall20261103/bootprint"),Is.Not.Null);
            var player=hud.Player;player.enabled=false;player.GetComponent<PlayerAttackController>().enabled=false;
            player.transform.position=new Vector3(GameConstants.VillageCenterX,GameConstants.VillageCenterY+3,0);yield return null;
            int before=Count(field);var body=player.GetComponent<Rigidbody2D>();body.velocity=Vector2.right*4;
            yield return new WaitForSeconds(.35f);body.velocity=Vector2.zero;yield return null;
            Assert.That(Count(field),Is.GreaterThan(before),"Actual player displacement leaves tracks");
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
