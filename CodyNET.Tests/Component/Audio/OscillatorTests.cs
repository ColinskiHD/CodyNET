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
    public void Osc_3Hz_Phase_should_Shift_To_Zero()
    {
        var osc = new Oscillator();

        osc.Step(3, Waveform.None);

        Assert.That(osc.Phase, Is.EqualTo(0));
    }

    [Test]
    public void Osc_440Hz_Phase()
    {
        var osc = new Oscillator();

        osc.Step(7382, Waveform.None);

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

        osc.Step(0xFFFF, Waveform.None);
        osc.Step(0xFFFF, Waveform.None);
        osc.Step(0xFFFF, Waveform.None);
        osc.Step(0xFFFF, Waveform.None);
        osc.Step(0x0010, Waveform.None);//reached 65536 > 0xFFFF

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

    [Test]
    public void Osc_440Hz_Triangle()
    {
        var osc = new Oscillator();

        osc.Step(7382, Waveform.Triangle);

        osc.Step(0x0000, Waveform.Triangle);//only needed to update Wave. Has no effect on test


        Assert.That(osc.Wave, Is.EqualTo(3690));
    }

    [Test]
    public void Osc_Triangle_Peaks_At_Half_Phase()
    {
        var osc = new Oscillator();

        osc.Step(0xFFFF, Waveform.Triangle);
        osc.Step(0xFFFF, Waveform.Triangle);
        osc.Step(0x0008, Waveform.Triangle);//Phase at 0x8000

        osc.Step(0x0000, Waveform.Triangle);//only needed to update Wave. Has no effect on test

        Assert.That(osc.Wave, Is.EqualTo(0xFFFF));
    }

    [Test]
    public void Osc_Triangle_Peaks_At_Full_Phase()
    {
        var osc = new Oscillator();

        osc.Step(0xFFFF, Waveform.Sawtooth);
        osc.Step(0xFFFF, Waveform.Sawtooth);
        osc.Step(0xFFFF, Waveform.Sawtooth);
        osc.Step(0xFFFF, Waveform.Sawtooth);
        osc.Step(0x0010, Waveform.Sawtooth);//Phase at 0xFFFF

        osc.Step(0x0000, Waveform.Triangle);//only needed to update Wave. Has no effect on test

        Assert.That(osc.Wave, Is.EqualTo(0x0000));
    }

    [Test]
    public void Osc_440Hz_Saw()
    {
        var osc = new Oscillator();

        osc.Step(7382, Waveform.Sawtooth);
        osc.Step(7382, Waveform.Sawtooth);

        Assert.That(osc.Wave, Is.EqualTo(1845));
    }
}