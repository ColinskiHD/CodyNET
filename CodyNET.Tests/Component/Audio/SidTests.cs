using CodyNET.Core.Cody;
using CodyNET.Core.Devices;//decide if .Devices.Audio is better
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

    private static SoundInterfaceDevice CreateSoundInterfaceDevice(Memory memory)
    {
        var sid = new SoundInterfaceDevice();
        memory.RegisterDevice(sid);
        return sid;
    }

}