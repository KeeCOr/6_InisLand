using NUnit.Framework;
namespace IL6.Tests
{
    public class HudButtonSpacingTests
    {
        [Test] public void WeaponButtonsReserveASeparateCaptionRow()
        {
            for(int slot=0;slot<3;slot++)
            {
                var r=MobileHudLayout.ActionButton(slot);
                Assert.That(r.height-r.width,Is.GreaterThanOrEqualTo(20),"Icon and caption need separate space");
            }
        }
    }
}
