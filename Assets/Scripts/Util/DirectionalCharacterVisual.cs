using System.Collections.Generic;
using UnityEngine;

namespace IL6
{
    public enum CharacterFacing { Right, Left, Front, Back }

    // Image-swap locomotion complements the existing exclusive action clips.
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class DirectionalCharacterVisual : MonoBehaviour
    {
        private static readonly Dictionary<string,Sprite[]> Sheets = new Dictionary<string,Sprite[]>();
        private static readonly Dictionary<string,Sprite> WalkSprites = new Dictionary<string,Sprite>();
        private readonly WalkFrameCycle _walk=new WalkFrameCycle();
        private SpriteRenderer _renderer;
        private Rigidbody2D _body;
        private PlayerController _player;
        private PlayerAttackController _attack;
        private Sprite _side;
        private string _key;
        private bool _wasVertical;
        public CharacterFacing Facing { get; private set; } = CharacterFacing.Right;
        public string CharacterKey => _key;
        public ActionPhase MovementPhase => _walk.Phase;
        public int MovementFrame => _walk.Frame;

        private void Awake()
        {
            _renderer=GetComponent<SpriteRenderer>();_body=GetComponent<Rigidbody2D>();
            _player=GetComponent<PlayerController>();_attack=GetComponent<PlayerAttackController>();
            _side=_renderer.sprite;
        }

        public void Configure(string role)
        {
            _key=KeyForRole(role);
            if(_renderer==null) _renderer=GetComponent<SpriteRenderer>();
            ActorSnowPresentation.Attach(_renderer);
            if(GetComponent<SnowFootprintTrail>()==null)gameObject.AddComponent<SnowFootprintTrail>();
            if(GetComponent<CharacterActionVisual>()==null)gameObject.AddComponent<CharacterActionVisual>();
            _side=_key.StartsWith("hero") ? SpriteBank.PlayerIdle() : SpriteBank.CompanionSpriteForRole(role);
            _wasVertical=false;
            if(!_key.StartsWith("hero")) RenderDirection(Vector2.zero,false);
        }

        public static string KeyForRole(string role)
        {
            switch((role??"").ToLowerInvariant())
            {
                case "hero": case "player": case "주인공": return "hero_axe";
                case "hunter": case "사냥꾼": return "hunter";
                case "warrior": case "전사": return "warrior";
                case "farmer": case "농부": return "farmer";
                case "cook": case "요리사": return "cook";
                case "healer": case "치유사": return "healer";
                default: return SpriteBank.IsChildRole(role)?"child":SpriteBank.IsFemaleRole(role)?"aunt":"uncle";
            }
        }

        public static CharacterFacing ResolveFacing(Vector2 movement,CharacterFacing previous)
        {
            if(movement.sqrMagnitude<.0025f) return previous;
            float x=Mathf.Abs(movement.x),y=Mathf.Abs(movement.y);
            // Keep the previous axis near diagonals, avoiding alternating front/profile frames.
            bool vertical=previous==CharacterFacing.Front || previous==CharacterFacing.Back;
            if(y>x*1.15f || (vertical && y>=x/1.15f)) return movement.y>0?CharacterFacing.Back:CharacterFacing.Front;
            return movement.x<0?CharacterFacing.Left:CharacterFacing.Right;
        }

        public static Sprite DirectionSprite(string key,CharacterFacing facing)
        {
            if(!Sheets.TryGetValue(key,out var sheet))
            {
                sheet=Resources.LoadAll<Sprite>("Sprites/Characters/Directions20261026/"+key);
                Sheets[key]=sheet;
            }
            string name=facing==CharacterFacing.Back?"back":"front";
            foreach(var s in sheet) if(s.name==name) return s;
            return null;
        }

        public void ShowDirection(Vector2 movement)
        {
            RenderDirection(movement,false);
        }

        public static Sprite WalkSprite(string key,CharacterFacing facing,int frame)
        {
            string direction=facing==CharacterFacing.Front?"front":facing==CharacterFacing.Back?"back":"side";
            string path=key+"/walk_"+direction+"_"+(frame%2);
            if(!WalkSprites.TryGetValue(path,out var sprite) || sprite==null)
            {sprite=Resources.Load<Sprite>("Sprites/Characters/Walk20261026/"+path);WalkSprites[path]=sprite;}
            return sprite;
        }

        public void AnimateMovement(Vector2 velocity,float deltaTime)
        {
            _walk.Tick(velocity,deltaTime);
            RenderDirection(velocity,_walk.Moving);
        }

        public static Sprite HeroSideIdle(string key)
        {
            return Resources.Load<Sprite>("Sprites/Characters/HeroIdentity20261026/"+key+"/idle_side");
        }

        private void RenderDirection(Vector2 movement,bool walking)
        {
            if(string.IsNullOrEmpty(_key) || _renderer==null) return;
            Facing=!_key.StartsWith("hero") && !walking && movement.sqrMagnitude<.0025f
                ? CharacterFacing.Front : ResolveFacing(movement,Facing);
            string key=_player!=null ? (_attack!=null && _attack.WeaponIndex==1?"hero_bow":_attack!=null && _attack.IsConstructionHammer?"hero_hammer":"hero_axe") : _key;
            if(_player!=null && _player.Actions!=null && _player.Actions.IsBusy)
            {
                if(_wasVertical && _renderer.sprite!=null && (_renderer.sprite.name=="front" || _renderer.sprite.name=="back" || _renderer.sprite.name.StartsWith("walk_")))
                    _renderer.sprite=key=="hero_bow"?SpriteBank.LoadSubSprite("Player/RedHairGirl/bow","redhair_bow_0"):_side;
                _wasVertical=false;return;
            }
            bool vertical=Facing==CharacterFacing.Front || Facing==CharacterFacing.Back;
            if(walking)
            {
                var step=WalkSprite(key,Facing,_walk.Frame);
                if(step!=null)
                {_renderer.sprite=step;_renderer.flipX=Facing==CharacterFacing.Left;_wasVertical=true;return;}
            }
            if(vertical)
            {
                var sprite=DirectionSprite(key,Facing);
                if(sprite!=null) {_renderer.sprite=sprite;_renderer.flipX=false;_wasVertical=true;}
            }
            else
            {
                if(_player!=null) _renderer.sprite=HeroSideIdle(key)??_side;
                else _renderer.sprite=_side;
                _renderer.flipX=Facing==CharacterFacing.Left;_wasVertical=false;
            }
        }

        private void LateUpdate()
        {
            if(Time.timeScale<=0 || InputReader.HudBlocked) return;
            AnimateMovement(_body!=null?_body.velocity:Vector2.zero,Time.deltaTime);
        }
    }
}
