using UnityEngine;
using System.Collections.Generic;
using System;

namespace NowHere.RPG
{
    /// <summary>
    /// 스킬 시스템을 관리하는 클래스
    /// 캐릭터의 스킬 학습, 사용, 효과를 담당
    /// </summary>
    [System.Serializable]
    public class SkillSystem : MonoBehaviour
    {
        [Header("Skill Database")]
        [SerializeField] private List<Skill> skillDatabase = new List<Skill>();
        [SerializeField] private List<Skill> learnedSkills = new List<Skill>();
        [SerializeField] private List<Skill> activeSkills = new List<Skill>();
        
        [Header("Skill Settings")]
        [SerializeField] private int maxActiveSkills = 4;
        [SerializeField] private float globalCooldown = 0.5f;
        
        [Header("Skill Effects")]
        [SerializeField] private List<SkillEffect> skillEffects = new List<SkillEffect>();
        
        // 스킬 상태
        private float lastSkillUseTime;
        private Dictionary<int, float> skillCooldowns = new Dictionary<int, float>();
        
        // 이벤트
        public event Action<Skill> OnSkillLearned;
        public event Action<Skill> OnSkillUsed;
        public event Action<Skill> OnSkillActivated;
        public event Action<Skill> OnSkillDeactivated;
        public event Action<Skill, float> OnSkillCooldownChanged;
        
        private void Start()
        {
            InitializeSkillDatabase();
        }
        
        private void Update()
        {
            UpdateSkillCooldowns();
        }
        
        private void InitializeSkillDatabase()
        {
            // 기본 스킬들 생성
            CreateDefaultSkills();
        }
        
        private void CreateDefaultSkills()
        {
            // 기본 공격 스킬
            skillDatabase.Add(new Skill
            {
                id = 1,
                name = "기본 공격",
                description = "기본적인 물리 공격",
                skillType = SkillType.Attack,
                damage = 10,
                manaCost = 0,
                cooldown = 1f,
                range = 2f,
                level = 1,
                requiredLevel = 1,
                isPassive = false
            });
            
            // 힐 스킬
            skillDatabase.Add(new Skill
            {
                id = 2,
                name = "힐",
                description = "체력을 회복합니다",
                skillType = SkillType.Heal,
                healAmount = 30,
                manaCost = 15,
                cooldown = 3f,
                range = 5f,
                level = 1,
                requiredLevel = 2,
                isPassive = false
            });
            
            // 파이어볼 스킬
            skillDatabase.Add(new Skill
            {
                id = 3,
                name = "파이어볼",
                description = "화염 마법 공격",
                skillType = SkillType.Magic,
                damage = 25,
                manaCost = 20,
                cooldown = 2f,
                range = 8f,
                level = 1,
                requiredLevel = 3,
                isPassive = false
            });
            
            // 버프 스킬
            skillDatabase.Add(new Skill
            {
                id = 4,
                name = "힘의 축복",
                description = "공격력을 증가시킵니다",
                skillType = SkillType.Buff,
                buffType = BuffType.Attack,
                buffValue = 10,
                buffDuration = 30f,
                manaCost = 25,
                cooldown = 60f,
                range = 10f,
                level = 1,
                requiredLevel = 5,
                isPassive = false
            });
        }
        
        public bool LearnSkill(int skillId)
        {
            Skill skill = GetSkillById(skillId);
            if (skill == null || learnedSkills.Contains(skill))
                return false;
            
            learnedSkills.Add(skill);
            OnSkillLearned?.Invoke(skill);
            
            Debug.Log($"스킬 '{skill.name}'을 학습했습니다.");
            return true;
        }
        
