using System;
using System.IO;
using NUnit.Framework;
namespace IL6.Tests
{
    public class SaveSlotTests
    {
        private string _previous,_directory;private int _slot;
        [SetUp] public void Setup()
        {
            _previous=Environment.GetEnvironmentVariable("IL6_TEST_SAVE_DIRECTORY");_slot=SaveLoad.ActiveSlot;
            _directory=Path.Combine(Path.GetTempPath(),"IL6-slots-"+Guid.NewGuid());
            Directory.CreateDirectory(_directory);Environment.SetEnvironmentVariable("IL6_TEST_SAVE_DIRECTORY",_directory);
        }
        [TearDown] public void Cleanup()
        {Environment.SetEnvironmentVariable("IL6_TEST_SAVE_DIRECTORY",_previous);SaveLoad.SelectSlot(_slot);Directory.Delete(_directory,true);}
        [Test] public void ThreeSlotsRemainIndependentAcrossSaveLoadAndClear()
        {
            for(int i=1;i<=3;i++){SaveLoad.SelectSlot(i);Assert.That(SaveLoad.Save(new SaveFileV1{currentDay=i*3}),Is.True);}
            for(int i=1;i<=3;i++)Assert.That(SaveLoad.LoadSlot(i).currentDay,Is.EqualTo(i*3));
            SaveLoad.SelectSlot(2);SaveLoad.Clear();Assert.That(SaveLoad.LoadSlot(2),Is.Null);
            Assert.That(SaveLoad.LoadSlot(1).currentDay,Is.EqualTo(3));Assert.That(SaveLoad.LoadSlot(3).currentDay,Is.EqualTo(9));
        }
        [Test] public void LegacySaveIsSlotOneAndInspectingOtherSlotsDoesNotSelectThem()
        {
            File.WriteAllText(Path.Combine(_directory,SaveLoad.FileName),"{\"version\":1,\"currentDay\":7}");
            SaveLoad.SelectSlot(3);Assert.That(SaveLoad.LoadSlot(1).currentDay,Is.EqualTo(7));Assert.That(SaveLoad.ActiveSlot,Is.EqualTo(3));
            Assert.That(SaveLoad.LoadSlot(2),Is.Null);Assert.That(SaveLoad.SlotCount,Is.EqualTo(3));
            Assert.Throws<ArgumentOutOfRangeException>(()=>SaveLoad.SelectSlot(4));Assert.That(SaveLoad.ActiveSlot,Is.EqualTo(3));
        }
        [Test] public void InvalidSaveCannotContinueAndIsNotDeleted()
        {
            var path=SaveLoad.SlotPath(2);File.WriteAllText(path,"{\"version\":99,\"currentDay\":7}");
            Assert.That(SaveLoad.LoadSlot(2),Is.Null);Assert.That(File.Exists(path),Is.True);
        }
    }
}
