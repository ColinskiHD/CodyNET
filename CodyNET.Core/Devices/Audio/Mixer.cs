namespace CodyNET.Core.Devices.Audio;

public class Mixer
{

    private ushort _volumeControl;
    public ushort MainVolume => (ushort)(_volumeControl & 0x0F);
    public bool Voice3Off => (_volumeControl & 0x80) != 0;
    public short Step(ushort voice1, ushort voice2, ushort voice3, ushort volumeControl)
    {
        _volumeControl = volumeControl;

        int sum = voice1 + voice2;
        if (!Voice3Off)
            sum += voice3;
        sum = (MainVolume * sum) >> 4;

        short sample = (short)(sum - 0x8000);
        return sample;
    }
}