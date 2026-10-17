using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests {
public class LocomotionResponseTests {
[Test] public void LatestMovementInputImmediatelySelectsCorrectAction() {
var go=new GameObject("Locomotion test");
try { var state=go.AddComponent<PlayerActionState>();
state.SetMoving(true); Assert.That(state.State,Is.EqualTo(PlayerAction.Move));
state.SetMoving(false); Assert.That(state.State,Is.EqualTo(PlayerAction.Idle));
state.SetMoving(true); Assert.That(state.State,Is.EqualTo(PlayerAction.Move));
Assert.That(state.Phase,Is.EqualTo(ActionPhase.Start));
} finally { Object.DestroyImmediate(go); }
}
}}
