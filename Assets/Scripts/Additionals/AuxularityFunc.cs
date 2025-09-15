using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Infrastructure.Configs;
using Game.Additional.MagicAttributes;
using UnityEngine;

namespace Game.Additional
{
    
    public static class AuxiliaryFunc
    {
        private static AnimationsConfig AnimationsConfig => ConfigsProxy.AnimationsConfig;
        
        // === VECTOR3_EXTENSIONS ===
        
        public static float LengthXZ(this Vector3 b)
        {
            float returnFloat = Mathf.Abs(b.x) + Mathf.Abs(b.z);
            return returnFloat;
        }

        public static float LengthY(this Vector3 b)
        {
            float returnFloat = Mathf.Abs(b.y);
            return returnFloat;
        }

        // === LAYER_MASK_OPERATIONS ===
        
        public static bool IsLayerInMask(this LayerMask mask, int layer)
        {
            int maskValue = mask.value;
            int layerValue = 1 << layer;

            if (maskValue < layerValue)
                return false;

            if (maskValue == layerValue)
                return maskValue == layerValue;

            int dynamicMaskValue = maskValue;

            for (int i = 30; i >= 0; i--)
            {
                int localMaskNum = 1 << i;
                
                if (localMaskNum > maskValue)
                    continue;

                if (dynamicMaskValue == layerValue)
                    return true;

                if (((layerValue * 2) - 1) >= dynamicMaskValue && layerValue <= dynamicMaskValue)
                    return true;

                if ((dynamicMaskValue - localMaskNum) < 0)
                    continue;


                dynamicMaskValue -= localMaskNum;

                if (((layerValue * 2) - 1) >= dynamicMaskValue && layerValue <= dynamicMaskValue)
                    return true;
            }

            return false;
        }

        // === RAYCAST_OPERATIONS ===
        
        public static float PointDirection_TargetLocalPosDOT(Vector3 targetLocalPos, Vector3 pointDirection)
        {
            return Vector3.Dot(pointDirection.normalized, targetLocalPos.normalized);
        }

        // === MATH_CONVERSION_OPERATIONS ===
        
        public static float ClampToTwoRemainingCharacters(this float target)
        {
            return (int)(target * 100f) / 100f;
        }
        
        public static float ClampToRemainingCharacters(this float target,int characters)
        {
            return (int)(target * Mathf.Pow(10,characters)) / Mathf.Pow(10,characters);
        }

        public static int ConvertFloatToPercent(this float target)
        {



            return (int)(target * 100);
        }
        
        public static string ConvertNumCharacters(this int charactersCount)
        {
            var resultString = charactersCount.ToString();

            if (resultString.Length == 1)
            {
                resultString = "0" + resultString;
            }

            return resultString;
        }
        
        public static TimeSpan ConvertSecondsToTimeSpan(int seconds)
        {
            return new TimeSpan(0, 0, seconds);
        }

        public static string ConvertSecondsToTimer(this float seconds)
        {
            var timeSpan = new TimeSpan(0, 0, (int)seconds);

            var milliseconds =
                ConvertNumCharacters((int)((seconds.ClampToTwoRemainingCharacters() - (int)seconds) * 100));

            return $"{ConvertNumCharacters(timeSpan.Minutes)}:" +
                   $"{ConvertNumCharacters(timeSpan.Seconds)}:" +
                   $"{milliseconds}";
        }

        // === TRANSFORM_GAMEOBJECT_OPERATIONS ===
        
        public static void SetChildsActive(this Transform objectT, bool active)
        {
            for (int i = 0; i < objectT.childCount; i++)
            {
                objectT.GetChild(i).gameObject.SetActive(active);
            }
        }

        public static void DeleteChilds(this Transform objectT)
        {
            var count = objectT.childCount;

            for (int i = 0; i < count; i++)
            {
                UnityEngine.Object.DestroyImmediate(objectT.GetChild(0).gameObject);
            }
        }

        public static Vector3 ClampMagnitude(this Vector3 vector, float max)
        {
            if (max == 0)
                return vector;

            var magnitudeExcess = max / vector.magnitude;

            if (magnitudeExcess < 1)
                return vector * magnitudeExcess;

            return vector;
        }

