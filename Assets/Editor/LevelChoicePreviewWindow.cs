using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    // Editor-only inspection of the actual runtime drawing method; no preview switches enter a player build.
    public sealed class LevelChoicePreviewWindow : EditorWindow
    {
        private GameObject _root;
        private SimpleHud _hud;
        private readonly BindingFlags _flags=BindingFlags.NonPublic|BindingFlags.Instance;
        public static void Open()
        {
            var window=CreateInstance<LevelChoicePreviewWindow>();window.titleContent=new GUIContent("InisLand Level Choices QA");
            window.position=new Rect(60,60,1280,720);window.ShowUtility();
        }
        private void OnEnable()
        {
            _root=new GameObject("Level choices QA"){hideFlags=HideFlags.HideAndDontSave};
            var progression=_root.AddComponent<PlayerProgression>();progression.GrantXp(progression.XpToNext);
            _hud=_root.AddComponent<SimpleHud>();_hud.enabled=false;_hud.Progression=progression;
            typeof(SimpleHud).GetField("_runeOffer",_flags).SetValue(_hud,new List<RuneKind>{RuneKind.MeleeDamageUp,RuneKind.RangedDamageUp,RuneKind.HpUp});
        }
        private void OnGUI()
        {
            if(_hud==null)return;
            var oldFont=GUI.skin.font;GUI.skin.font=UiTheme.HudFont;
            typeof(SimpleHud).GetMethod("EnsureStyles",_flags).Invoke(_hud,null);
            var old=GUI.matrix;
            float scale=Mathf.Min(position.width/MobileHudLayout.Width,position.height/MobileHudLayout.Height);
            GUI.matrix=Matrix4x4.TRS(new Vector3((position.width-MobileHudLayout.Width*scale)/2,(position.height-MobileHudLayout.Height*scale)/2,0),Quaternion.identity,Vector3.one*scale);
            typeof(SimpleHud).GetMethod("DrawRuneChoiceCards",_flags).Invoke(_hud,null);
            GUI.matrix=old;
            GUI.skin.font=oldFont;
        }
        private void OnDisable(){if(_root!=null)DestroyImmediate(_root);}
    }
}
