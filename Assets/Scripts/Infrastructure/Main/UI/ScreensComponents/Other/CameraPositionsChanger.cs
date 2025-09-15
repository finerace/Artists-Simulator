using System;
using Cysharp.Threading.Tasks;
using Game.Services.Meta;
using Game.Services.Common;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Infrastructure.Main.UI
{
    public class CameraPositionsChanger : MonoBehaviour
    {
        private CamerasService camerasService;

        [SerializeField] private Button rightButton;
        [SerializeField] private Button leftButton;

        [Space]
        
        [SerializeField] private Transform[] cameraPoints;
        [SerializeField] private int startPoint;
        
        public event Action<int> OnCameraPointChanged; 

        private int currentPoint;
        public int CurrentPoint => currentPoint;

        public int StartPoint => startPoint;

        [Inject]
        private void Construct(CamerasService camerasService)
        {
            this.camerasService = camerasService;
        }
        
        private void Start()
        {
            rightButton.onClick.AddListener(() => MoveCameraPoint(1));
            leftButton.onClick.AddListener(() => MoveCameraPoint(-1));

            currentPoint = startPoint;
        }
        
        private void MoveCameraPoint(int direction)
        {
            currentPoint += direction;
            
            if (currentPoint < 0)
            {
                currentPoint = cameraPoints.Length - 1;
            }
            else if (currentPoint >= cameraPoints.Length)
            {
                currentPoint = 0;
            }
            
            camerasService.MoveMainCameraToPoint(cameraPoints[currentPoint]).Forget();
            
            OnCameraPointChanged?.Invoke(currentPoint);
        }
        
        public void SetCameraPoints(Transform[] cameraPoints)
        {
            this.cameraPoints = cameraPoints;
        }

    }
}