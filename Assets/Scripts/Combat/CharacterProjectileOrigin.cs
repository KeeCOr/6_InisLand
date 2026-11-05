using UnityEngine;

namespace IL6
{
    // Anchors are measured within the attack sprite, independent of its pivot and actor scale.
    public static class CharacterProjectileOrigin
    {
        public static Vector3 Resolve(Transform actor, Vector3 target, string characterKey)
        {
            var renderer = actor.GetComponent<SpriteRenderer>();
            var facing = DirectionalCharacterVisual.ResolveFacing(target - actor.position, CharacterFacing.Right);
            var sprite = string.IsNullOrEmpty(characterKey) ? null
                : CharacterActionVisual.Frame(characterKey, facing, ActionPhase.Start);
            if (sprite == null && renderer != null) sprite = renderer.sprite;
            if (sprite == null) return actor.position + Vector3.up * .45f;

            Vector2 anchor = facing == CharacterFacing.Front ? new Vector2(.51f, .52f)
                : facing == CharacterFacing.Back ? new Vector2(.5f, .70f)
                : new Vector2(.84f, .56f);
            var bounds = sprite.bounds;
            Vector3 local = new Vector3(Mathf.Lerp(bounds.min.x, bounds.max.x, anchor.x),
                Mathf.Lerp(bounds.min.y, bounds.max.y, anchor.y), 0);
            if (facing == CharacterFacing.Left) local.x = -local.x;
            return actor.TransformPoint(local);
        }
    }
}
