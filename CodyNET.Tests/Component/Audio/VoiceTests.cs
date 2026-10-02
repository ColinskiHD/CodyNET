using CodyNET.Core.Cody;
using CodyNET.Core.Devices;//decide if .Devices.Audio is better
using NUnit.Framework;

namespace CodyNET.Tests.Component;

public class VoiceTests
{
    private const ushort VOICE1_BASE = 0xD400;

    [Test]
    public void Voice_CombineFrequencyLOandHIBytes()
    {
        byte[] _voiceMemory = new byte[0x100];
        Voice voice = new Voice(0xD400, _voiceMemory, 0x00);

        _voiceMemory[0x00] = 0xD6;//LO BYTE
        _voiceMemory[0x01] = 0x1C;//HI BYTE

        Assert.That(voice.Frequency, Is.EqualTo(0x1CD6));
    }

    [Test]
    public void Voice_CombinePwmLOandHIBytes()
    {
        byte[] _voiceMemory = new byte[0x100];
        Voice voice = new Voice(0xD400, _voiceMemory, 0x00);

        _voiceMemory[0x02] = 0xD6;//LO BYTE
        _voiceMemory[0x03] = 0x1C;//HI BYTE

        Assert.That(voice.Pwm, Is.EqualTo(0x1CD6));
    }

    [Test]
    public void Voice_ControlBitsSet()
    {
        byte[] _voiceMemory = new byte[0x100];
        Voice voice = new Voice(0xD400, _voiceMemory, 0x00);

        _voiceMemory[0x04] = 0xFF;//all bits set to 1

        Assert.That(voice.Gate, Is.EqualTo(true));
        Assert.That(voice.Sync, Is.EqualTo(true));
        Assert.That(voice.RingMod, Is.EqualTo(true));
        Assert.That(voice.Reset, Is.EqualTo(true));
        Assert.That(voice.Triangle, Is.EqualTo(true));
        Assert.That(voice.Saw, Is.EqualTo(true));
        Assert.That(voice.Pulse, Is.EqualTo(true));
        Assert.That(voice.Noise, Is.EqualTo(true));
    }

    [Test]
    public void Voice_ControlBitsNotSet()
    {
        byte[] _voiceMemory = new byte[0x100];
        Voice voice = new Voice(0xD400, _voiceMemory, 0x00);

        _voiceMemory[0x04] = 0x00;//all bits set to 0

        Assert.That(voice.Gate, Is.EqualTo(false));
        Assert.That(voice.Sync, Is.EqualTo(false));
        Assert.That(voice.RingMod, Is.EqualTo(false));
        Assert.That(voice.Reset, Is.EqualTo(false));
        Assert.That(voice.Triangle, Is.EqualTo(false));
        Assert.That(voice.Saw, Is.EqualTo(false));
        Assert.That(voice.Pulse, Is.EqualTo(false));
        Assert.That(voice.Noise, Is.EqualTo(false));
    }
    [Test]
    public void Voice_ControlBitsSomeSet()
    {
        byte[] _voiceMemory = new byte[0x100];
        Voice voice = new Voice(0xD400, _voiceMemory, 0x00);

        _voiceMemory[0x04] = 0x55;//01010101

        Assert.That(voice.Gate, Is.EqualTo(true));
        Assert.That(voice.Sync, Is.EqualTo(false));
        Assert.That(voice.RingMod, Is.EqualTo(true));
        Assert.That(voice.Reset, Is.EqualTo(false));
        Assert.That(voice.Triangle, Is.EqualTo(true));
        Assert.That(voice.Saw, Is.EqualTo(false));
        Assert.That(voice.Pulse, Is.EqualTo(true));
        Assert.That(voice.Noise, Is.EqualTo(false));
    }

}