        public static RaycastHit Raycast(this Transform origin, Vector3 rayDirection, float distance = Mathf.Infinity,
            int layerMask = -1)
        {
            RaycastHit raycastHitOut;
            
            if (layerMask >= 0)
            {
                if (Physics.Raycast(origin.position, rayDirection, out raycastHitOut, distance, layerMask))
                    return raycastHitOut;
            }
            else if (Physics.Raycast(origin.position, rayDirection, out raycastHitOut, distance))
                return raycastHitOut;
            
            return new RaycastHit();
        }
        
        public static Vector2 ToVectorXZ(this Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.z);
        }

        public static string ConvertToString(this float target)
        {
            return $"{(int)target}." +
                   $"{((int)((target.ClampToTwoRemainingCharacters() - (int)target) * 100)).ConvertNumCharacters()}";
        }

        // === PARTICLE_SYSTEM_EXTENSIONS ===
        
        public static void PlayP(this ParticleSystem particle)
        {
            if (!particle.isEmitting)
                particle.Play();
        }

        public static string ToShortenInt(this int count)
        {
            var countStr = count.ToString();

            switch (countStr.Length)
            {
                default:
                    return countStr;

                case 4:
                    return $"{countStr[0]}.{countStr[1]}{countStr[2]}k";
                case 5:
                    return $"{countStr[0]}{countStr[1]}.{countStr[2]}k";
                case 6:
                    return $"{countStr[0]}{countStr[1]}{countStr[2]}k";
                case 7:
                    return $"{countStr[0]}.{countStr[1]}{countStr[2]}m";
                case 8:
                    return $"{countStr[0]}{countStr[1]}.{countStr[2]}m";
                case 9:
                    return $"{countStr[0]}{countStr[1]}{countStr[2]}m";
            }
        }

        public static float CalculateSpeedToEndTime(this float target, float maxValue)
        {
            return maxValue / target;
        }
        
        public static int ConvertSecondsToMilliseconds(this float target)
        {
            return (int)(target * 1000);
        }
        
        // === COLOR_OPERATIONS ===
        
        public static Color SpecialLerp(this Color color1, Color color2, float t)
        {
            var r1 = color1.r;
            var g1 = color1.g;
            var b1 = color1.b;
            var r2 = color2.r;
            var g2 = color2.g;
            var b2 = color2.b;
            
            var r3 = Mathf.Lerp(r1,r2,t);
            var g3 = Mathf.Lerp(g1,g2,t);
            var b3 = Mathf.Lerp(b1,b2,t);

            var newColor = new Color(0,0,0);
            
            newColor.r = r3;
            newColor.g = g3;
            newColor.b = b3;
            
            Debug.Log($"{r1} {g1} {b1} - {r2} {g2} {b2} - {newColor}");

            return newColor;
        }
        
        public static double DoubleLerp(this double n1, double n2, double t)
        {
            return n1 + (n2 - n1) * t;
        }
        
        // === UI_ANIMATIONS ===
        
        [SafeAnimation]
        public static UniTask ChangeCanvasAlpha(this CanvasGroup canvasGroup, float targetAlpha)
        {
            canvasGroup.interactable = targetAlpha > 0;
            
            return canvasGroup.DOFade(targetAlpha, AnimationsConfig.CommonAnimationTime)
                .SetEase(AnimationsConfig.CommonAnimationEase).AsyncWaitForCompletion().AsUniTask();
        }
        
        [SafeAnimation]
        public static async UniTask ChangeCanvasAlphaAndEnableDisabler(this CanvasGroup canvasGroup, float targetAlpha)
        {
            if(targetAlpha > 0)
                canvasGroup.gameObject.SetActive(true);
            
            await canvasGroup.DOFade(targetAlpha, AnimationsConfig.CommonAnimationTime)
                .SetEase(AnimationsConfig.CommonAnimationEase).AsyncWaitForCompletion().AsUniTask();
            
            if(targetAlpha <= 0 && canvasGroup && canvasGroup.gameObject)
                canvasGroup.gameObject.SetActive(false);
        }

