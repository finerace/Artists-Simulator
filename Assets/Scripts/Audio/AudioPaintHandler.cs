using UnityEngine;
using Game.Audio;
using Game.Services.Core;
using Zenject;

namespace Game.UI
{
    public class AudioPaintHandler : MonoBehaviour
    {
        [Header("Paint Accuracy Audio")]
        [SerializeField] private AudioCastData scoreUpdateAudio;
        [SerializeField] private AudioCastData newPathAudio;
        
        [Header("Paint Gameplay Audio")]
        [SerializeField] private AudioCastData matchStartAudio;
        [SerializeField] private AudioCastData matchEndAudio;
        [SerializeField] private AudioCastData pathCompleteAudio;
        [SerializeField] private AudioCastData totalScoreChangedAudio;
        
        private IAudioPoolService audioPoolService;
        private PaintAccuracyService paintAccuracyService;
        private PaintGameplayGenerationService paintGameplayGenerationService;
        
        [Inject]
        public void Construct(
            IAudioPoolService audioPoolService,
            PaintAccuracyService paintAccuracyService,
            PaintGameplayGenerationService paintGameplayGenerationService)
        {
            this.audioPoolService = audioPoolService;
            this.paintAccuracyService = paintAccuracyService;
            this.paintGameplayGenerationService = paintGameplayGenerationService;
        }
        
        private void Start()
        {
            SubscribeToPaintAccuracyEvents();
            SubscribeToPaintGameplayEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromPaintAccuracyEvents();
            UnsubscribeFromPaintGameplayEvents();
        }
        
        private void SubscribeToPaintAccuracyEvents()
        {
            if (paintAccuracyService != null)
            {
                paintAccuracyService.OnScoreUpdate += OnScoreUpdate;
                paintAccuracyService.OnNewPath += OnNewPath;
            }
        }
        
        private void UnsubscribeFromPaintAccuracyEvents()
        {
            if (paintAccuracyService != null)
            {
                paintAccuracyService.OnScoreUpdate -= OnScoreUpdate;
                paintAccuracyService.OnNewPath -= OnNewPath;
            }
        }
        
        private void SubscribeToPaintGameplayEvents()
        {
            if (paintGameplayGenerationService != null)
            {
                paintGameplayGenerationService.OnCompetitiveGameFinished += OnMatchFinished;
                paintGameplayGenerationService.OnTotalScoreChanged += OnTotalScoreChanged;
            }
        }
        
        private void UnsubscribeFromPaintGameplayEvents()
        {
            if (paintGameplayGenerationService != null)
            {
                paintGameplayGenerationService.OnCompetitiveGameFinished -= OnMatchFinished;
                paintGameplayGenerationService.OnTotalScoreChanged -= OnTotalScoreChanged;
            }
        }
        
        private void OnScoreUpdate(float score)
        {
            if (audioPoolService != null && scoreUpdateAudio.Clips != null && scoreUpdateAudio.Clips.Length > 0)
            {
                audioPoolService.CastAudio(scoreUpdateAudio);
            }
        }
        
        private void OnNewPath(PaintPath path)
        {
            if (audioPoolService != null && newPathAudio.Clips != null && newPathAudio.Clips.Length > 0)
            {
                audioPoolService.CastAudio(newPathAudio);
            }
        }
        
        private void OnMatchFinished(MatchResult result)
        {
            if (audioPoolService != null && matchEndAudio.Clips != null && matchEndAudio.Clips.Length > 0)
            {
                audioPoolService.CastAudio(matchEndAudio);
            }
        }
        
        private void OnTotalScoreChanged(float totalScore)
        {
            if (audioPoolService != null && totalScoreChangedAudio.Clips != null && totalScoreChangedAudio.Clips.Length > 0)
            {
                audioPoolService.CastAudio(totalScoreChangedAudio);
            }
        }
        
        public void PlayMatchStartAudio()
        {
            if (audioPoolService != null && matchStartAudio.Clips != null && matchStartAudio.Clips.Length > 0)
            {
                audioPoolService.CastAudio(matchStartAudio);
            }
        }
        
        public void PlayPathCompleteAudio()
        {
            if (audioPoolService != null && pathCompleteAudio.Clips != null && pathCompleteAudio.Clips.Length > 0)
            {
                audioPoolService.CastAudio(pathCompleteAudio);
            }
        }
    }
} 