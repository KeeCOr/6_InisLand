using NUnit.Framework;
namespace IL6.Tests
{
    public class CharacterResourceIntegrityTests
    {
        [Test] public void AllRequiredCharacterViewsPortraitsAndUiTexturesExist()
        {
            System.Type guard=null;
            foreach(var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                if((guard=assembly.GetType("IL6.EditorBuild.CharacterResourceBuildGuard"))!=null) break;
            Assert.That(guard,Is.Not.Null);
            Assert.That(guard.GetMethod("Missing").Invoke(null,null),Is.Empty,"Required Resources paths cannot be deleted just because an archive PNG has matching bytes.");
        }
    }
}
