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
        _voiceMemory[0x00] = 0xD6;//LO BYTE
        _voiceMemory[0x01] = 0x1C;//HI BYTE

        Voice voice = new Voice(0xD400, _voiceMemory, 0x00);

        Assert.That(voice.Frequency, Is.EqualTo(0x1CD6));
    }
}