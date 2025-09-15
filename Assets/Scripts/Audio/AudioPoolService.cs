using System;
using System.Collections.Generic;
using Game.Additional.MagicAttributes;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace Game.Audio
{
    
    public class AudioPoolService : IAudioPoolService
    {
        private readonly int startMaxAudioSources = 25;
        private readonly float spreadEffect = 0.35f;
        private int realMaxAudioSources;
        private Dictionary<AudioSource, bool> audioPool = new Dictionary<AudioSource, bool>();
        
        public void Initialize()
        {
            SetNewMaxAudioSourcesCount(startMaxAudioSources);
        }
        
        private void FreeUsedAudioSources()
    {
        var resultFreeSources = new List<AudioSource>();
        
        foreach (var audioSource in audioPool.Keys)
        {
            var isAudioSourceNotFree = !audioPool[audioSource];
            
            if(isAudioSourceNotFree)
                if ((!audioSource.isPlaying && !audioSource.mute))
                    resultFreeSources.Add(audioSource);
        }

        foreach (var audioSource in resultFreeSources)
        {
            audioPool[audioSource] = true;
            
            audioSource.transform.parent = null;
            audioSource.volume = 0;
        }
    }
        
        public AudioSource CastAudio(AudioCastData audioCastData)
    {
        FreeUsedAudioSources();

        AudioSource freeSource;
        
        if(!GetFreeSource(audioCastData.Priority,out freeSource))
            return null;
            
        bool GetFreeSource(int castPriority,out AudioSource freeSource)
        {
            foreach (var source in audioPool.Keys)
            {
                var isSourceFree = audioPool[source];

                if (isSourceFree)
                {
                    freeSource = source;
                    audioPool[source] = false;
                    return true;
                }
            }
            
            foreach (var source in audioPool.Keys)
            {
                var isSourceHaveLowPriority = source.priority > castPriority;

                if (isSourceHaveLowPriority)
                {
                    freeSource = source;
                    audioPool[source] = false;
                    return true;
                }
            }

            freeSource = null;
            return false;
        }
        
        InitFreeSource();
        void InitFreeSource()
        {
            freeSource.outputAudioMixerGroup = audioCastData.AudioMixerGroup;
            freeSource.clip = audioCastData.Clips[Random.Range(0,audioCastData.Clips.Length)];

            freeSource.priority = audioCastData.Priority;
            freeSource.volume = Random.Range(audioCastData.VolumeMin,audioCastData.VolumeMax);
            freeSource.loop = audioCastData.IsLoop;
            freeSource.pitch = Random.Range(audioCastData.PitchMin,audioCastData.PitchMax);
            freeSource.panStereo = audioCastData.StereoPan;
            freeSource.spatialBlend = audioCastData.SpatialBlend;

            freeSource.minDistance = audioCastData.MinDistance;
            freeSource.maxDistance = audioCastData.MaxDistance;
            
            var freeSourceT = freeSource.gameObject.transform;
            freeSourceT.position = audioCastData.CastPos;
            
            if (audioCastData.CastParent != null)
                freeSourceT.parent = audioCastData.CastParent;
            else
                freeSourceT.parent = null;
            
            freeSource.minDistance = audioCastData.MinDistance;
            freeSource.maxDistance = audioCastData.MaxDistance;
            
            freeSource.Play();
            
            freeSource.minDistance = audioCastData.MinDistance;
            freeSource.maxDistance = audioCastData.MaxDistance;
            
            freeSource.minDistance = audioCastData.MinDistance;
            freeSource.maxDistance = audioCastData.MaxDistance;
        }
        
        return freeSource;
    }
        
        public void SetNewMaxAudioSourcesCount(int newMaxAudioSources)
    {
        void AddNewAudioSources(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var audioSourceGameObject = new GameObject();

                const string audioObjectName = "AudioSource[AudioPoolService]";
                audioSourceGameObject.name = audioObjectName;
                
                var newAudioSource = audioSourceGameObject.AddComponent<AudioSource>();
                newAudioSource.spread = spreadEffect;

                audioPool.Add(newAudioSource,true);
            }
        }
        void DeleteAudioSources(int count)
        {
            var freeSources = GetFreeAudioSources();
            AudioSource[] GetFreeAudioSources()
            {
                var resultAudioSources = new List<AudioSource>();

                var findedFreeSources = 0;
                foreach (var audioSource in audioPool.Keys)
                {
                    var isAudioSourceFree = audioPool[audioSource];

                    if (isAudioSourceFree)
                    {
                        resultAudioSources.Add(audioSource);
                        findedFreeSources++;
                        
                        if(findedFreeSources == count)
                            break;
                    } 
                }

                return resultAudioSources.ToArray();
            }

            var deletedSources = 0;
            
            foreach (var source in freeSources)
            {
                audioPool.Remove(source);
                UnityEngine.Object.Destroy(source.gameObject);

                deletedSources++;
                
                if(deletedSources > count)
                    break;
            }
            
            var toDeleteActiveSourcesCount = count - freeSources.Length;

            if (toDeleteActiveSourcesCount > 0)
            {
                DeleteActiveSources();
                void DeleteActiveSources()
                {
                    var toDeleteSources = new List<AudioSource>();

                    foreach (var source in audioPool.Keys)
                    {
                        toDeleteSources.Add(source);

                        if (toDeleteSources.Count >= toDeleteActiveSourcesCount)
                            break;
                    }

                    foreach (var source in toDeleteSources)
                    {
                        audioPool.Remove(source);
                    }
                }
            }

        }
        
        if (newMaxAudioSources > realMaxAudioSources)
            AddNewAudioSources(newMaxAudioSources-realMaxAudioSources);
        else if(newMaxAudioSources < realMaxAudioSources)
            DeleteAudioSources(Mathf.Abs(newMaxAudioSources - realMaxAudioSources));

        realMaxAudioSources = newMaxAudioSources;
    }
    }

    [Serializable]
    public class AudioCastData
    {
        [SerializeField] private AudioMixerGroup audioMixerGroup;
        [SerializeField] private AudioClip[] clips;
        [SerializeField] public Vector3 castPos;
        [SerializeField] public Transform castParent;
        [Range(0,128)] [SerializeField] private int priority = 128;
        [Range(0,1)] [SerializeField] private float volumeMin = 1;
        [Range(0,1)] [SerializeField] private float volumeMax = 1;
        [SerializeField] private bool isLoop = false;
        [Range(-3,3)] [SerializeField] private float pitchMin = 1;
        [Range(-3,3)] [SerializeField] private float pitchMax = 1;
        [Range(-1,1)] [SerializeField] private float stereoPan = 0;
        [Range(0,1)][SerializeField] private float spatialBlend;
        [Range(0,500)] [SerializeField] private float minDistance;
        [Range(0,500)] [SerializeField] private float maxDistance;

        public AudioMixerGroup AudioMixerGroup => audioMixerGroup;

        public AudioClip[] Clips => clips;

        public Vector3 CastPos => castPos;

        public Transform CastParent => castParent;

        public int Priority => priority;

        public float VolumeMin => volumeMin;

        public float VolumeMax => volumeMax;

        public bool IsLoop => isLoop;

        public float PitchMin => pitchMin;

        public float PitchMax => pitchMax;

        public float StereoPan => stereoPan;

        public float SpatialBlend => spatialBlend;

        public float MinDistance => minDistance;

        public float MaxDistance => maxDistance;
    }
}