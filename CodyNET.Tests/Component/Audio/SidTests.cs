using CodyNET.Core.Cody;
using CodyNET.Core.Devices;//decide if .Devices.Audio is better
using CodyNET.Core.Interfaces;
using NUnit.Framework;

namespace CodyNET.Tests.Component;

public class SidTests
{
    private const ushort SID_BASE = 0xD400;
    private SoundInterfaceDevice _sid = null!;

    [Test]
    public void Sid_RegisterWriteRead()
    {
        var memory = new Memory();
        var sid = CreateSoundInterfaceDevice(memory);

        sid.Write(0xD400, 0x05);

        Assert.That(sid.Read(0xD400), Is.EqualTo(0x05));
    }

    [Test]
    public void Sid_Voices()
    {
        var memory = new Memory();
        var sid = CreateSoundInterfaceDevice(memory);

        sid.Write(0xD40E, 0xD6);
        sid.Write(0xD40F, 0x1C);

        Assert.That(sid.Voices[2].Frequency, Is.EqualTo(0x1CD6));
    }

    private class AudioTestOutput : IAudioOutput
    {
        private long _sampleCount = 0;
        public void RenderSample(short samples)
        {
            _sampleCount++;
            return;
        }
        public long SampleCounter()
        {
            return _sampleCount;
        }
    }

    [Test]
    public void Sid_62_Cycles_No_Sample()
    {
        var audioOutput = new AudioTestOutput();
        var sid = new SoundInterfaceDevice(audioOutput);

        sid.Update(62);

        Assert.That(audioOutput.SampleCounter, Is.EqualTo(0));
    }

    [Test]
    public void Sid_63_Cycles_1_Sample()
    {
        var audioOutput = new AudioTestOutput();
        var sid = new SoundInterfaceDevice(audioOutput);

        sid.Update(63);

        Assert.That(audioOutput.SampleCounter, Is.EqualTo(1));
    }

    [Test]
    public void Sid_125_Cycles_2_Samples()
    {
        var audioOutput = new AudioTestOutput();
        var sid = new SoundInterfaceDevice(audioOutput);

        sid.Update(125);

        Assert.That(audioOutput.SampleCounter, Is.EqualTo(2));
    }

    [Test]
    public void Sid_16_000_Samples_per_Second()
    {
        var audioOutput = new AudioTestOutput();
        var sid = new SoundInterfaceDevice(audioOutput);

        sid.Update(1_000_000);

        Assert.That(audioOutput.SampleCounter, Is.EqualTo(16_000));
    }

    private static SoundInterfaceDevice CreateSoundInterfaceDevice(Memory memory)
    {
        var audioOutput = new AudioTestOutput();
        var sid = new SoundInterfaceDevice(audioOutput);
        memory.RegisterDevice(sid);
        return sid;
    }

}