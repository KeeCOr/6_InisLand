using UnityEngine;

namespace IL6
{
    public static class ScreenEnemyPresence
    {
        public static bool Any(Camera camera)
        {
            if(camera==null || !camera.isActiveAndEnabled)return false;
            var planes=GeometryUtility.CalculateFrustumPlanes(camera);
            foreach(var enemy in Object.FindObjectsByType<Zombie>(FindObjectsSortMode.None))
            {
                if(enemy.IsDead || !enemy.gameObject.activeInHierarchy)continue;
                var renderer=enemy.GetComponent<SpriteRenderer>();
                if(renderer==null || !renderer.enabled || renderer.sprite==null)continue;
                if((camera.cullingMask & (1<<renderer.gameObject.layer))==0)continue;
                if(GeometryUtility.TestPlanesAABB(planes,renderer.bounds))return true;
            }
            return false;
        }
    }
}
