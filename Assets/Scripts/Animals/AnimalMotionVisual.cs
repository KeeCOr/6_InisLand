using System.Collections.Generic;
using UnityEngine;
namespace IL6
{
    [DefaultExecutionOrder(110)]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class AnimalMotionVisual : MonoBehaviour
    {
        private static readonly Dictionary<string,Sprite> Cache=new Dictionary<string,Sprite>();
        private WalkFrameCycle _cycle=new WalkFrameCycle();
        private SpriteRenderer _renderer;
        private Rigidbody2D _body;
        private AnimalAi _animal;
        private string _key;
        public string SpeciesKey => _key;
        public CharacterFacing Facing { get; private set; }=CharacterFacing.Right;
        public ActionPhase MovementPhase => _cycle.Phase;
        public int MovementFrame => _cycle.Frame;

        public static Sprite ReferenceSprite(string key)
        {
            switch(key)
            {
                case "deer":return SpriteBank.Deer();case "rabbit":return SpriteBank.Rabbit();
                case "boar":return SpriteBank.Boar();case "wolf":return SpriteBank.Wolf();
                case "bear":return SpriteBank.Bear();case "mammoth":return SpriteBank.Mammoth();
                case "fox":return SpriteBank.AnimalByName("Fox_proc");
                case "snow_hare":return SpriteBank.AnimalByName("SnowHare_proc");
                case "dog":return SpriteBank.DogPet();case "hawk":return SpriteBank.HawkPet();
                default:return null;
            }
        }
        public static string KeyForSprite(Sprite sprite)
        {
            if(sprite==null)return null;
            foreach(string key in new[]{"deer","rabbit","boar","wolf","bear","mammoth","fox","snow_hare","dog","hawk"})
                if(sprite==ReferenceSprite(key))return key;
            return null;
        }
        public static AnimalMotionVisual Attach(GameObject target,string key=null)
        {
            var renderer=target.GetComponent<SpriteRenderer>();
            if(renderer==null)return null;
            if(string.IsNullOrEmpty(key))key=KeyForSprite(renderer.sprite);
            if(string.IsNullOrEmpty(key))return null;
            var visual=target.GetComponent<AnimalMotionVisual>()??target.AddComponent<AnimalMotionVisual>();
            visual.Configure(key);return visual;
        }
        public void Configure(string key)
        {
            _key=key;_cycle=new WalkFrameCycle();Facing=CharacterFacing.Right;
            _renderer=GetComponent<SpriteRenderer>();_body=GetComponent<Rigidbody2D>();_animal=GetComponent<AnimalAi>();
            ActorSnowPresentation.Attach(_renderer,key);
            var collider=GetComponent<CircleCollider2D>();
            if(collider!=null)
            {
                float scale=Mathf.Max(Mathf.Abs(transform.lossyScale.x),Mathf.Abs(transform.lossyScale.y));
                collider.radius=AnimalSizeProfile.For(key).FootprintRadius/Mathf.Max(.001f,scale);
            }
            Render(false);
        }
        public static Sprite SpriteFor(string key,CharacterFacing facing,bool moving,int frame=0)
        {
            string direction=facing==CharacterFacing.Front?"front":facing==CharacterFacing.Back?"back":"side";
            string path=key+"/"+(moving?"move_"+direction+"_"+(frame%2):"idle_"+direction);
            if(!Cache.TryGetValue(path,out var sprite)||sprite==null)
            {sprite=Resources.Load<Sprite>("Sprites/Animals/Motion20261026/"+path);Cache[path]=sprite;}
            return sprite;
        }
        public void AnimateMovement(Vector2 velocity,float deltaTime)
        {
            if(deltaTime<=0)return;
            Facing=DirectionalCharacterVisual.ResolveFacing(velocity,Facing);
            float cadence=_key=="mammoth"?.65f:_key=="bear"?.8f:_key=="rabbit"||_key=="snow_hare"?1.15f:1f;
            _cycle.Tick(velocity*cadence,deltaTime);Render(_cycle.Moving);
        }
        private void Render(bool moving)
        {
            if(_renderer==null||string.IsNullOrEmpty(_key))return;
            var sprite=SpriteFor(_key,Facing,moving,_cycle.Frame);
            if(sprite!=null)_renderer.sprite=sprite;
            _renderer.flipX=Facing==CharacterFacing.Left;
        }
        private void LateUpdate()
        {
            if(Time.timeScale<=0)return;
            bool stopped=(_animal!=null&&_animal.IsDead)||MeleeKnockback.IsActive(gameObject);
            AnimateMovement(!stopped&&_body!=null?_body.velocity:Vector2.zero,Time.deltaTime);
        }
    }
}
