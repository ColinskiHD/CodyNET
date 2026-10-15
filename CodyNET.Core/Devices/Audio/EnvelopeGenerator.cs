using System.Collections;
using System.Data;

namespace CodyNET.Core.Devices.Audio;

public class EnvelopeGenerator
{
    private const int MAX_LEVEL = 0xFFFFFF / 3;
    private static readonly int[] AttackMs = { 2, 4, 16, 24, 38, 58, 68, 80, 100, 250, 500, 800, 1000, 3000, 5000, 8000 };
    private static readonly int[] DecayMs = { 6, 24, 48, 72, 114, 168, 204, 240, 300, 750, 1500, 2400, 3000, 9000, 15000, 24000 };

    private static readonly int[] AttackRates = AttackMs.Select(n => (0xFFFFFF / 3) / (16 * n)).ToArray();

    private static readonly int[] DecayRates = DecayMs.Select(n => (0xFFFFFF / 3) / (16 * n)).ToArray();
    public int Amplitude { get; private set; }
    public ushort Envelope => (ushort)(Amplitude >> 8);
    private ADSRState _adsrState = ADSRState.Release;
    public void Step(ushort attack, ushort decay, ushort sustain, ushort release, bool gate)
    {
        if (gate & _adsrState == ADSRState.Release)
        {
            _adsrState = ADSRState.Attack;
        }
        if (gate)
        {
            switch (_adsrState)
            {
                case ADSRState.Attack:
                    Amplitude = Attack(Amplitude, attack);
                    break;
                case ADSRState.Decay:
                    Amplitude = Decay(Amplitude, decay, sustain);
                    break;
            }
        }
        else
        {
            Amplitude = Release(Amplitude, release);
            _adsrState = ADSRState.Release;
        }
        return;
    }
    private int Attack(int amplitude, int attack)
    {
        if (amplitude + AttackRates[attack] >= MAX_LEVEL)
        {
            _adsrState = ADSRState.Decay;
            return MAX_LEVEL;
        }
        return amplitude + AttackRates[attack];
    }
    private int Decay(int amplitude, int decay, int sustain)
    {
        if (amplitude - DecayRates[decay] <= MAX_LEVEL * sustain / 15)
        {
            return MAX_LEVEL * sustain / 15;
        }
        return amplitude - DecayRates[decay];
    }
    private int Release(int amplitude, int release)
    {
        if (amplitude - DecayRates[release] <= 0x0000)
        {
            return 0x0000;
        }
        return amplitude - DecayRates[release];
    }

    private enum ADSRState
    {
        Attack,
        Decay,
        Release,
    }
}