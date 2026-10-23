using CodyNET.Core.Devices.Audio;
using NUnit.Framework;

namespace CodyNET.Tests.Component;

public class EnvelopeGeneratorTests
{

    private const int MAX_LEVEL = 0xFFFFFF / 3;
    private static void Step(EnvelopeGenerator envelopeGenerator, bool gate, ushort attack = 0, ushort decay = 0, ushort sustain = 0, ushort release = 0)
    {
        envelopeGenerator.Step(attack, decay, sustain, release, gate);
    }

    private static EnvelopeGenerator DecayStateEnvelope()
    {
        var envelopeGenerator = new EnvelopeGenerator();

        for (int i = 0; i <= 32; i++)
        {
            Step(envelopeGenerator, true, 0, 0, 0, 0);
        }
        return envelopeGenerator;
    }

    private static EnvelopeGenerator SustainStateEnvelope()
    {
        var envelopeGenerator = DecayStateEnvelope();

        for (int i = 0; i <= 16 * 6; i++)
        {
            Step(envelopeGenerator, true, 0, 0, 15, 0);
        }
        return envelopeGenerator;//_adsrState == ADSRSTATE.Decay
    }

    [Test]
    public void Amplitude0()
    {
        var envelopeGenerator = new EnvelopeGenerator();

        Assert.That(envelopeGenerator.Amplitude, Is.EqualTo(0x0000));
    }
    [TestCase(0, 0x02AAAA, TestName = "Attack_0")]
    [TestCase(15, 0x00002B, TestName = "Attack_15")]
    public void Add_Attack(int attack, int expected)
    {
        var envelopeGenerator = new EnvelopeGenerator();

        Step(envelopeGenerator, true, (ushort)attack, 0, 0, 0);

        Assert.That(envelopeGenerator.Amplitude, Is.EqualTo(expected));
    }

    [Test]
    public void AttackMaximum()
    {
        var envelopeGenerator = new EnvelopeGenerator();

        for (int i = 0; i <= 32; i++)
        {
            Step(envelopeGenerator, true, 0, 0, 0, 0);
        }

        Assert.That(envelopeGenerator.Amplitude, Is.EqualTo(MAX_LEVEL));
    }

    [TestCase(0, 0, 0x5471C7, TestName = "Decay_0_Sustain_0")]
    [TestCase(15, 0, 0x555547, TestName = "Decay_15_Sustain_0")]
    [TestCase(0, 15, 0x555555, TestName = "Decay_15_Sustain_15")]
    public void SubDecay(int decay, int sustain, int expected)
    {
        var envelopeGenerator = DecayStateEnvelope();

        Step(envelopeGenerator, true, 0, (ushort)decay, (ushort)sustain, 0);

        Assert.That(envelopeGenerator.Amplitude, Is.EqualTo(expected));
    }

    [TestCase(15, 0x555555, TestName = "Hold_Sustain_15")]
    [TestCase(8, 0x2D82D8, TestName = "Hold_Sustain_8")]

    public void GateBitSetHoldsSustainAmplitude(int sustain, int expected)
    {
        var envelopeGenerator = SustainStateEnvelope();

        for (int i = 0; i <= 16000 + 1; i++)
        {
            Step(envelopeGenerator, true, 0, 0, (ushort)sustain, 0);
        }

        Assert.That(envelopeGenerator.Amplitude, Is.EqualTo(expected));
    }

    [TestCase(8, 15, 0x2D82D8, TestName = "IncreaseSustain_Doesnt_Increase_Vol_After_Decay")]
    public void Sustain_Increase(int sustain, int sustainIncreased, int expected)
    {
        var envelopeGenerator = SustainStateEnvelope();

        for (int i = 0; i <= 16000 + 1; i++)
        {
            Step(envelopeGenerator, true, 0, 0, (ushort)sustain, 0);
        }
        for (int i = 0; i <= 1000 + 1; i++)
        {
            Step(envelopeGenerator, true, 0, 0, (ushort)sustainIncreased, 0);
        }


        Assert.That(envelopeGenerator.Amplitude, Is.EqualTo(expected));
    }

    [TestCase(0, 0x5471C7, TestName = "Release_0")]
    [TestCase(15, 0x555547, TestName = "Release_15")]
    public void SubRelease(int release, int expected)
    {
        var envelopeGenerator = SustainStateEnvelope();

        Step(envelopeGenerator, false, 0, 0, 15, (ushort)release);

        Assert.That(envelopeGenerator.Amplitude, Is.EqualTo(expected));
    }

    [TestCase(0, 6, TestName = "Release_0")]
    [TestCase(15, 25000, TestName = "Release_15")]//TODO: find out exactly at which number it should be zero, as the expected 24000 is not correct because of rounding error. 
    public void AmplitudeIsZeroRightAfterRelease(int release, int decayRateFactor)
    {
        var envelopeGenerator = SustainStateEnvelope();

        for (int i = 0; i <= 16 * decayRateFactor + 1; i++)
        {
            Step(envelopeGenerator, false, 0, 0, 15, (ushort)release);
        }
        Assert.That(envelopeGenerator.Amplitude, Is.EqualTo(0x0000));
    }

    [Test]
    public void AmplitudeStaysZero16KCyclesAfterRelease0()
    {
        var envelopeGenerator = SustainStateEnvelope();

        for (int i = 0; i <= 16 * 6 * +1 + 16000; i++)
        {
            Step(envelopeGenerator, false, 0, 0, 15, 0);
        }
        Assert.That(envelopeGenerator.Amplitude, Is.EqualTo(0x0000));
    }
}