using UnityEngine;
using UnityEngine.EventSystems;
using Game.Audio;
using Zenject;

namespace Game.UI
{
    public class AudioUIHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
    {
        [Header("Audio Events")]
        [SerializeField] private AudioCastData mouseDownAudio;
        [SerializeField] private AudioCastData mouseUpAudio;
        [SerializeField] private AudioCastData mouseEnterAudio;
        
        private IAudioPoolService audioPoolService;
        
        [Inject]
        public void Construct(IAudioPoolService audioPoolService)
        {
            this.audioPoolService = audioPoolService;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (audioPoolService != null && mouseDownAudio.Clips != null && mouseDownAudio.Clips.Length > 0)
            {
                audioPoolService.CastAudio(mouseDownAudio);
            }
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (audioPoolService != null && mouseUpAudio.Clips != null && mouseUpAudio.Clips.Length > 0)
            {
                audioPoolService.CastAudio(mouseUpAudio);
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (audioPoolService != null && mouseEnterAudio.Clips != null && mouseEnterAudio.Clips.Length > 0)
            {
                audioPoolService.CastAudio(mouseEnterAudio);
            }
        }
    }
} 