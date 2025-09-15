using UnityEngine;

namespace Game.Audio
{
    public interface IAudioPoolService
    {
        void Initialize();
        AudioSource CastAudio(AudioCastData audioCastData);
        void SetNewMaxAudioSourcesCount(int newMaxAudioSources);
    }
} 