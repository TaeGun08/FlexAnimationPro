using UnityEngine;
using System.Collections;
using FlexAnimation.Internal;

namespace FlexAnimation
{
    [System.Serializable]
    public class LookAtModule : AnimationModule
    {
        [Header("Target")]
        public bool useTargetTransform = true;
        public Transform targetTransform;
        public Vector3 worldPosition;
        public Vector3 targetOffset;

        [Header("Expert Settings")]
        [Range(0f, 1f)] public float smoothing = 0.5f; // 가속/감속(Ease)의 부드러움 강도
        
        [Header("Orientation & Constraints")]
        public Vector3 forwardAxis = Vector3.forward;
        public Vector3 upAxis = Vector3.up;
        public bool lockX, lockY, lockZ;

        public override IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            if (target == null) yield break;

            Quaternion initialRotation = target.rotation;
            float elapsed = 0f;

            while (true)
            {
                if (target == null) yield break;

                // --- 1. 시간 계산 (첫 프레임은 0에서 시작하여 Snap 방지) ---
                float t = 0f;
                bool isFinished = false;

                if (loop == LoopMode.Loop || loop == LoopMode.Incremental)
                {
                    // [Loop 추적 모드] - 시간 제한 없이 계속 따라감
                    // 여기서는 기존의 부드러운 지연(Smoothing) 방식을 유지
                    float dt = (ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) * globalTimeScale;
                    elapsed += dt;
                    
                    Vector3 currentTargetPos = GetTargetPosition() + targetOffset;
                    Vector3 lookDir = currentTargetPos - target.position;
                    if (lookDir.sqrMagnitude > 0.0001f)
                    {
                        Quaternion targetLook = CalculateTargetRotation(lookDir, target.rotation);
                        float lerpSpeed = Mathf.Lerp(30f, 2f, smoothing);
                        target.rotation = Quaternion.Slerp(target.rotation, targetLook, 1f - Mathf.Exp(-lerpSpeed * dt));
                    }
                    
                    if (duration > 0 && loopCount != -1 && elapsed >= duration * loopCount) break;
                    yield return null;
                    continue;
                }
                else if (loop == LoopMode.Yoyo)
                {
                    float totalProgress = elapsed / duration;
                    int cycleIndex = Mathf.FloorToInt(totalProgress);
                    float cycleLocal = totalProgress - cycleIndex;
                    
                    bool isForward = (cycleIndex % 2 == 0);
                    float linearT = isForward ? cycleLocal : (1f - cycleLocal);
                    t = ApplySmoothing(linearT, smoothing);

                    if (loopCount != -1 && elapsed >= duration * loopCount) 
                    {
                        isFinished = true;
                        t = (loopCount % 2 != 0) ? 1f : 0f; 
                    }
                }
                else // Single Mode (None)
                {
                    float linearT = duration > 0 ? Mathf.Clamp01(elapsed / duration) : 1f;
                    t = ApplySmoothing(linearT, smoothing);
                    if (elapsed >= duration) 
                    {
                        isFinished = true;
                        t = 1f; 
                    }
                }

                // --- 2. 목표 회전값 계산 (매 프레임 갱신) ---
                Vector3 targetPos = GetTargetPosition() + targetOffset;
                Vector3 dir = targetPos - target.position;
                
                if (dir.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetLook = CalculateTargetRotation(dir, target.rotation);
                    // Slerp를 통해 시작점에서 목표점까지 t 비율로 정확히 이동
                    // 타겟이 움직여도 targetLook이 매 프레임 바뀌므로 자연스럽게 추적함
                    target.rotation = Quaternion.Slerp(initialRotation, targetLook, t);
                }

                if (isFinished) break;

                yield return null;
                
                float frameDt = (ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) * globalTimeScale;
                elapsed += frameDt;
            }
        }

        // 스무딩 슬라이더 값에 따라 보간 곡선의 부드러움을 결정
        private float ApplySmoothing(float t, float smoothAmount)
        {
            if (smoothAmount <= 0.01f) return t; // Linear

            // InOutCubic 곡선 계산
            float smoothT = t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
            
            // 사용자가 설정한 smoothing 값에 따라 Linear와 Smooth 곡선을 블렌딩
            return Mathf.Lerp(t, smoothT, smoothAmount);
        }

        private Vector3 GetTargetPosition()
        {
            return (useTargetTransform && targetTransform != null) ? targetTransform.position : worldPosition;
        }

        private Quaternion CalculateTargetRotation(Vector3 lookDir, Quaternion currentRot)
        {
            Quaternion lookRot = Quaternion.LookRotation(lookDir, upAxis);
            
            if (forwardAxis == Vector3.up) lookRot *= Quaternion.Euler(90, 0, 0);
            else if (forwardAxis == Vector3.right) lookRot *= Quaternion.Euler(0, 90, 0);
            else if (forwardAxis == Vector3.left) lookRot *= Quaternion.Euler(0, -90, 0);
            else if (forwardAxis == -Vector3.forward) lookRot *= Quaternion.Euler(0, 180, 0);

            if (lockX || lockY || lockZ)
            {
                Vector3 targetEuler = lookRot.eulerAngles;
                Vector3 currentEuler = currentRot.eulerAngles;
                if (lockX) targetEuler.x = currentEuler.x;
                if (lockY) targetEuler.y = currentEuler.y;
                if (lockZ) targetEuler.z = currentEuler.z;
                lookRot = Quaternion.Euler(targetEuler);
            }
            return lookRot;
        }
    }
}
