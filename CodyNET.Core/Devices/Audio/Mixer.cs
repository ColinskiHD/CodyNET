using CodyNET.Core.Devices.Audio;
namespace CodyNET.Core.Devices.Audio;

public class Mixer
{
    private readonly NoiseGenerator _noiseGenerator = new();
    private ushort _volumeControl;
    public bool Voice3Off => (_volumeControl & 0x80) != 0;
    public short Step(Voice[] voices, ushort volumeControl)
    {
        _volumeControl = volumeControl;

        _noiseGenerator.Step();
        foreach (var voice in voices)
            voice.Step(_noiseGenerator.Value);

        int sum = (ushort)(voices[0].Output + voices[1].Output);
        if (Voice3Off)
            sum += voices[2].Output;

        short sample = (short)(sum - 0x8000);//cast to short
        return sample;
    }
}