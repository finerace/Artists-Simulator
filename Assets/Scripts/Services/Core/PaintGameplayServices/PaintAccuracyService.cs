using System;
using System.Collections.Generic;
using Game.Infrastructure.Configs;
using UnityEngine;
using Zenject;
using Game.Additional.MagicAttributes;
using Game.Services.Common.Logging;

namespace Game.Services.Core
{
    
    public class PaintAccuracyService : IFixedTickable
    {
        private readonly PaintingService paintingService;

        private PaintPath targetPath;
        private PaintPathProgress paintPathProgress;
        private List<Vector3> cachedPathPoints;

        private Vector3 previousPaintDot;
        private float currentScore;
        private float maxProgressAchieved;
        private bool isActive;
        private bool isLightMode;
        private bool shouldStopCalculations;
        
        private Vector3[] pathDirectionsCache;
        private float[] pathSegmentLengths;
        private Quaternion cachedSurfaceNormal;
        
        private int lastFoundPointIndex = 0;
        private float pathTraversingSpeed = 0f;
        private Vector3 lastProcessedPoint = Vector3.zero;
        private float timeSinceLastPointUpdate = 0f;

        private PaintProgressDragger currentDragger;
        
        public event Action<float> OnScoreUpdate;
        public event Action<PaintPath> OnNewPath;
        
        public float CurrentScore => currentScore;

        [Inject]
        private PaintAccuracyService(PaintingService paintingService)
        {
            this.paintingService = paintingService;
        }

        public float GetCurrentScoreAndReset()
        {
            float score = currentScore;
            currentScore = 0f;
            Logs.Debug($"Score reset. Final score: {score:F2}");
            return score;
        }

        public void SetNewPath(PaintPath path, PaintPathProgress paintPathProgress, bool isLightMode = false)
        {
            InitializePathData(path, paintPathProgress, isLightMode);
            void InitializePathData(PaintPath path, PaintPathProgress progress, bool lightMode)
            {
                targetPath = path;
                this.paintPathProgress = progress;
                this.isLightMode = lightMode;
            }

            CachePathPoints();
            void CachePathPoints()
            {
                int pathAccuracy = ConfigsProxy.PaintGameplaySystemsConfig.PathAccuracy;
                cachedPathPoints = new List<Vector3>(targetPath.GetPath(pathAccuracy));
                PrecomputePathData();
            }

            PrepareAccuracyCalculation();
            void PrepareAccuracyCalculation()
            {
                ResetCaches();
                previousPaintDot = Vector3.zero;
                maxProgressAchieved = 0f;
                currentDragger = paintPathProgress.ActiveDragger;
                cachedSurfaceNormal = Quaternion.LookRotation(paintingService.GetPaintingSurface().GetNormal());
            }

            NotifyPathChanged();
            void NotifyPathChanged()
            {
                OnNewPath?.Invoke(targetPath);
                Logs.Debug($"New path set. Points: {cachedPathPoints.Count}, Light mode: {isLightMode}");
            }
        }

        private void ResetCaches()
        {
            lastFoundPointIndex = 0;
            pathTraversingSpeed = 0f;
            lastProcessedPoint = Vector3.zero;
            timeSinceLastPointUpdate = 0f;
            cachedSurfaceNormal = Quaternion.identity;
        }

        private void PrecomputePathData()
        {
            InitializeCaches();
            void InitializeCaches()
            {
                int count = cachedPathPoints.Count;
                pathDirectionsCache = new Vector3[count - 1];
                pathSegmentLengths = new float[count - 1];
            }

            ComputeSegmentData();
            void ComputeSegmentData()
            {
                for (int i = 0; i < cachedPathPoints.Count - 1; i++)
                {
                    ProcessSegment(i);
                    void ProcessSegment(int index)
                    {
                        Vector3 direction = cachedPathPoints[index + 1] - cachedPathPoints[index];
                        float length = direction.magnitude;
                        
                        pathSegmentLengths[index] = length;
                        pathDirectionsCache[index] = CalculateNormalizedDirection(direction, length, index);
                        
                        Vector3 CalculateNormalizedDirection(Vector3 dir, float len, int idx)
                        {
                            if (len > 0.0001f)
                                return dir / len;
                            
                            return idx > 0 ? pathDirectionsCache[idx - 1] : Vector3.forward;
                        }
                    }
                }
            }
        }

