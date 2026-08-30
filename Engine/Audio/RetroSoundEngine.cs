using System;
using System.Threading;
using System.Threading.Tasks;

namespace MonochromeEngine.Engine.Audio;

public class RetroSoundEngine
{
    private const int SampleRate = 44100;

    public enum WaveType { Pulse, Triangle, Noise }

    private struct Channel
    {
        public WaveType Type;
        public double Frequency;
        public float Volume;
        public double Phase;
    }

    private readonly Channel[] _channels = new Channel[5];
    private CancellationTokenSource? _engineCts;
    private readonly Random _noiseRng = new();

    public RetroSoundEngine()
    {
        // Конфігурація під чип 2A03 (Famicom):
        _channels[0].Type = WaveType.Pulse;    // Pulse 1 (Lead)
        _channels[1].Type = WaveType.Pulse;    // Pulse 2 (Harmony)
        _channels[2].Type = WaveType.Triangle; // Triangle (Bass)
        _channels[3].Type = WaveType.Noise;    // Noise (Snare / Hi-Hat)
        _channels[4].Type = WaveType.Triangle; // Емуляція Conga/Percussion
    }

    public void SetChannel(int channelIndex, double freq, float volume = 0.2f)
    {
        if (channelIndex >= 0 && channelIndex < _channels.Length)
        {
            _channels[channelIndex].Frequency = freq;
            _channels[channelIndex].Volume = volume;
        }
    }

    public void StartAudioEngine()
    {
        _engineCts = new CancellationTokenSource();
        var token = _engineCts.Token;

        Task.Run(() =>
        {
            const int bufferSize = 2048;
            byte[] pcmBuffer = new byte[bufferSize * 2];

            while (!token.IsCancellationRequested)
            {
                for (int i = 0; i < bufferSize; i++)
                {
                    float mixedSample = 0f;

                    for (int ch = 0; ch < _channels.Length; ch++)
                    {
                        if (_channels[ch].Frequency <= 0 || _channels[ch].Volume <= 0) 
                            continue;

                        _channels[ch].Phase += _channels[ch].Frequency / SampleRate;
                        if (_channels[ch].Phase >= 1.0) _channels[ch].Phase -= 1.0;

                        float sample = 0f;

                        switch (_channels[ch].Type)
                        {
                            case WaveType.Pulse:
                                // Прямокутна хвиля (Square)
                                sample = _channels[ch].Phase < 0.5 ? 1f : -1f;
                                break;

                            case WaveType.Triangle:
                                // Трикутна хвиля для оксамитового басу
                                sample = (float)(2.0 * Math.Abs(2.0 * (_channels[ch].Phase - Math.Floor(_channels[ch].Phase + 0.5))) - 1.0);
                                break;

                            case WaveType.Noise:
                                // Генератор псевдовипадкового шуму (LFSR / Random)
                                sample = (float)(_noiseRng.NextDouble() * 2.0 - 1.0);
                                break;
                        }

                        mixedSample += sample * _channels[ch].Volume;
                    }

                    mixedSample = Math.Clamp(mixedSample, -1.0f, 1.0f);
                    short pcmValue = (short)(mixedSample * 32767);

                    pcmBuffer[i * 2] = (byte)(pcmValue & 0xFF);
                    pcmBuffer[i * 2 + 1] = (byte)((pcmValue >> 8) & 0xFF);
                }

                WinMMAudio.PlayRawPcm(pcmBuffer);
            }
        }, token);
    }

    public void StopAudioEngine()
    {
        _engineCts?.Cancel();
    }

