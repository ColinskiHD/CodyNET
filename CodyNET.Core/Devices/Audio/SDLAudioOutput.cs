#region License
/* Copyright (c) 2024-2026 Eduard Gushchin.
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 */

/*this class contains modified code from Eduard Gushin*/
#endregion
using SDL3;
namespace CodyNET.Core.Devices.Audio;

using System;
using CodyNET.Core.Interfaces;

public class SDLAudioOutput : IAudioOutput
{
    private const int SampleRate = 16000;
    private IntPtr _stream;
    private readonly short[] _buffer = new short[256];
    private readonly byte[] _bytes = new byte[256 * sizeof(short)];
    private int _count;
    private const int MaxBytesQueued = SampleRate * sizeof(short) / 4;

    public SDLAudioOutput()
    {
        if (!SDL.Init(SDL.InitFlags.Audio))
        {
            throw new InvalidOperationException($"SDL audio init failed: {SDL.GetError()}");
        }
        Configure();
    }

    private void Configure()
    {
        var spec = new SDL.AudioSpec
        {
            Channels = 1,
            Format = SDL.AudioFormat.AudioS16LE,
            Freq = SampleRate
        };
        _stream = SDL.OpenAudioDeviceStream(SDL.AudioDeviceDefaultPlayback, in spec, null, IntPtr.Zero);
        if (_stream == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Couldn't create audio stream: {SDL.GetError()}");
        }
        SDL.ResumeAudioStreamDevice(_stream);
    }

    public void RenderSample(short sample)
    {
        _buffer[_count++] = sample;
        if (_count < _buffer.Length)
            return;
        _count = 0;
        if (SDL.GetAudioStreamQueued(_stream) > MaxBytesQueued)
        {
            return;
        }
        Buffer.BlockCopy(_buffer, 0, _bytes, 0, _bytes.Length);//convert short array to byte array
        SDL.PutAudioStreamData(_stream, _bytes, _bytes.Length);
    }

    private void Cleanup()//TODO: figure out if and from where this should be called
    {
        if (_stream == IntPtr.Zero)
        {
            return;
        }
        SDL.DestroyAudioStream(_stream);
        _stream = IntPtr.Zero;
    }
}