using System;
using System.ComponentModel.DataAnnotations;
using CodyNET.Core.Devices.Audio;
using CodyNET.Core.Interfaces;
using Serilog.Formatting.Display;

namespace CodyNET.Core.Devices.Audio;

public class DCBlockingAudioOutput : IAudioOutput
{
    private readonly IAudioOutput? _audioOutput;
    private double _previousInput = short.MinValue;
    private double _previousOutput;
    private const double P = 0.995;
    private const double MIN = short.MinValue;
    private const double MAX = short.MaxValue;
    public DCBlockingAudioOutput(IAudioOutput? audioOutput = null)
    {
        _audioOutput = audioOutput;
    }
    public void RenderSample(short inputSample)
    {   //y[n] = p · y[n–1] + x[n]–x[n–1]
        double output = P * _previousOutput + inputSample - _previousInput;

        _audioOutput?.RenderSample((short)Math.Clamp(Math.Round(output), MIN, MAX));
        _previousInput = inputSample;
        _previousOutput = output;
    }
}