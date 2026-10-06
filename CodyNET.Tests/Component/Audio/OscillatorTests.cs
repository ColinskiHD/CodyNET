using CodyNET.Core.Devices.Audio;
using NUnit.Framework;

namespace CodyNET.Tests.Component;

public class OscillatorTests
{
    [Test]
    public void Osc_0Hz_Saw()
    {
        var osc = new Oscillator();

        osc.Step(0, Waveform.Sawtooth);

        Assert.That(osc.Phase, Is.EqualTo(0));
    }

    [Test]
    public void Osc_3Hz_Saw_should_Shift_To_Zero()
    {
        var osc = new Oscillator();

        osc.Step(3, Waveform.Sawtooth);

        Assert.That(osc.Phase, Is.EqualTo(0));
    }

    [Test]
    public void Osc_440Hz_Saw()
    {
        var osc = new Oscillator();

        osc.Step(7382, Waveform.Sawtooth);

        Assert.That(osc.Phase, Is.EqualTo(1845));
    }

    [Test]
    public void Osc_No_Overflow()
    {
        var osc = new Oscillator();

        osc.Step(0xFFFF, Waveform.Sawtooth);
        osc.Step(0xFFFF, Waveform.Sawtooth);
        osc.Step(0xFFFF, Waveform.Sawtooth);
        osc.Step(0xFFFF, Waveform.Sawtooth);
        osc.Step(0x000F, Waveform.Sawtooth);//reached 65535 = 0xFFFF

        Assert.That(osc.Overflowed, Is.EqualTo(false));
    }

    [Test]
    public void Osc_Overflow()
    {
        var osc = new Oscillator();

        osc.Step(0xFFFF, Waveform.Sawtooth);
        osc.Step(0xFFFF, Waveform.Sawtooth);
        osc.Step(0xFFFF, Waveform.Sawtooth);
        osc.Step(0xFFFF, Waveform.Sawtooth);
        osc.Step(0x0010, Waveform.Sawtooth);//reached 65536 > 0xFFFF

        Assert.That(osc.Overflowed, Is.EqualTo(true));
    }

    [Test]
    public void Osc_Zero_After_One_Phase_Cycle()
    {
        var osc = new Oscillator();

        osc.Step(0xFFFF, Waveform.Sawtooth);
        osc.Step(0xFFFF, Waveform.Sawtooth);
        osc.Step(0xFFFF, Waveform.Sawtooth);
        osc.Step(0xFFFF, Waveform.Sawtooth);
        osc.Step(0x0010, Waveform.Sawtooth);//reached 65536 > 0xFFFF

        Assert.That(osc.Phase, Is.EqualTo(0));
    }
}