        public void FixedTick()
        {
            if (!ShouldProcessAccuracy()) return;
            bool ShouldProcessAccuracy() => 
                !shouldStopCalculations && 
                isActive && 
                paintingService.IsPaintingNow && 
                (paintPathProgress.IsDragging || isLightMode);

            ProcessCurrentAccuracyMode();
            void ProcessCurrentAccuracyMode()
            {
                if (isLightMode)
                    ProcessLightMode();
                else
                    ProcessHardModeIfPointMoved();

                void ProcessHardModeIfPointMoved()
                {
                    var lastPoint = paintingService.LastPaintPoint;
                    var config = ConfigsProxy.PaintGameplaySystemsConfig;
                    
                    if (Vector3.Distance(previousPaintDot, lastPoint) >= config.PaintPointsRegDistance)
                        ProcessHardMode(lastPoint);
                }
            }

            NotifyScoreUpdate();
            void NotifyScoreUpdate() => OnScoreUpdate?.Invoke(currentScore);
        }
        
        private void ProcessLightMode()
        {
            float scoreDelta = CalculateLightModeScore();
            float CalculateLightModeScore()
            {
                float dotProduct = CalculateDirectionAlignment();
                float CalculateDirectionAlignment()
                {
                    Vector3 playerDirection = (cachedSurfaceNormal * currentDragger.MouseDelta).normalized;
                    Vector3 pathDirection = targetPath.GetDirection(paintPathProgress.CurrentProgress).normalized;
                    return Vector3.Dot(playerDirection, pathDirection);
                }

                return EvaluateScore(dotProduct);
                float EvaluateScore(float alignment)
                {
                    var config = ConfigsProxy.PaintGameplaySystemsConfig;
                    
                    if (alignment > config.MinDotProductThreshold)
                    {
                        UpdateMaxProgress();
                        void UpdateMaxProgress()
                        {
                            var progress = paintPathProgress.CurrentProgress;
                            if (progress > maxProgressAchieved)
                                maxProgressAchieved = progress;
                        }

                        return config.MaxScorePerPoint * alignment * config.ClosePathMultiplier * config.LightModeScoreMultiplier;
                    }
                    
                    return config.FarPenaltyMultiplier * alignment;
                }
            }

            ApplyScore(scoreDelta);
            void ApplyScore(float delta)
            {
                var config = ConfigsProxy.PaintGameplaySystemsConfig;
                currentScore = Mathf.Max(config.MinScoreThreshold, currentScore + delta);
            }
        }

        private void ProcessHardMode(Vector3 lastPoint)
        {
            float scoreDelta = CalculateHardModeScore(lastPoint);
            float CalculateHardModeScore(Vector3 point)
            {
                float minDistance = GetMinDistance(point, cachedPathPoints);
                float progress = GetProgressAlongPath(point, cachedPathPoints);

                return EvaluateProgressAndDistance(progress, minDistance);
                float EvaluateProgressAndDistance(float prog, float dist)
                {
                    var config = ConfigsProxy.PaintGameplaySystemsConfig;

                    if (prog > maxProgressAchieved)
                    {
                        UpdateMaxProgress(prog);
                        void UpdateMaxProgress(float newProgress) => maxProgressAchieved = newProgress;

                        return CalculatePositiveScore(dist);
                        float CalculatePositiveScore(float distance)
                        {
                            float closenessFactor = 1f - Mathf.Clamp01(distance / config.MaxAccuracyDistance);
                            return config.MaxScorePerPoint * closenessFactor * config.ClosePathMultiplier;
                        }
                    }
                    
                    if (dist > config.MaxAccuracyDistance)
                        return CalculatePenalty(dist);
                    
                    return 0f;

                    float CalculatePenalty(float distance) => 
                        -config.FarPenaltyMultiplier * (distance - config.MaxAccuracyDistance);
                }
            }

            UpdateStateAndScore(lastPoint, scoreDelta);
            void UpdateStateAndScore(Vector3 point, float delta)
            {
                previousPaintDot = point;
                currentScore = Mathf.Max(0f, currentScore + delta);
            }
        }

        private float GetMinDistance(Vector3 playerPaintDot, List<Vector3> pathPoints)
        {
            float minDistance = float.MaxValue;
            foreach (var point in pathPoints)
            {
                float d = Vector3.Distance(playerPaintDot, point);
                if (d < minDistance)
                    minDistance = d;
            }
            return minDistance;
        }

