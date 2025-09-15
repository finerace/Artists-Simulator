using System;
using Game.Additional.MagicAttributes;
using UnityEngine;

namespace Game.Services.Core
{
    
    public class PaintPath : MonoBehaviour
    {
        [SerializeField] private Transform[] pathPoints;

        [Space] 
        
        [SerializeField] private float v1;
        [SerializeField] private float v2;
        
        public Vector3 GetPointOnPath(float t)
        {
            var centerPointIndex = (int)t;
            var tStep = t - centerPointIndex;
            
            if (tStep == 0 && centerPointIndex >= 1)
            {
                centerPointIndex--;
                tStep = 1f - Single.MinValue;
            }
            
            centerPointIndex *= 2;
            
            switch (pathPoints.Length)
            {
                case <= 2:
                    return transform.position;
                default:
                {
                    var centerPoint = pathPoints[centerPointIndex].position;
                    var nextPoint = Vector3.zero;
                    var nextNextPoint = Vector3.zero;

                    var resultPos = Vector3.zero;

                    const int power = 2;
                    nextPoint = pathPoints[centerPointIndex + 1].position * power;
                    nextNextPoint = pathPoints[centerPointIndex + 2].position;

                    resultPos =
                        Vector3.Lerp(centerPoint, nextNextPoint, tStep);
                    
                    resultPos = 
                        Vector3.Lerp(
                            resultPos, 
                            (nextPoint + -nextNextPoint + Vector3.Lerp(centerPoint,nextNextPoint,0.5f)
                                * 0.25f * -power * 1.5f) * v1, 
                            tStep * v2);
                    
                    resultPos = 
                        Vector3.Lerp(resultPos, nextNextPoint, tStep);
                    
                    return resultPos;
                }
            }
        }

        public int GetPathLong()
        {
            var result = pathPoints.Length / 2;

            if (pathPoints.Length % 2 == 0)
                result--;
            
            return result;
        }
        
        public Vector3[] GetPath(int lines)
        {
            if(lines <= 0)
                throw new ArgumentException("Lines can't be less and equal than 0");
            
            var result = new Vector3[GetPathLong() * lines + 1];
            
            for (int i = 0; i < GetPathLong(); i++)
            {
                for (int j = 0; j < lines + 1; j++)
                {
                    var t = (j / (float)lines) + i;
                    
                    if(j == 0)
                    {
                        result[i * lines + j] = pathPoints[i * 2].position;
                        
                        continue;
                    }
                    
                    if(j == lines)
                    {
                        result[i * lines + j] = pathPoints[i * 2 + 2].position;
                        
                        continue;
                    }
                    
                    result[i * lines + j] = GetPointOnPath(t);
                }
            }
            
            return result;
        }
        
        public float GetPathLong(int accuracyDots)
        {
            if(accuracyDots < 0)
                throw new ArgumentException("Accuracy dots can't be less than 0");
            
            var path = GetPath(accuracyDots);
            var result = 0f;

            for (int i = 0; i < path.Length - 1; i++)
            {
                result += Vector3.Distance(path[i], path[i + 1]);
            }

            return result;
        }
        
        public float GetPathPartLong(int partIndex, int accuracyDots)
        {
            if(partIndex < 0 || partIndex > GetPathLong())
                throw new ArgumentException("Part index can't be less than 0 and more than path long");
            
            if(accuracyDots < 0)
                throw new ArgumentException("Accuracy dots can't be less than 0");
            
            var path = GetPath(accuracyDots);
            var result = 0f;

            for (int i = partIndex * accuracyDots; i < (partIndex + 1) * accuracyDots; i++)
            {
                result += Vector3.Distance(path[i], path[i + 1]);
            }

            return result;
        }
        
        public Vector3 GetDirection(float t)
        {
            const float step = 0.0001f;

            if (t < 0)
                throw new ArgumentException("T can't be less than 0");
            
            var point = Vector3.zero;
            var nextPoint = Vector3.zero;;
            
            if (t > GetPathLong() - step)
            {
                point = GetPointOnPath(GetPathLong() - step);
                nextPoint = GetPointOnPath(GetPathLong());

                return (nextPoint - point).normalized;
            }
            
            point = GetPointOnPath(t);
            nextPoint = GetPointOnPath(t + step);
            
            return (nextPoint - point).normalized;
        }
        
#if UNITY_EDITOR
        
        [Space]
        
        [SerializeField] private bool drawGizmos;
        [SerializeField] private float pointsSize = 0.1f;
        [SerializeField] private int pathDrawDotsPerPathPoint = 100;
        
        private void OnDrawGizmos()
        {
            if(!drawGizmos)
                return;
            
            Gizmos.color = Color.red;

            DrawPoints();
            void DrawPoints()
            {
                for (int i = 0; i < pathPoints.Length; i++)
                {
                    Gizmos.DrawSphere(pathPoints[i].position, pointsSize);
                }
            }
            
            DrawPath();
            void DrawPath()
            {
                var path = GetPath(pathDrawDotsPerPathPoint);
                
                for (int i = 0; i < path.Length - 1; i++)
                {
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }
            }
        }

#endif
        
    }
}