        [SafeAnimation]
        public static async UniTask ShowScaleAnimation(this Transform target, float scaleMultiplier, bool alphaEnable = false)
        {
            var defaultScale = target.localScale;
            target.localScale *= scaleMultiplier;
            
            UniTask alphaTask = UniTask.CompletedTask;
            
            if(alphaEnable)
            {
                var canvasGroup = target.GetComponent<CanvasGroup>();
                canvasGroup.blocksRaycasts = true;
                
                alphaTask = canvasGroup.ChangeCanvasAlpha(1);
            }
            
            var scaleTask = target.DOScale(defaultScale,
                    AnimationsConfig.CommonAnimationTime).SetEase(AnimationsConfig.CommonAnimationEase)
                .AsyncWaitForCompletion().AsUniTask();
            
            await UniTask.WhenAll(alphaTask, scaleTask);
        }
        
        [SafeAnimation]
        public static async UniTask HideScaleAnimation(this Transform target, float scaleMultiplier, bool alphaEnable = false)
        {
            var defaultScale = target.localScale;
            var localTarget = target;
            var tweener = target.DOScale(target.localScale * scaleMultiplier,
                    AnimationsConfig.CommonAnimationTime).SetEase(AnimationsConfig.CommonAnimationEase);
                
            UniTask alphaTask = UniTask.CompletedTask;
            
            if(alphaEnable)
            {
                var canvasGroup = target.GetComponent<CanvasGroup>();
                canvasGroup.blocksRaycasts = false;
                
                alphaTask = canvasGroup.ChangeCanvasAlpha(0);
            }

            var scaleTask = tweener.AsyncWaitForCompletion().AsUniTask();

            await UniTask.WhenAll(alphaTask, scaleTask);

            if (localTarget && localTarget.gameObject)
                localTarget.localScale = defaultScale;
        }
        
        [SafeAnimation]
        public static async UniTask ShowScaleAnimation(this Transform target, float scaleMultiplier, 
            Vector3 defaultScale, bool alphaEnable = false)
        {
            target.localScale *= scaleMultiplier;
            
            UniTask alphaTask = UniTask.CompletedTask;
            
            if(alphaEnable)
            {
                var canvasGroup = target.GetComponent<CanvasGroup>();
                canvasGroup.blocksRaycasts = true;
                
                alphaTask = canvasGroup.ChangeCanvasAlpha(1);
            }
            
            var scaleTask = target.DOScale(defaultScale,
                    AnimationsConfig.CommonAnimationTime).SetEase(AnimationsConfig.CommonAnimationEase)
                .AsyncWaitForCompletion().AsUniTask();
            
            await UniTask.WhenAll(alphaTask, scaleTask);
        }
        
        [SafeAnimation]
        public static async UniTask HideScaleAnimation(this Transform target, float scaleMultiplier,
            Vector3 defaultScale, bool alphaEnable = false)
        {
            var localTarget = target;
            var tweener = target.DOScale(target.localScale * scaleMultiplier,
                    AnimationsConfig.CommonAnimationTime).SetEase(AnimationsConfig.CommonAnimationEase);
                
            UniTask alphaTask = UniTask.CompletedTask;
            
            if(alphaEnable)
            {
                var canvasGroup = target.GetComponent<CanvasGroup>();
                canvasGroup.blocksRaycasts = false;
                
                alphaTask = canvasGroup.ChangeCanvasAlpha(0);
            }

            var scaleTask = tweener.AsyncWaitForCompletion().AsUniTask();

            await UniTask.WhenAll(alphaTask, scaleTask);

            if (localTarget && localTarget.gameObject)
                localTarget.localScale = defaultScale;
        }
        
        [SafeAnimation]
        public static async UniTask AnchorMove(this RectTransform target, RectTransform toPos)
        {
            await target.DOAnchorPos(toPos.anchoredPosition,
                    ConfigsProxy.AnimationsConfig.CommonAnimationTime)
                .SetEase(ConfigsProxy.AnimationsConfig.CommonAnimationEase)
                .AsyncWaitForCompletion().AsUniTask();
        }
        
        [SafeAnimation]
        public static async UniTask AnchorMove(this RectTransform target, RectTransform toPos, Ease ease)
        {
            await target.DOAnchorPos(toPos.anchoredPosition,
                    ConfigsProxy.AnimationsConfig.CommonAnimationTime)
                .SetEase(ease)
                .AsyncWaitForCompletion().AsUniTask();
        }
        
        [SafeAnimation]
        public static async UniTask AnchorMove(this RectTransform target, RectTransform toPos, float animationTime, Ease ease)
        {
            await target.DOAnchorPos(toPos.anchoredPosition,
                    animationTime)
                .SetEase(ease)
                .AsyncWaitForCompletion().AsUniTask();
        }
        