        public bool UseSkill(int skillId, Vector3 targetPosition)
        {
            Skill skill = GetSkillById(skillId);
            if (skill == null || !learnedSkills.Contains(skill))
                return false;
            
            // 쿨다운 체크
            if (IsSkillOnCooldown(skillId))
                return false;
            
            // 글로벌 쿨다운 체크
            if (Time.time - lastSkillUseTime < globalCooldown)
                return false;
            
            // 마나 체크
            CharacterSystem characterSystem = GetComponent<CharacterSystem>();
            if (characterSystem != null)
            {
                var stats = characterSystem.GetCurrentStats();
                if (stats.mana < skill.manaCost)
                    return false;
                
                // 마나 소모
                characterSystem.UseMana(skill.manaCost);
            }
            
            // 스킬 사용
            ExecuteSkill(skill, targetPosition);
            
            // 쿨다운 설정
            skillCooldowns[skillId] = Time.time + skill.cooldown;
            lastSkillUseTime = Time.time;
            
            OnSkillUsed?.Invoke(skill);
            return true;
        }
        
        private void ExecuteSkill(Skill skill, Vector3 targetPosition)
        {
            switch (skill.skillType)
            {
                case SkillType.Attack:
                    ExecuteAttackSkill(skill, targetPosition);
                    break;
                case SkillType.Heal:
                    ExecuteHealSkill(skill, targetPosition);
                    break;
                case SkillType.Magic:
                    ExecuteMagicSkill(skill, targetPosition);
                    break;
                case SkillType.Buff:
                    ExecuteBuffSkill(skill, targetPosition);
                    break;
                case SkillType.Debuff:
                    ExecuteDebuffSkill(skill, targetPosition);
                    break;
            }
        }
        
        private void ExecuteAttackSkill(Skill skill, Vector3 targetPosition)
        {
            // 공격 스킬 실행
            Debug.Log($"공격 스킬 '{skill.name}' 사용: {skill.damage} 데미지");
            
            // 실제로는 타겟을 찾아서 데미지를 입히는 로직 구현
            // 예: Collider[] targets = Physics.OverlapSphere(targetPosition, skill.range);
        }
        
        private void ExecuteHealSkill(Skill skill, Vector3 targetPosition)
        {
            // 힐 스킬 실행
            Debug.Log($"힐 스킬 '{skill.name}' 사용: {skill.healAmount} 회복");
            
            CharacterSystem characterSystem = GetComponent<CharacterSystem>();
            if (characterSystem != null)
            {
                characterSystem.Heal(skill.healAmount);
            }
        }
        
        private void ExecuteMagicSkill(Skill skill, Vector3 targetPosition)
        {
            // 마법 스킬 실행
            Debug.Log($"마법 스킬 '{skill.name}' 사용: {skill.damage} 데미지");
            
            // 파이어볼 같은 투사체 생성
            CreateProjectile(skill, targetPosition);
        }
        
        private void ExecuteBuffSkill(Skill skill, Vector3 targetPosition)
        {
            // 버프 스킬 실행
            Debug.Log($"버프 스킬 '{skill.name}' 사용: {skill.buffValue} 증가");
            
            // 버프 효과 적용
            ApplyBuff(skill);
        }
        
        private void ExecuteDebuffSkill(Skill skill, Vector3 targetPosition)
        {
            // 디버프 스킬 실행
            Debug.Log($"디버프 스킬 '{skill.name}' 사용: {skill.debuffValue} 감소");
            
            // 디버프 효과 적용
            ApplyDebuff(skill, targetPosition);
        }
        
        private void CreateProjectile(Skill skill, Vector3 targetPosition)
        {
            // 투사체 생성 로직
            GameObject projectile = new GameObject($"Projectile_{skill.name}");
            projectile.transform.position = transform.position;
            
            // 투사체 컴포넌트 추가
            Projectile projectileScript = projectile.AddComponent<Projectile>();
            projectileScript.Initialize(skill, targetPosition);
        }
        
        private void ApplyBuff(Skill skill)
        {
            // 버프 효과 적용
            StatusEffect buffEffect = new StatusEffect
            {
                effectName = skill.name,
                effectType = EffectType.Buff,
                value = skill.buffValue,
                duration = skill.buffDuration,
                isPermanent = false
            };
            
            CharacterSystem characterSystem = GetComponent<CharacterSystem>();
            if (characterSystem != null)
            {
                characterSystem.AddStatusEffect(buffEffect);
            }
        }
        