        private float GetProgressAlongPath(Vector3 playerPaintDot, List<Vector3> pathPoints)
        {
            UpdateTraversingSpeed(playerPaintDot);
            void UpdateTraversingSpeed(Vector3 currentPoint)
            {
                if (lastProcessedPoint != Vector3.zero)
                {
                    float movementDistance = Vector3.Distance(lastProcessedPoint, currentPoint);
                    pathTraversingSpeed = Mathf.Lerp(pathTraversingSpeed, movementDistance / Time.deltaTime, 0.3f);
                    timeSinceLastPointUpdate += Time.deltaTime;
                }
                lastProcessedPoint = currentPoint;
            }

            int bestIndex = FindClosestPointIndex(playerPaintDot, pathPoints);
            int FindClosestPointIndex(Vector3 playerPoint, List<Vector3> points)
            {
                int searchRange = CalculateSearchRange();
                int CalculateSearchRange()
                {
                    var config = ConfigsProxy.PaintGameplaySystemsConfig;
                    int range = Mathf.RoundToInt(config.BaseSearchRange + pathTraversingSpeed * timeSinceLastPointUpdate * config.SpeedSearchMultiplier);
                    return Mathf.Clamp(range, config.BaseSearchRange, points.Count / 2);
                }

                int bestIdx = SearchInRange(playerPoint, points, searchRange);
                int SearchInRange(Vector3 point, List<Vector3> pathPts, int range)
                {
                    int startIdx = Mathf.Max(0, lastFoundPointIndex - range);
                    int endIdx = Mathf.Min(pathPts.Count - 1, lastFoundPointIndex + range);
                    
                    float minDist = float.MaxValue;
                    int bestPointIdx = lastFoundPointIndex;

                    for (int i = startIdx; i <= endIdx; i++)
                    {
                        float d = Vector3.Distance(point, pathPts[i]);
                        if (d < minDist)
                        {
                            minDist = d;
                            bestPointIdx = i;
                        }
                    }

                    if (bestPointIdx == startIdx || bestPointIdx == endIdx)
                        return SearchFullRange(point, pathPts, startIdx, endIdx, minDist, bestPointIdx);
                    
                    return bestPointIdx;
                }

                int SearchFullRange(Vector3 point, List<Vector3> pathPts, int skipStart, int skipEnd, float currentMinDist, int currentBest)
                {
                    float minDist = currentMinDist;
                    int bestIdx = currentBest;

                    for (int i = 0; i < pathPts.Count; i++)
                    {
                        if (i >= skipStart && i <= skipEnd)
                            continue;

                        float d = Vector3.Distance(point, pathPts[i]);
                        if (d < minDist)
                        {
                            minDist = d;
                            bestIdx = i;
                        }
                    }
                    return bestIdx;
                }

                return bestIdx;
            }

            UpdateFoundIndex(bestIndex);
            void UpdateFoundIndex(int index)
            {
                lastFoundPointIndex = index;
                timeSinceLastPointUpdate = 0f;
            }

            return CalculateProgressFromIndex(bestIndex, pathPoints);
            float CalculateProgressFromIndex(int index, List<Vector3> points)
            {
                float progress = CalculateProgressToIndex(index, points);
                float CalculateProgressToIndex(int idx, List<Vector3> pts)
                {
                    float prog = 0f;
                    for (int i = 1; i <= idx; i++)
                        prog += Vector3.Distance(pts[i - 1], pts[i]);
                    return prog;
                }

                return progress + CalculateExtraProgressBetweenPoints(index, points);
                float CalculateExtraProgressBetweenPoints(int idx, List<Vector3> pts)
                {
                    if (idx >= pts.Count - 1) return 0f;

                    Vector3 currentPoint = pts[idx];
                    Vector3 nextPoint = pts[idx + 1];
                    Vector3 dirToNext = (nextPoint - currentPoint).normalized;
                    Vector3 playerOffset = playerPaintDot - currentPoint;

                    float extraProgress = Vector3.Dot(playerOffset, dirToNext);
                    if (extraProgress <= 0) return 0f;

                    float segmentLength = Vector3.Distance(currentPoint, nextPoint);
                    return Mathf.Min(extraProgress, segmentLength);
                }
            }
        }

        public float CalculateMaxScore()
        {
            var config = ConfigsProxy.PaintGameplaySystemsConfig;
            float maxScore = 0f;
            for (int i = 0; i < cachedPathPoints.Count - 1; i++)
            {
                float segmentLength = Vector3.Distance(cachedPathPoints[i], cachedPathPoints[i + 1]);
                maxScore += segmentLength * config.GetMaxScorePerUnit(isLightMode);
            }
            return maxScore;
        }

        public void SetActive(bool active)
        {
            if (StateChanged(active))
                HandleStateChange(active);
            bool StateChanged(bool newState) => isActive != newState;

            void HandleStateChange(bool newState)
            {
                if (newState)
                    ResetForNewSession();
                void ResetForNewSession()
                {
                    ResetCaches();
                    currentScore = 0f;
                }

                LogStateChange(newState);
                void LogStateChange(bool state) => 
                    Logs.Debug($"PaintAccuracyService active state changed: {isActive} → {state}");
            }

            ApplyNewState(active);
            void ApplyNewState(bool newState)
            {
                isActive = newState;
                shouldStopCalculations = false;

                if (!newState)
                    isLightMode = false;
            }
        }

        public void SetCalculationsStopActive(bool isActive)
        {
            shouldStopCalculations = isActive;
            if (isActive)
                Logs.Debug("Accuracy calculations stopped");
        }
    }
}