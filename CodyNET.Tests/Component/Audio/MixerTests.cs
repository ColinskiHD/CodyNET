using CodyNET.Core.Devices.Audio;
using NUnit.Framework;

namespace CodyNET.Tests.Component;

public class MixerTests
{
    [TestCase(0, 0x10, 0, TestName = "MainVolume_0")]
    [TestCase(0x08, 0x10, 8, TestName = "MainVolume_8")]
    [TestCase(0x0F, 0x10, 0x0F, TestName = "MainVolume_15")]
    public void MainVolume(int volumeControl, int Voice1inputVol, int expectedOutputVol)
    {

        Mixer mixer = new Mixer();

        short result = mixer.Step((ushort)Voice1inputVol, 0, 0, (ushort)volumeControl);

        Assert.That(result + 0x8000, Is.EqualTo(expectedOutputVol));
    }

    [TestCase(0x8F, 0x10, 0, TestName = "VolumeControlVoice3Off")]
    [TestCase(0x0F, 0x10, 0x0F, TestName = "VolumeControlVoice3On")]
    public void Voice3Off(int volumeControl, int Voice3inputVol, int expectedOutputVol)
    {

        Mixer mixer = new Mixer();

        short result = mixer.Step(0, 0, (ushort)Voice3inputVol, (ushort)volumeControl);

        Assert.That(result + 0x8000, Is.EqualTo(expectedOutputVol));
    }
}