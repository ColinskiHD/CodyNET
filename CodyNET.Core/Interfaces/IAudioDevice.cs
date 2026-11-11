using CodyNET.Core.Cody;

namespace CodyNET.Core.Interfaces;

public interface IAudioDevice : IMemoryMappedDevice
{
    void SetClockFrequency(long cyclesPerSecond);
}