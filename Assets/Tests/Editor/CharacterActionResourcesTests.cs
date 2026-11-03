using NUnit.Framework;
using UnityEngine;

namespace IL6.Tests
{
    public class CharacterActionResourcesTests
    {
        [Test] public void AllElevenCharacterSetsHaveDirectionalStartLoopEndActionFrames()
        {
            foreach(string key in new[]{"hero_axe","hero_bow","hero_hammer","hunter","warrior","farmer","cook","healer","aunt","uncle","child"})
            {
                var sprites=Resources.LoadAll<Sprite>("Sprites/Characters/Combat20261103/"+key);
                Assert.That(sprites.Length,Is.EqualTo(9),key);
                foreach(string direction in new[]{"side","front","back"})foreach(string phase in new[]{"Start","Loop","End"})
                {
                    Sprite found=null;foreach(var sprite in sprites)if(sprite.name=="attack_"+direction+"_"+phase)found=sprite;
                    Assert.That(found,Is.Not.Null,key+" "+direction+" "+phase);
                    Assert.That(found.texture.filterMode,Is.EqualTo(FilterMode.Bilinear));
                }
            }
        }
    }
}
