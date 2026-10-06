namespace CodyNET.Core.Interfaces;

public interface IAudioOutput
{
    public void RenderSample(short samples);
}