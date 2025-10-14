using UnityEngine;
using System;

namespace NowHere.RPG
{
    /// <summary>
    /// 상태 효과를 나타내는 클래스
    /// 버프, 디버프, 지속 효과 등을 관리
    /// </summary>
    [System.Serializable]
    public class StatusEffect
    {
        [Header("Effect Settings")]
        public string effectName;
        public EffectType effectType;
        public float value;
        public float duration;
        public bool isPermanent;
        public float tickInterval = 1f;
        
        [Header("Visual Effects")]
        public GameObject visualEffect;
        public Color effectColor = Color.white;
        
        // 내부 변수
        private float startTime;
        private float lastTickTime;
        private bool isActive = false;
        
        // 이벤트
        public System.Action<StatusEffect> OnEffectStarted;
        public System.Action<StatusEffect> OnEffectTick;
        public System.Action<StatusEffect> OnEffectEnded;
        
        public StatusEffect()
        {
            effectName = "Unknown Effect";
            effectType = EffectType.Buff;
            value = 0f;
            duration = 10f;
            isPermanent = false;
            tickInterval = 1f;
        }
        
        public StatusEffect(string name, EffectType type, float val, float dur)
        {
            effectName = name;
            effectType = type;
            value = val;
            duration = dur;
            isPermanent = dur <= 0f;
            tickInterval = 1f;
        }
        
        public void StartEffect()
        {
            startTime = Time.time;
            lastTickTime = startTime;
            isActive = true;
            
            OnEffectStarted?.Invoke(this);
            Debug.Log($"상태 효과 시작: {effectName}");
        }
        
        public void UpdateEffect()
        {
            if (!isActive) return;
            
            // 지속 시간 체크
            if (!isPermanent && Time.time - startTime >= duration)
            {
                EndEffect();
                return;
            }
            
            // 틱 간격 체크
            if (Time.time - lastTickTime >= tickInterval)
            {
                TickEffect();
                lastTickTime = Time.time;
            }
        }
        
        private void TickEffect()
        {
            OnEffectTick?.Invoke(this);
            Debug.Log($"상태 효과 틱: {effectName}");
        }
        
        public void EndEffect()
        {
            isActive = false;
            OnEffectEnded?.Invoke(this);
            Debug.Log($"상태 효과 종료: {effectName}");
        }
        
        public void ApplyEffect(CharacterSystem character = null)
        {
            // 효과 적용 로직
            Debug.Log($"상태 효과 적용: {effectName}");
        }
        
        public void RemoveEffect(CharacterSystem character = null)
        {
            // 효과 제거 로직
            Debug.Log($"상태 효과 제거: {effectName}");
        }
        
        public bool IsActive()
        {
            return isActive;
        }
        
        public float GetRemainingTime()
        {
            if (isPermanent) return -1f;
            return Mathf.Max(0f, duration - (Time.time - startTime));
        }
        
        public float GetProgress()
        {
            if (isPermanent) return 1f;
            return Mathf.Clamp01((Time.time - startTime) / duration);
        }
        
        public StatusEffect Clone()
        {
            StatusEffect clone = new StatusEffect();
            clone.effectName = this.effectName;
            clone.effectType = this.effectType;
            clone.value = this.value;
            clone.duration = this.duration;
            clone.isPermanent = this.isPermanent;
            clone.tickInterval = this.tickInterval;
            clone.visualEffect = this.visualEffect;
            clone.effectColor = this.effectColor;
            return clone;
        }
    }
    
    public enum EffectType
    {
        Buff,        // 버프 (긍정적 효과)
        Debuff,      // 디버프 (부정적 효과)
        Heal,        // 회복
        Damage,      // 데미지
        Shield,      // 보호막
        Stun,        // 기절
        Slow,        // 둔화
        Haste,       // 가속
        Poison,      // 독
        Burn,        // 화상
        Freeze,      // 빙결
        Shock,       // 감전
        Regeneration // 재생
    }
}
