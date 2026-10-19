using CodyNET.Core.Devices.Audio;
using NUnit.Framework;

namespace CodyNET.Tests.Component;

public class MixerTests
{
    [TestCase(0, 16, 0, TestName = "MainVolume_0")]
    [TestCase(8, 16, 8, TestName = "MainVolume_8")]
    [TestCase(15, 16, 15, TestName = "MainVolume_15")]
    public void MainVolume(int volume, int inputVol, int expectedOutputVol)
    {

        Mixer mixer = new Mixer();

        short result = mixer.Step((ushort)inputVol, 0, 0, (ushort)volume);

        Assert.That(result + 0x8000, Is.EqualTo(expectedOutputVol));
    }
}