using System.Runtime.CompilerServices;
using Avalonia.Metadata;
using CodyNET.Core.Cody;
using CodyNET.Core.Devices;//decide if .Devices.Audio is better
using JetBrains.Annotations;
using NUnit.Framework;

namespace CodyNET.Tests.Component;

public class VoiceTests
{
    private const ushort SID_BASE = 0xD400;
    private const ushort V1_OFFSET = 0x00;
    private const ushort V2_OFFSET = 0x07;
    private const ushort V3_OFFSET = 0x0E;

    [TestCase(V2_OFFSET, TestName = "{m} (Voice 2)")]
    public void Voice_CombineFrequencyLOandHIBytes(ushort offset)
    {
        byte[] _voiceMemory = new byte[0x100];
        Voice voice = new Voice(SID_BASE, _voiceMemory, offset);

        _voiceMemory[0x00 + offset] = 0xD6;//LO BYTE
        _voiceMemory[0x01 + offset] = 0x1C;//HI BYTE

        Assert.That(voice.Frequency, Is.EqualTo(0x1CD6));
    }

    [TestCase(V2_OFFSET, TestName = "{m} (Voice 2)")]
    public void Voice_CombinePwmLOandHIBytes(ushort offset)
    {
        byte[] _voiceMemory = new byte[0x100];
        Voice voice = new Voice(SID_BASE, _voiceMemory, offset);

        _voiceMemory[0x02 + offset] = 0xD6;//LO BYTE
        _voiceMemory[0x03 + offset] = 0x1C;//HI BYTE

        Assert.That(voice.Pwm, Is.EqualTo(0x1CD6));
    }

    [TestCase(V2_OFFSET, 0xFF, new bool[] { true, true, true, true, true, true }, TestName = "Voice_ControlBitsSet (Voice 2)")]
    [TestCase(V2_OFFSET, 0x00, new bool[] { false, false, false, false, false, false }, TestName = "Voice_ControlBitsNotSet (Voice 2)")]
    [TestCase(V2_OFFSET, 0x55, new bool[] { true, false, true, false, true, false }, TestName = "Voice_ControlBitsSomeSet (Voice 2)")]
    public void Voice_ControlBitsSet(ushort offset, int values, bool[] expected)
    {
        byte[] _voiceMemory = new byte[0x100];
        Voice voice = new Voice(SID_BASE, _voiceMemory, offset);

        _voiceMemory[0x04 + offset] = (byte)values;

        Assert.That(new bool[] { voice.Gate, voice.Reset, voice.Triangle, voice.Saw, voice.Pulse, voice.Noise }, Is.EqualTo(expected));
    }

    [TestCase(V2_OFFSET, TestName = "{m} (Voice 2)")]
    public void Voice_Attack_Decay(ushort offset)
    {
        byte[] _voiceMemory = new byte[0x100];
        Voice voice = new Voice(SID_BASE, _voiceMemory, offset);

        _voiceMemory[0x05 + offset] = 0x67;

        Assert.That(voice.Attack, Is.EqualTo(0x06));
        Assert.That(voice.Decay, Is.EqualTo(0x07));
    }

    [TestCase(V2_OFFSET, TestName = "{m} (Voice 2)")]
    public void Voice_Sustain_Release(ushort offset)
    {
        byte[] _voiceMemory = new byte[0x100];
        Voice voice = new Voice(SID_BASE, _voiceMemory, offset);

        _voiceMemory[0x06 + offset] = 0x67;

        Assert.That(voice.Sustain, Is.EqualTo(0x06));
        Assert.That(voice.Release, Is.EqualTo(0x07));
    }

    [Test]
    public void Voice_TestIndepentVoices()
    {
        byte[] _voiceMemory = new byte[0x100];
        Voice voice1 = new Voice(SID_BASE, _voiceMemory, V1_OFFSET);
        Voice voice2 = new Voice(SID_BASE, _voiceMemory, V2_OFFSET);
        Voice voice3 = new Voice(SID_BASE, _voiceMemory, V3_OFFSET);

        _voiceMemory[0x00 + V1_OFFSET] = 0x01;
        _voiceMemory[0x00 + V2_OFFSET] = 0x02;
        _voiceMemory[0x00 + V3_OFFSET] = 0x03;

        Assert.That(voice1.Frequency, Is.EqualTo(0x0001));
        Assert.That(voice2.Frequency, Is.EqualTo(0x0002));
        Assert.That(voice3.Frequency, Is.EqualTo(0x0003));
    }

}