        [SafeAnimation]
        public static async UniTask SetPanelShow(this RectTransform panel, bool state, RectTransform showPoint,
            RectTransform hidePoint, float animationTime, float scaleDifference, Ease ease)
        {
            RectTransform targetPoint;

            targetPoint = state ? showPoint : hidePoint;

            if(state)
            {
                panel.gameObject.SetActive(true);
                
                panel.ShowScaleAnimation(scaleDifference, Vector3.one, false).Forget();
            }

            await panel.AnchorMove(targetPoint, animationTime, ease);
            
            if(!state && panel && panel.gameObject)
                panel.gameObject.SetActive(false);
        }
        
        [SafeAnimation]
        public static void SetButtonShowAlpha(this RectTransform button, bool state)
        {
            var animConfig = ConfigsProxy.AnimationsConfig;
            
            if(state)
            {
                button.gameObject.SetActive(true);
                
                button.ShowScaleAnimation(animConfig.CommonAnimationScaleDifference, true)
                    .Forget();
            }
            else
            {
                button.HideScaleAnimation(animConfig.CommonAnimationScaleDifference, true)
                    .GetAwaiter()
                    .OnCompleted(() => 
                    {
                        if(button != null && button.gameObject != null)
                            button.gameObject.SetActive(false);
                    });
            }
        }
        
        [SafeAnimation]
        public static void SetUIObjectShowAlpha(this RectTransform objectT, bool state, Vector3 defaultScale)
        {
            var animConfig = ConfigsProxy.AnimationsConfig;
            
            if(state)
            {
                objectT.gameObject.SetActive(true);
                
                objectT.ShowScaleAnimation(animConfig.CommonAnimationScaleDifference,defaultScale, true)
                    .Forget();
            }
            else
            {
                objectT.HideScaleAnimation(animConfig.CommonAnimationScaleDifference,defaultScale, true)
                    .GetAwaiter().OnCompleted(() =>
                    {
                        objectT.gameObject.SetActive(false);
                    });
            }
        }
        
        [SafeAnimation]
        public static UniTask SetUIObjectShowAlphaTask(this RectTransform objectT, bool state, Vector3 defaultScale)
        {
            var animConfig = ConfigsProxy.AnimationsConfig;
            
            if(state)
            {
                objectT.gameObject.SetActive(true);
                
                return objectT.ShowScaleAnimation(animConfig.CommonAnimationScaleDifference, defaultScale, true);
            }
            else
            {
                var task = objectT.HideScaleAnimation(animConfig.CommonAnimationScaleDifference, defaultScale, true);
                
                task.GetAwaiter().OnCompleted(() =>
                {
                    if(objectT && objectT.gameObject)
                        objectT.gameObject.SetActive(false);
                });
                
                return task;
            }
        }
        
        public static Transform GetLastChildT(this Transform target)
        {
            return target.GetChild(target.childCount - 1);
        }

        // === LAYER_UTILITIES ===
        
        public static void SetLayerSelfAndChilds(this GameObject target, int layer)
        {
            target.layer = layer;

            for (int i = 0; i < target.transform.childCount; i++)
            {
                var child = target.transform.GetChild(i).gameObject;

                child.layer = layer;
                
                SetLayerSelfAndChilds(child,layer);
            }
            
        }
        
        // === VECTOR3_UTILITIES ===
        
        public static Vector3 GetChildsCenterPos(this Transform target)
        {
            Vector3 totalPosition = Vector3.zero;
            int count = 0;

            foreach (Transform child in target)
            {
                totalPosition += child.position;
                count++;
            }

            if (count > 0)
            {
                totalPosition /= count;
            }
            
            return totalPosition;
        }
        
        public static int RoundToInt(this float target)
        {
            return Mathf.RoundToInt(target);
        }
        
        // === TEXTURE_OPERATIONS ===
        
        public static Vector2 GetTextureCenterPos(this Texture2D texture)
        {
            return new Vector2(texture.width / 2, texture.height / 2);
        }
        
        public static Vector2 GetTextureSize(this Texture2D texture)
        {
            return new Vector2(texture.width, texture.height);
        }
        
    }
}