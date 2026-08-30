using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MonochromeEngine.Engine.Audio;

public static class WinMMAudio
{
    private const int WAVE_FORMAT_PCM = 1;
    private const int CALLBACK_NULL = 0;

    [StructLayout(LayoutKind.Sequential)]
    private struct WAVEFORMATEX
    {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WAVEHDR
    {
        public IntPtr lpData;
        public uint dwBufferLength;
        public uint dwBytesRecorded;
        public IntPtr dwUser;
        public uint dwFlags;
        public uint dwLoops;
        public IntPtr lpNext;
        public IntPtr reserved;
    }

    private const uint WHDR_DONE = 0x00000001;

    [DllImport("winmm.dll")]
    private static extern int waveOutOpen(out IntPtr hWaveOut, uint uDeviceID, ref WAVEFORMATEX lpFormat, IntPtr dwCallback, IntPtr dwInstance, uint fdwOpen);

    [DllImport("winmm.dll")]
    private static extern int waveOutPrepareHeader(IntPtr hWaveOut, ref WAVEHDR lpWaveOutHdr, uint uSize);

    [DllImport("winmm.dll")]
    private static extern int waveOutWrite(IntPtr hWaveOut, ref WAVEHDR lpWaveOutHdr, uint uSize);

    [DllImport("winmm.dll")]
    private static extern int waveOutUnprepareHeader(IntPtr hWaveOut, ref WAVEHDR lpWaveOutHdr, uint uSize);

    [DllImport("winmm.dll")]
    private static extern int waveOutClose(IntPtr hWaveOut);

    /// <summary>
    /// Відтворює 16-бітний PCM буфер (моно, 44100 Гц за замовчуванням).
    /// Блокує потік до повного завершення програвання буфера.
    /// </summary>
    public static void PlayRawPcm(byte[] pcmData, int sampleRate = 44100, short channels = 1, short bitsPerSample = 16)
    {
        if (pcmData == null || pcmData.Length == 0) return;

        var format = new WAVEFORMATEX
        {
            wFormatTag = WAVE_FORMAT_PCM,
            nChannels = (ushort)channels,
            nSamplesPerSec = (uint)sampleRate,
            wBitsPerSample = (ushort)bitsPerSample,
            nBlockAlign = (ushort)(channels * (bitsPerSample / 8)),
            nAvgBytesPerSec = (uint)(sampleRate * channels * (bitsPerSample / 8)),
            cbSize = 0
        };

        if (waveOutOpen(out IntPtr hWaveOut, 0xFFFFFFFF /* WAVE_MAPPER */, ref format, IntPtr.Zero, IntPtr.Zero, CALLBACK_NULL) != 0)
        {
            return;
        }

        // Фіксуємо масив байтів у пам'яті
        GCHandle handle = GCHandle.Alloc(pcmData, GCHandleType.Pinned);
        try
        {
            var header = new WAVEHDR
            {
                lpData = handle.AddrOfPinnedObject(),
                dwBufferLength = (uint)pcmData.Length,
                dwFlags = 0
            };

            int headerSize = Marshal.SizeOf(typeof(WAVEHDR));

            waveOutPrepareHeader(hWaveOut, ref header, (uint)headerSize);
            waveOutWrite(hWaveOut, ref header, (uint)headerSize);

            // Очікуємо, поки драйвер закінчить програвання буфера
            while ((header.dwFlags & WHDR_DONE) == 0)
            {
                Thread.Sleep(5);
            }

            waveOutUnprepareHeader(hWaveOut, ref header, (uint)headerSize);
        }
        finally
        {
            handle.Free();
            waveOutClose(hWaveOut);
        }
    }
}