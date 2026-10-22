using System.Collections.Generic;
using UnityEngine;

namespace IL6
{
    // Share the actor scan across trees; keep people readable beneath a canopy.
    public sealed class TreeVisibility : MonoBehaviour
    {
        private static readonly List<SpriteRenderer> People = new();
        private static float _refreshAt;
        private SpriteRenderer _tree;
        private float _originalAlpha;
        private void Awake()
        {
            _tree=GetComponent<SpriteRenderer>();
            _originalAlpha=_tree!=null?_tree.color.a:1f;
        }
        private void LateUpdate()
        {
            if (_tree==null) return;
            if (Time.unscaledTime>=_refreshAt)
            {
                _refreshAt=Time.unscaledTime+.15f;People.Clear();
                foreach(var npc in Object.FindObjectsByType<RecruitableNpc>(FindObjectsSortMode.None))
                    if(npc.enabled) Add(npc.gameObject);
                foreach(var companion in Object.FindObjectsByType<Companion>(FindObjectsSortMode.None))
                    if(!companion.IsDead) Add(companion.gameObject);
                foreach(var player in Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
                    if(!player.IsDead) Add(player.gameObject);
            }
            var bounds=_tree.bounds;bool covered=false;
            foreach(var person in People)
            {
                if(person==null || !person.enabled || !person.gameObject.activeInHierarchy) continue;
                Vector3 center=person.bounds.center;
                if(center.x>=bounds.min.x && center.x<=bounds.max.x && center.y>=bounds.min.y && center.y<=bounds.max.y)
                {covered=true;break;}
            }
            Color color=_tree.color;
            color.a=Mathf.MoveTowards(color.a,covered?_originalAlpha*.28f:_originalAlpha,Time.deltaTime*5f);
            _tree.color=color;
        }
        private static void Add(GameObject actor)
        {
            if(actor.TryGetComponent<SpriteRenderer>(out var sprite)) People.Add(sprite);
        }
        private void OnDisable()
        {
            if(_tree!=null){Color color=_tree.color;color.a=_originalAlpha;_tree.color=color;}
        }
    }
}
