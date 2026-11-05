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

    private void SetFrequency(SoundInterfaceDevice sid, int voiceIndex, ushort frequency)
    {
        ushort address = (ushort)(SID_BASE + voiceIndex * 7);
        sid.Write(address, (byte)(frequency & 0xFF));
        sid.Write((ushort)(address + 1), (byte)(frequency >> 8));

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

    [TestCase(0, 2, TestName = "Sid_Sync_Voice1")]
    [TestCase(1, 0, TestName = "Sid_Sync_Voice2")]
    [TestCase(2, 1, TestName = "Sid_Sync_Voice3")]
    public void Sid_Sync(int voiceAIndex, int voiceBIndex)
    {
        long cycle = 0;
        var memory = new Memory();
        var sid = CreateSoundInterfaceDevice(memory);

        SetFrequency(sid, voiceAIndex, 0xFFFF);
        sid.Update(cycle += 1000);//16 samples

        SetFrequency(sid, voiceAIndex, 0);
        sid.Write((ushort)(SID_BASE + 4 + voiceAIndex * 7), 0x02);
        SetFrequency(sid, voiceBIndex, 0xFFFF);
        sid.Update(cycle += 1000);//16 samples

        Assert.That(sid.Voices[voiceAIndex].Phase, Is.EqualTo(0));
    }


    private static SoundInterfaceDevice CreateSoundInterfaceDevice(Memory memory)
    {
        var audioOutput = new AudioTestOutput();
        var sid = new SoundInterfaceDevice(audioOutput);
        memory.RegisterDevice(sid);
        return sid;
    }

}