        private void ApplyDebuff(Skill skill, Vector3 targetPosition)
        {
            // 디버프 효과 적용
            StatusEffect debuffEffect = new StatusEffect
            {
                effectName = skill.name,
                effectType = EffectType.Debuff,
                value = skill.debuffValue,
                duration = skill.debuffDuration,
                isPermanent = false
            };
            
            // 타겟에게 디버프 적용
            // 실제로는 타겟을 찾아서 적용
        }
        
        private void UpdateSkillCooldowns()
        {
            List<int> expiredCooldowns = new List<int>();
            
            foreach (var kvp in skillCooldowns)
            {
                if (Time.time >= kvp.Value)
                {
                    expiredCooldowns.Add(kvp.Key);
                }
            }
            
            foreach (int skillId in expiredCooldowns)
            {
                skillCooldowns.Remove(skillId);
                Skill skill = GetSkillById(skillId);
                if (skill != null)
                {
                    OnSkillCooldownChanged?.Invoke(skill, 0f);
                }
            }
        }
        
        public bool IsSkillOnCooldown(int skillId)
        {
            return skillCooldowns.ContainsKey(skillId) && Time.time < skillCooldowns[skillId];
        }
        
        public float GetSkillCooldownRemaining(int skillId)
        {
            if (!skillCooldowns.ContainsKey(skillId))
                return 0f;
            
            float remaining = skillCooldowns[skillId] - Time.time;
            return Mathf.Max(0f, remaining);
        }
        
        public Skill GetSkillById(int skillId)
        {
            foreach (var skill in skillDatabase)
            {
                if (skill.id == skillId)
                    return skill;
            }
            return null;
        }
        
        public List<Skill> GetLearnedSkills()
        {
            return new List<Skill>(learnedSkills);
        }
        
        public List<Skill> GetAvailableSkills(int characterLevel)
        {
            List<Skill> availableSkills = new List<Skill>();
            
            foreach (var skill in skillDatabase)
            {
                if (skill.requiredLevel <= characterLevel && !learnedSkills.Contains(skill))
                {
                    availableSkills.Add(skill);
                }
            }
            
            return availableSkills;
        }
        
        public bool CanLearnSkill(int skillId, int characterLevel)
        {
            Skill skill = GetSkillById(skillId);
            if (skill == null || learnedSkills.Contains(skill))
                return false;
            
            return skill.requiredLevel <= characterLevel;
        }
    }
    
    [System.Serializable]
    public class Skill
    {
        public int id;
        public string name;
        public string description;
        public SkillType skillType;
        public int level;
        public int requiredLevel;
        public bool isPassive;
        
        // 스킬 효과
        public int damage;
        public int healAmount;
        public int manaCost;
        public float cooldown;
        public float range;
        
        // 버프/디버프 효과
        public BuffType buffType;
        public int buffValue;
        public float buffDuration;
        public int debuffValue;
        public float debuffDuration;
        
        // 스킬 아이콘
        public Sprite icon;
        
        public Skill()
        {
            skillType = SkillType.Attack;
            level = 1;
            requiredLevel = 1;
            isPassive = false;
            damage = 0;
            healAmount = 0;
            manaCost = 0;
            cooldown = 0f;
            range = 0f;
            buffType = BuffType.None;
            buffValue = 0;
            buffDuration = 0f;
            debuffValue = 0;
            debuffDuration = 0f;
        }
    }
    
    [System.Serializable]
    public class SkillEffect
    {
        public string effectName;
        public EffectType effectType;
        public float value;
        public float duration;
        public bool isPermanent;
        
        public SkillEffect(string name, EffectType type, float val, float dur = 0f, bool permanent = false)
        {
            effectName = name;
            effectType = type;
            value = val;
            duration = dur;
            isPermanent = permanent;
        }
    }
    
    public enum SkillType
    {
        Attack,
        Heal,
        Magic,
        Buff,
        Debuff,
        Passive
    }
    
    public enum BuffType
    {
        None,
        Attack,
        Defense,
        Speed,
        Health,
        Mana
    }
}
