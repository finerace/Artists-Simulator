using Game.Additional;
using Game.Infrastructure.Main.Locations;
using UnityEngine;
using Zenject;

namespace Game.Infrastructure.Main.UI.LocationImproveMenu
{
    public class LocationImproveMenuView : UniversalMenuView
    {
        [SerializeField] private CameraPositionsChanger cameraPositionsChanger;
        [SerializeField] private RectTransform rightArrow;
        [SerializeField] private RectTransform leftArrow;

        [Space] 
        
        [SerializeField] private RectTransform[] locImprovesGroups;

        private MainLocationView mainLocationView;
        
        [Inject]
        private void Construct(MainLocationProxy mainLocationProxy)
        {
            mainLocationView = mainLocationProxy.MainLocation;
        }
        
        private void Start()
        {
            cameraPositionsChanger.SetCameraPoints(mainLocationView.LocImprovementsCameraPoints);
            
            ActivateLocImproveGroup(cameraPositionsChanger.StartPoint);
            CheckArrowsVisibility(cameraPositionsChanger.StartPoint);
            
            cameraPositionsChanger.OnCameraPointChanged += ActivateLocImproveGroup;
            cameraPositionsChanger.OnCameraPointChanged += CheckArrowsVisibility;
        }

        private void ActivateLocImproveGroup(int index)
        {
            for (int i = 0; i < locImprovesGroups.Length; i++)
            {
                locImprovesGroups[i].SetUIObjectShowAlpha(i == index,Vector3.one);
            }
        }

        private void CheckArrowsVisibility(int index)
        {
            rightArrow.SetUIObjectShowAlpha(index != locImprovesGroups.Length-1,Vector3.one);
            leftArrow.SetUIObjectShowAlpha(index != 0,Vector3.one);
        }
        
    }
}