using CodyNET.Core.Cody;
using CodyNET.Core.Devices;//decide if .Devices.Audio is better
using CodyNET.Core.Interfaces;
using NUnit.Framework;

namespace CodyNET.Tests.Component;

public class SidTests
{
    private const ushort SID_BASE = 0xD400;
    private const ushort FREQ_LO = 0x00;
    private const ushort FREQ_HI = 0x01;
    private const ushort PWM_LO = 0x02;
    private const ushort PWM_HI = 0x03;
    private const ushort CONTROL = 0x04;
    private const ushort ATTACK_DECAY = 0x05;
    private const ushort SUSTAIN_RELEASE = 0x06;
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
        sid.Write((ushort)(SID_BASE + CONTROL + voiceAIndex * 7), 0x02);
        SetFrequency(sid, voiceBIndex, 0xFFFF);
        sid.Update(cycle += 1000);//16 samples

        Assert.That(sid.Voices[voiceAIndex].Phase, Is.EqualTo(0));
    }

    [TestCase(0, 2, TestName = "Sid_RingMod_Voice1")]
    [TestCase(1, 0, TestName = "Sid_RingMod_Voice2")]
    [TestCase(2, 1, TestName = "Sid_RingMod_Voice3")]
    public void Sid_RingMod(int voiceAIndex, int voiceBIndex)
    {
        long cycle = 0;
        var memorya = new Memory();
        var memoryb = new Memory();
        var sidRingMod = CreateSoundInterfaceDevice(memorya);
        var sidNoRingMod = CreateSoundInterfaceDevice(memoryb);

        SetFrequency(sidRingMod, voiceAIndex, 0xFFFF);
        SetFrequency(sidNoRingMod, voiceAIndex, 0xFFFF);
        sidRingMod.Write((ushort)(SID_BASE + CONTROL + voiceAIndex * 7), 0x10);//set Triangle bit
        sidNoRingMod.Write((ushort)(SID_BASE + CONTROL + voiceAIndex * 7), 0x10);//set Triangle bit
        sidRingMod.Update(cycle += 1000);
        sidNoRingMod.Update(cycle);

        SetFrequency(sidRingMod, voiceAIndex, 0);
        SetFrequency(sidNoRingMod, voiceAIndex, 0);
        sidRingMod.Write((ushort)(SID_BASE + CONTROL + voiceAIndex * 7), 0x14);//set RingMod bit
        SetFrequency(sidRingMod, voiceBIndex, 0xFFFF);
        SetFrequency(sidNoRingMod, voiceBIndex, 0xFFFF);
        sidRingMod.Update(cycle += 1000);
        sidNoRingMod.Update(cycle);
        Assume.That(sidRingMod.Voices[voiceBIndex].Phase, Is.EqualTo(sidNoRingMod.Voices[voiceBIndex].Phase));
        Assume.That(sidRingMod.Voices[voiceBIndex].Phase, Is.GreaterThan(0x8000));

        Assert.That(sidRingMod.Voices[voiceAIndex].Wave, Is.EqualTo(0xFFFF - sidNoRingMod.Voices[voiceAIndex].Wave));
    }

    [TestCase(2, 0x1111, 0xD41B, 0x7FF8, 0x7F, TestName = "Read_Voice3_Osc")]
    public void Read_Register(int voiceIndex, int frequencyValue, int address, int expectedWave, int expectedByte)
    {
        long cycle = 0;
        var memory = new Memory();
        var sid = CreateSoundInterfaceDevice(memory);

        SetFrequency(sid, voiceIndex, (ushort)frequencyValue);
        sid.Write((ushort)(SID_BASE + CONTROL + voiceIndex * 7), 0x10);
        sid.Update(cycle += 1000);
        Assume.That(sid.Voices[voiceIndex].Wave, Is.EqualTo(expectedWave));

        Assert.That(sid.Read((ushort)address), Is.EqualTo(expectedByte));
    }

    [TestCase(2, 0x00, 0xF0, 0xD41C, 0x5555, 0x55, TestName = "Read_Voice3_Env_MAX")]
    [TestCase(2, 0x00, 0x00, 0xD41C, 0x0000, 0x00, TestName = "Read_Voice3_Env_MIN")]
    [TestCase(2, 0x00, 0x80, 0xD41C, 0x2D82, 0x2D, TestName = "Read_Voice3_Env_DECAY")]
    public void Read_Register_Env(int voiceIndex, int attackDecay, int sustainRelease, int address, int expectedEnv, int expectedByte)
    {
        long cycle = 0;
        var memory = new Memory();
        var sid = CreateSoundInterfaceDevice(memory);

        sid.Write((ushort)(SID_BASE + ATTACK_DECAY + voiceIndex * 7), (byte)attackDecay);
        sid.Write((ushort)(SID_BASE + SUSTAIN_RELEASE + voiceIndex * 7), (byte)sustainRelease);
        sid.Write((ushort)(SID_BASE + CONTROL + voiceIndex * 7), 0x11);// set Triangle + Gate Bit
        sid.Update(cycle += 100_000);
        Assume.That(sid.Voices[voiceIndex].Envelope, Is.EqualTo(expectedEnv));

        Assert.That(sid.Read((ushort)address), Is.EqualTo(expectedByte));
    }

    private static SoundInterfaceDevice CreateSoundInterfaceDevice(Memory memory)
    {
        var audioOutput = new AudioTestOutput();
        var sid = new SoundInterfaceDevice(audioOutput);
        memory.RegisterDevice(sid);
        return sid;
    }

}