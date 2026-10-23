using CodyNET.Core.Devices.Audio;
using JetBrains.Annotations;
using NUnit.Framework;

namespace CodyNET.Tests.Component;

public class OscillatorTests
{

    private static void Step(Oscillator osc, int frequency, Waveform waveform, ushort noise = 0, ushort pwm = 0)
    {
        osc.Step((ushort)frequency, waveform, noise, pwm);
    }

    private static void StepPhaseTo(Oscillator osc, int phase, Waveform waveform, ushort noise = 0, ushort pwm = 0)
    {
        int remaining = phase;
        while (remaining > 0xFFFF)
        {
            Step(osc, 0xFFFF, waveform, noise, pwm);
            remaining -= 0xFFFF;
        }
        Step(osc, (ushort)remaining, waveform, noise, pwm);
    }

    private static void updatePhase(Oscillator osc, Waveform waveform)
    {
        Step(osc, 0x0000, waveform);//only needed because values are updated after Step.
    }
    [TestCase(0, 0, TestName = "Osc_0Hz_Saw")]
    [TestCase(3, 0, TestName = "Osc_3Hz_Phase_should_Shift_To_Zero")]
    [TestCase(7382, 1845, TestName = "Osc_440Hz_Phase")]
    public void Phase_Test_After_One_Step(int frequency, int expectedPhase)
    {
        var osc = new Oscillator();

        Step(osc, (ushort)frequency, Waveform.Sawtooth);

        Assert.That(osc.Phase, Is.EqualTo(expectedPhase));
    }

    [TestCase(0x000F, false, TestName = "No_Overflow")]
    [TestCase(0x0010, true, TestName = "Overflow")]
    public void Overflow_Test(int frequency, bool expected)
    {
        var osc = new Oscillator();
        for (int i = 0; i < 4; i++)
        {
            Step(osc, 0xFFFF, Waveform.None);
        }
        Step(osc, (ushort)frequency, Waveform.Sawtooth);
        Assert.That(osc.Overflowed, Is.EqualTo(expected));
    }

    [TestCase(0x0010, 0, TestName = "Osc_Zero_After_One_Phase_Cycle")]
    public void OverflowTestPhaseReset(int frequency, int expected)
    {
        var osc = new Oscillator();
        for (int i = 0; i < 4; i++)
        {
            Step(osc, 0xFFFF, Waveform.None);
        }
        Step(osc, (ushort)frequency, Waveform.Sawtooth);
        Assert.That(osc.Phase, Is.EqualTo(expected));
    }




    /*[TestCase(0x8000, 0xFFFF, TestName = "Triangle_Peaks_At_Half_Phase")]
    public void PhaseTest(int frequency, int expected)
    {
        var osc = new Oscillator();

        StepPhaseTo(osc, frequency, Waveform.Triangle);

        Step(osc, 0x0000, Waveform.Triangle);//only needed to update Wave. Has no effect on test


        Assert.That(osc.Wave, Is.EqualTo(expected));
    }*/

    /*[Test]
    public void Osc_440Hz_Triangle()
    {
        var osc = new Oscillator();

        Step(osc, 7382, Waveform.Triangle);
        updatePhase(osc);

        Assert.That(osc.Wave, Is.EqualTo(3690));
    }
*/
    [TestCase(Waveform.Triangle, 0xFFFF, TestName = "Triangle_Is_OxFFFF_At_Half_Phase")]
    public void Osc_Triangle_Peaks_At_Half_Phase(Waveform waveform, int expected)
    {
        var osc = new Oscillator();

        Step(osc, 0xFFFF, waveform);
        Step(osc, 0xFFFF, waveform);
        Step(osc, 0x0008, waveform);//Phase at 0x8000

        updatePhase(osc, waveform);

        Assert.That(osc.Wave, Is.EqualTo(expected));
    }

    [TestCase(Waveform.Triangle, 0x0000, TestName = "Triangle_Is_0x0000_At_Full_Phase")]
    public void Osc_Triangle_Peaks_At_Full_Phase(Waveform waveform, int expected)//TODO: this uses saw not triangle??
    {
        var osc = new Oscillator();

        Step(osc, 0xFFFF, waveform);
        Step(osc, 0xFFFF, waveform);
        Step(osc, 0xFFFF, waveform);
        Step(osc, 0xFFFF, waveform);
        Step(osc, 0x0010, waveform);//Phase at 0xFFFF

        updatePhase(osc, waveform);//only needed to update Wave. Has no effect on test

        Assert.That(osc.Wave, Is.EqualTo(expected));
    }

    [TestCase(Waveform.Triangle, 3690, TestName = "Osc_440Hz_Triangle")]
    [TestCase(Waveform.Sawtooth, 1845, TestName = "Osc_440Hz_Saw")]
    public void Osc_440Hz(Waveform waveform, int expected)
    {
        var osc = new Oscillator();

        Step(osc, 7382, waveform);
        updatePhase(osc, waveform);

        Assert.That(osc.Wave, Is.EqualTo(expected));
    }
}