    public async Task PlayCastlevaniaVampireKillerAsync(CancellationToken token = default)
    {
        // Темп: 1/16 нота = 105 мс
        const int T = 105;

        // Стовпці: [0:Pulse1 (Lead), 1:Pulse2 (Гармонія/Октава), 2:Triangle (Бас), 3:Noise (Ударні), 4:Perc (Акценти)]
        int[,] score =
        {
            // --- ВСТУП (INTRO) ---
            { 440, 220, 110, 1, 0 }, // A4, A3, A2
            { 0, 0, 0, 0, 0 },
            { 440, 220, 110, 1, 0 },
            { 0, 0, 0, 0, 0 },
            { 440, 220, 110, 1, 0 },
            { 0, 0, 0, 0, 0 },
            { 392, 196, 196, 1, 100 }, // G4, G3, G3
            { 440, 220, 220, 1, 0 }, // A4, A3, A3

            // --- ОСНОВНИЙ РИФ (КУПЛЕТ: ЧАСТИНА 1) ---
            // Такт 1
            { 440, 220, 110, 0, 0 }, // A4, A3, A2
            { 0, 0, 110, 1, 0 },
            { 523, 262, 110, 0, 0 }, // C5, C4, A2
            { 0, 0, 110, 1, 0 },
            { 440, 220, 110, 0, 0 }, // A4, A3, A2
            { 0, 0, 110, 1, 0 },
            { 587, 294, 110, 0, 0 }, // D5, D4, A2
            { 0, 0, 110, 1, 0 },

            // Такт 2
            { 523, 262, 110, 0, 0 }, // C5, C4, A2
            { 0, 0, 110, 1, 0 },
            { 494, 247, 110, 0, 0 }, // B4, B3, A2
            { 0, 0, 110, 1, 0 },
            { 440, 220, 110, 0, 0 }, // A4, A3, A2
            { 0, 0, 110, 1, 0 },
            { 392, 196, 196, 1, 100 }, // G4, G3, G3
            { 440, 220, 220, 1, 0 }, // A4, A3, A3

            // Такт 3
            { 440, 220, 110, 0, 0 }, // A4, A3, A2
            { 0, 0, 110, 1, 0 },
            { 523, 262, 110, 0, 0 }, // C5, C4, A2
            { 0, 0, 110, 1, 0 },
            { 440, 220, 110, 0, 0 }, // A4, A3, A2
            { 0, 0, 110, 1, 0 },
            { 659, 330, 110, 0, 0 }, // E5, E4, A2
            { 0, 0, 110, 1, 0 },

            // Такт 4 (Спуск до D)
            { 587, 294, 147, 0, 0 }, // D5, D4, D3
            { 0, 0, 147, 1, 0 },
            { 523, 262, 147, 0, 0 }, // C5, C4, D3
            { 0, 0, 147, 1, 0 },
            { 494, 247, 147, 0, 0 }, // B4, B3, D3
            { 0, 0, 147, 1, 0 },
            { 523, 262, 147, 1, 100 }, // C5, C4, D3
            { 587, 294, 147, 1, 0 }, // D5, D4, D3

            // --- КУПЛЕТ: ЧАСТИНА 2 (РОЗВИТОК ТА ПІДЙОМ) ---
            // Такт 5 (F-мажорна лінія)
            { 698, 349, 175, 0, 0 }, // F5, F4, F3
            { 0, 0, 175, 1, 0 },
            { 659, 330, 175, 0, 0 }, // E5, E4, F3
            { 0, 0, 175, 1, 0 },
            { 587, 294, 175, 0, 0 }, // D5, D4, F3
            { 0, 0, 175, 1, 0 },
            { 523, 262, 175, 0, 0 }, // C5, C4, F3
            { 0, 0, 175, 1, 0 },

            // Такт 6 (G-мажорна лінія)
            { 587, 294, 196, 0, 0 }, // D5, D4, G3
            { 0, 0, 196, 1, 0 },
            { 523, 262, 196, 0, 0 }, // C5, C4, G3
            { 0, 0, 196, 1, 0 },
            { 494, 247, 196, 0, 0 }, // B4, B3, G3
            { 0, 0, 196, 1, 0 },
            { 392, 196, 196, 1, 100 }, // G4, G3, G3
            { 494, 247, 196, 1, 0 }, // B4, B3, G3

            // Такт 7-8 (Фінальне соло мотиву: E -> F -> E -> D# -> E)
            { 659, 330, 165, 0, 0 }, // E5, E4, E3
            { 0, 0, 165, 1, 0 },
            { 698, 349, 165, 0, 0 }, // F5, F4, E3
            { 659, 330, 165, 1, 0 }, // E5, E4, E3
            { 622, 311, 165, 0, 0 }, // D#5, D#4, E3
            { 659, 330, 165, 1, 0 }, // E5, E4, E3
            { 784, 392, 165, 0, 100 }, // G5, G4, E3
            { 0, 0, 165, 1, 0 },

            { 880, 440, 220, 1, 120 }, // A5, A4, A3
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 }
        };

