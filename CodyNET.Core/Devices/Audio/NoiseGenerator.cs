namespace CodyNET.Core.Devices.Audio;

public class NoiseGenerator
{
    public ushort Value = 0xACE1;// Variable for the noise generator (Galois linear feedback shift register);
    public void Step() => Value = Next(Value);
    private const ushort NOISE_BITS = 0xB400; // Value to AND with the current LFSR value (for noise generation)

    internal static ushort Next(ushort value)
    {
        ushort temp = (ushort)(value & 0x0001);
        temp = (ushort)-temp;
        temp = (ushort)(temp & NOISE_BITS);
        value = (ushort)(value >> 1);
        value = (ushort)(value ^ temp);
        return value;
    }
}