using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class DialoguePortraitTests
    {
        [Test] public void EveryCharacterHasADistinctDialogueIllustration()
        {
            var portraits=new HashSet<Texture2D>();
            foreach(var role in new[]{"Hero","Hunter","Warrior","Farmer","Cook","Healer","Aunt","Uncle","Child"})
            {
                var portrait=DialoguePortraits.ForRole(role);
                Assert.That(portrait,Is.Not.Null,role);
                Assert.That(portrait.width,Is.GreaterThanOrEqualTo(512));
                Assert.That(portraits.Add(portrait),Is.True,role+" must use its own illustration");
            }
        }
        [TestCase("Hunter","사냥꾼")][TestCase("Warrior","전사")]
        [TestCase("Farmer","농부")][TestCase("Cook","요리사")][TestCase("Healer","치유사")]
        [TestCase("Child","아이")][TestCase("Aunt","Elder")]
        public void LocalizedAndLegacyRolesKeepTheCorrectIdentity(string english,string alias)
        { Assert.That(DialoguePortraits.ForRole(english),Is.Not.Null);Assert.That(DialoguePortraits.ForRole(alias),Is.SameAs(DialoguePortraits.ForRole(english))); }
    }
}