        for (int row = 0; row < score.GetLength(0); row++)
        {
            if (token.IsCancellationRequested) break;

            SetChannel(0, score[row, 0], 0.22f); // Lead
            SetChannel(1, score[row, 1], 0.14f); // Harmony
            SetChannel(2, score[row, 2], 0.35f); // Bass (Triangle)
            SetChannel(3, score[row, 3] > 0 ? 1 : 0, score[row, 3] > 0 ? 0.09f : 0f); // Noise
            SetChannel(4, score[row, 4], 0.16f); // Conga/Accent

            await Task.Delay(T, token);
        }

        for (int ch = 0; ch < 5; ch++) SetChannel(ch, 0, 0f);
    }

    // Головна тема Tiny Toon Adventures (NES / Konami)
public async Task PlayTinyToonAsync(CancellationToken token = default)
{
    // Темп: 1/16 нота = 110 мс
    const int T = 110;

    // [0:Pulse1 (Мелодія), 1:Pulse2 (Гармонія), 2:Triangle (Бас), 3:Noise (Снер/Хет), 4:Perc (Акцентний тон)]
    int[,] score = {
        // --- ВСТУП (INTRO ROLL) ---
        {   0,   0, 131, 1, 0 },
        {   0,   0, 147, 1, 0 },
        {   0,   0, 165, 1, 0 },
        {   0,   0, 175, 1, 0 },
        { 392, 196, 196, 1, 120 }, // G4, G3, G3
        { 392, 196, 196, 0, 0 },
        { 392, 196, 196, 1, 0 },
        {   0,   0,   0, 0, 0 },

        // --- КУПЛЕТ 1: "We're tiny, we're toony..." ---
        { 523, 262, 131, 0, 80 },  // C5, C4, C3
        {   0,   0,   0, 1, 0 },
        { 587, 294, 196, 0, 0 },   // D5, D4, G3
        { 659, 330,   0, 1, 80 },  // E5, E4
        { 698, 349, 131, 0, 0 },   // F5, F4, C3
        {   0,   0,   0, 1, 0 },
        { 784, 392, 196, 0, 80 },  // G5, G4, G3
        {   0,   0,   0, 1, 0 },

        // "...we're all a little loony..."
        { 880, 440, 175, 0, 80 },  // A5, A4, F3
        {   0,   0,   0, 1, 0 },
        { 784, 392, 196, 0, 0 },   // G5, G4, G3
        { 659, 330,   0, 1, 80 },  // E5, E4
        { 523, 262, 131, 0, 0 },   // C5, C4, C3
        {   0,   0,   0, 1, 0 },
        { 587, 294, 196, 1, 80 },  // D5, D4, G3
        {   0,   0,   0, 0, 0 },

        // --- КУПЛЕТ 2: "...and in this cartoony..." ---
        { 523, 262, 131, 0, 80 },  // C5, C4, C3
        {   0,   0,   0, 1, 0 },
        { 587, 294, 196, 0, 0 },   // D5, D4, G3
        { 659, 330,   0, 1, 80 },  // E5, E4
        { 698, 349, 131, 0, 0 },   // F5, F4, C3
        {   0,   0,   0, 1, 0 },
        { 784, 392, 196, 0, 80 },  // G5, G4, G3
        {   0,   0,   0, 1, 0 },

        // "...we're invading your TV!"
        { 880, 440, 175, 0, 80 },  // A5, A4, F3
        {   0,   0,   0, 1, 0 },
        { 784, 392, 196, 0, 0 },   // G5, G4, G3
        { 659, 330,   0, 1, 80 },  // E5, E4
        { 587, 294, 196, 0, 0 },   // D5, D4, G3
        { 523, 262, 131, 1, 80 },  // C5, C4, C3
        {   0,   0,   0, 0, 0 },
        {   0,   0,   0, 0, 0 },

        // --- БРИДЖ (Хроматичні переходи) ---
        { 440, 220, 175, 0, 80 },  // A4, A3, F3
        { 494, 247, 196, 1, 0 },   // B4, B3, G3
        { 523, 262, 220, 0, 80 },  // C5, C4, A3
        { 587, 294, 147, 1, 0 },   // D5, D4, D3
        { 659, 330, 165, 0, 80 },  // E5, E4, E3
        { 698, 349, 175, 1, 0 },   // F5, F4, F3
        { 784, 392, 196, 0, 100 }, // G5, G4, G3
        {   0,   0,   0, 1, 0 },

        // Фінальний стрибок і каданс
        { 880, 440, 175, 0, 80 },  // A5, A4, F3
        {   0,   0,   0, 1, 0 },
        { 784, 392, 196, 0, 0 },   // G5, G4, G3
        { 659, 330, 165, 1, 80 },  // E5, E4, E3
        { 587, 294, 196, 0, 0 },   // D5, D4, G3
        {   0,   0,   0, 1, 0 },
        { 523, 262, 131, 0, 120 }, // C5, C4, C3
        {   0,   0,   0, 0, 0 },
        {   0,   0,   0, 0, 0 },
        {   0,   0,   0, 0, 0 }
    };

    for (int row = 0; row < score.GetLength(0); row++)
    {
        if (token.IsCancellationRequested) break;

        SetChannel(0, score[row, 0], 0.22f); // Pulse 1 (Lead)
        SetChannel(1, score[row, 1], 0.14f); // Pulse 2 (Harmony)
        SetChannel(2, score[row, 2], 0.32f); // Triangle (Bass)
        SetChannel(3, score[row, 3] > 0 ? 1 : 0, score[row, 3] > 0 ? 0.08f : 0f); // Noise
        SetChannel(4, score[row, 4], 0.15f); // Percussion/Bongo

        await Task.Delay(T, token);
    }

    // Заглушити всі канали після завершення
    for (int ch = 0; ch < 5; ch++) SetChannel(ch, 0, 0f);
}

    // 5-канальна партитура
    public async Task PlayMario5ChannelAsync(CancellationToken token = default)
    {
        const int T = 125;

        // Колонки: [0:Pulse1 (Мелодія), 1:Pulse2 (Гармонія), 2:Triangle (Бас), 3:Noise (Ударні), 4:Perc (Бонго)]
        int[,] score = {
            // Intro
            { 659, 330, 147, 1, 0 },
            { 659, 330, 147, 0, 0 },
            {   0,   0,   0, 0, 0 },
            { 659, 330, 147, 1, 0 },
            {   0,   0,   0, 0, 0 },
            { 523, 262, 147, 0, 0 },
            { 659, 330, 147, 1, 0 },
            {   0,   0,   0, 0, 0 },
            { 784, 392, 196, 1, 100 },
            {   0,   0,   0, 0, 0 },
            {   0,   0,   0, 0, 0 },
            {   0,   0,   0, 0, 0 },
            { 392, 196, 196, 1, 80 },
            {   0,   0,   0, 0, 0 },
            {   0,   0,   0, 0, 0 },
            {   0,   0,   0, 0, 0 },

            // Куплет
            { 523, 262, 196, 0, 100 },
            {   0,   0,   0, 1, 0 },
            {   0,   0, 131, 0, 0 },
            { 392, 196,   0, 1, 80 },
            {   0,   0, 196, 0, 0 },
            {   0,   0,   0, 1, 0 },
            { 330, 165, 165, 0, 100 },
            {   0,   0,   0, 1, 0 },
            {   0,   0, 220, 0, 0 },
            { 440, 220,   0, 1, 80 },
            { 494, 247, 247, 0, 0 },
            { 466, 233, 233, 1, 0 },
            { 440, 220, 220, 0, 100 },
            {   0,   0,   0, 1, 0 },
            { 392, 196, 196, 0, 80 },
            { 659, 330, 165, 1, 0 },
            { 784, 392, 196, 0, 100 },
            { 880, 440, 220, 1, 0 },
            {   0,   0,   0, 0, 0 },
            { 698, 349, 175, 1, 80 },
            { 784, 392, 196, 0, 0 },
            {   0,   0,   0, 1, 0 },
            { 659, 330, 165, 0, 100 },
            {   0,   0,   0, 1, 0 },
            { 523, 262, 131, 0, 80 },
            { 587, 294, 147, 1, 0 },
            { 494, 247, 196, 0, 0 },
            {   0,   0,   0, 1, 0 },
            {   0,   0,   0, 0, 0 },
            {   0,   0,   0, 0, 0 }
        };

        for (int row = 0; row < score.GetLength(0); row++)
        {
            if (token.IsCancellationRequested) break;

            SetChannel(0, score[row, 0], 0.20f); // Lead
            SetChannel(1, score[row, 1], 0.15f); // Harmony
            SetChannel(2, score[row, 2], 0.30f); // Bass
            SetChannel(3, score[row, 3] > 0 ? 1 : 0, score[row, 3] > 0 ? 0.08f : 0f); // Noise
            SetChannel(4, score[row, 4], 0.15f); // Conga

            await Task.Delay(T, token);
        }

        for (int ch = 0; ch < 5; ch++) SetChannel(ch, 0, 0f);
    }
}