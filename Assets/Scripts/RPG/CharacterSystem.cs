using UnityEngine;
using System.Collections.Generic;
using System;

namespace NowHere.RPG
{
    /// <summary>
    /// RPG 캐릭터 시스템의 핵심 클래스
    /// 캐릭터의 스탯, 스킬, 아이템 등을 관리
    /// </summary>
    [System.Serializable]
    public class CharacterSystem : MonoBehaviour
    {
        [Header("Character Stats")]
        [SerializeField] private CharacterStats baseStats;
        [SerializeField] private CharacterStats currentStats;
        
        [Header("Character Info")]
        [SerializeField] private string characterName = "Player";
        [SerializeField] private int characterLevel = 1;
        [SerializeField] private int experience = 0;
        [SerializeField] private int experienceToNextLevel = 100;
        
        [Header("Equipment")]
        [SerializeField] private EquipmentSlot[] equipmentSlots;
        [SerializeField] private List<Item> inventory = new List<Item>();
        [SerializeField] private int maxInventorySize = 50;
        
        [Header("Skills")]
        [SerializeField] private List<Skill> learnedSkills = new List<Skill>();
        [SerializeField] private List<Skill> activeSkills = new List<Skill>();
        
        [Header("Status Effects")]
        [SerializeField] private List<StatusEffect> activeStatusEffects = new List<StatusEffect>();
        
        // 이벤트
        public event Action<CharacterStats> OnStatsChanged;
        public event Action<int> OnLevelChanged;
        public event Action<int> OnExperienceChanged;
        public event Action<Item> OnItemEquipped;
        public event Action<Item> OnItemUnequipped;
        public event Action<Skill> OnSkillLearned;
        public event Action<StatusEffect> OnStatusEffectAdded;
        public event Action<StatusEffect> OnStatusEffectRemoved;
        
        private void Start()
        {
            InitializeCharacter();
        }
        
        private void InitializeCharacter()
        {
            // 기본 스탯으로 현재 스탯 초기화
            currentStats = new CharacterStats(baseStats);
            
            // 장비 슬롯 초기화
            if (equipmentSlots == null || equipmentSlots.Length == 0)
            {
                InitializeEquipmentSlots();
            }
            
            // 이벤트 발생
            OnStatsChanged?.Invoke(currentStats);
        }
        
        private void InitializeEquipmentSlots()
        {
            equipmentSlots = new EquipmentSlot[]
            {
                new EquipmentSlot(EquipmentType.Head, "머리"),
                new EquipmentSlot(EquipmentType.Chest, "가슴"),
                new EquipmentSlot(EquipmentType.Legs, "다리"),
                new EquipmentSlot(EquipmentType.Feet, "발"),
                new EquipmentSlot(EquipmentType.Weapon, "무기"),
                new EquipmentSlot(EquipmentType.Shield, "방패"),
                new EquipmentSlot(EquipmentType.Accessory1, "액세서리1"),
                new EquipmentSlot(EquipmentType.Accessory2, "액세서리2")
            };
        }
        
        public void AddExperience(int exp)
        {
            experience += exp;
            OnExperienceChanged?.Invoke(experience);
            
            // 레벨업 체크
            while (experience >= experienceToNextLevel)
            {
                LevelUp();
            }
        }
        
        private void LevelUp()
        {
            experience -= experienceToNextLevel;
            characterLevel++;
            experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * 1.2f);
            
            // 레벨업 시 스탯 증가
            currentStats.health += 10;
            currentStats.maxHealth += 10;
            currentStats.mana += 5;
            currentStats.maxMana += 5;
            currentStats.attack += 2;
            currentStats.defense += 1;
            currentStats.speed += 1;
            
            OnLevelChanged?.Invoke(characterLevel);
            OnStatsChanged?.Invoke(currentStats);
            
            Debug.Log($"레벨업! 현재 레벨: {characterLevel}");
        }
        
        public bool EquipItem(Item item)
        {
            if (item == null || item.equipmentType == EquipmentType.None)
                return false;
            
            // 해당 장비 슬롯 찾기
            EquipmentSlot slot = GetEquipmentSlot(item.equipmentType);
            if (slot == null)
                return false;
            
            // 기존 장비 해제
            if (slot.equippedItem != null)
            {
                UnequipItem(slot.equippedItem);
            }
            
            // 새 장비 착용
            slot.equippedItem = item;
            ApplyItemStats(item, true);
            
            OnItemEquipped?.Invoke(item);
            return true;
        }
        
        public bool UnequipItem(Item item)
        {
            if (item == null)
                return false;
            
            EquipmentSlot slot = GetEquipmentSlot(item.equipmentType);
            if (slot == null || slot.equippedItem != item)
                return false;
            
            slot.equippedItem = null;
            ApplyItemStats(item, false);
            
            OnItemUnequipped?.Invoke(item);
            return true;
        }
        
        private void ApplyItemStats(Item item, bool equip)
        {
            int multiplier = equip ? 1 : -1;
            
            currentStats.health += item.healthBonus * multiplier;
            currentStats.maxHealth += item.maxHealthBonus * multiplier;
            currentStats.mana += item.manaBonus * multiplier;
            currentStats.maxMana += item.maxManaBonus * multiplier;
            currentStats.attack += item.attackBonus * multiplier;
            currentStats.defense += item.defenseBonus * multiplier;
            currentStats.speed += item.speedBonus * multiplier;
            
            OnStatsChanged?.Invoke(currentStats);
        }
        
        private EquipmentSlot GetEquipmentSlot(EquipmentType type)
        {
            foreach (var slot in equipmentSlots)
            {
                if (slot.equipmentType == type)
                    return slot;
            }
            return null;
        }
        
        public bool AddItemToInventory(Item item)
        {
            if (inventory.Count >= maxInventorySize)
                return false;
            
            inventory.Add(item);
            return true;
        }
        
        public bool RemoveItemFromInventory(Item item)
        {
            return inventory.Remove(item);
        }
        
        public void LearnSkill(Skill skill)
        {
            if (skill == null || learnedSkills.Contains(skill))
                return;
            
            learnedSkills.Add(skill);
            OnSkillLearned?.Invoke(skill);
        }
        
        public void AddStatusEffect(StatusEffect effect)
        {
            if (effect == null)
                return;
            
            activeStatusEffects.Add(effect);
            effect.ApplyEffect(this);
            OnStatusEffectAdded?.Invoke(effect);
        }
        
        public void RemoveStatusEffect(StatusEffect effect)
        {
            if (effect == null || !activeStatusEffects.Contains(effect))
                return;
            
            activeStatusEffects.Remove(effect);
            effect.RemoveEffect(this);
            OnStatusEffectRemoved?.Invoke(effect);
        }
        
        public void TakeDamage(int damage)
        {
            int actualDamage = Mathf.Max(1, damage - currentStats.defense);
            currentStats.health = Mathf.Max(0, currentStats.health - actualDamage);
            OnStatsChanged?.Invoke(currentStats);
        }
        
        public void Heal(int healAmount)
        {
            currentStats.health = Mathf.Min(currentStats.maxHealth, currentStats.health + healAmount);
            OnStatsChanged?.Invoke(currentStats);
        }
        
        public void UseMana(int manaCost)
        {
            currentStats.mana = Mathf.Max(0, currentStats.mana - manaCost);
            OnStatsChanged?.Invoke(currentStats);
        }
        
        public void RestoreMana(int manaAmount)
        {
            currentStats.mana = Mathf.Min(currentStats.maxMana, currentStats.mana + manaAmount);
            OnStatsChanged?.Invoke(currentStats);
        }
        
        // Getter 메서드들
        public CharacterStats GetCurrentStats() => currentStats;
        public CharacterStats GetBaseStats() => baseStats;
        public int GetLevel() => characterLevel;
        public int GetExperience() => experience;
        public int GetExperienceToNextLevel() => experienceToNextLevel;
        public List<Item> GetInventory() => new List<Item>(inventory);
        public List<Skill> GetLearnedSkills() => new List<Skill>(learnedSkills);
        public List<StatusEffect> GetActiveStatusEffects() => new List<StatusEffect>(activeStatusEffects);
        public EquipmentSlot[] GetEquipmentSlots() => equipmentSlots;
    }
    
    [System.Serializable]
    public class CharacterStats
    {
        public int health;
        public int maxHealth;
        public int mana;
        public int maxMana;
        public int attack;
        public int defense;
        public int speed;
        public int criticalChance;
        public int criticalDamage;
        
        public CharacterStats()
        {
            health = 100;
            maxHealth = 100;
            mana = 50;
            maxMana = 50;
            attack = 10;
            defense = 5;
            speed = 10;
            criticalChance = 5;
            criticalDamage = 150;
        }
        
        public CharacterStats(CharacterStats other)
        {
            health = other.health;
            maxHealth = other.maxHealth;
            mana = other.mana;
            maxMana = other.maxMana;
            attack = other.attack;
            defense = other.defense;
            speed = other.speed;
            criticalChance = other.criticalChance;
            criticalDamage = other.criticalDamage;
        }
    }
    
    [System.Serializable]
    public class EquipmentSlot
    {
        public EquipmentType equipmentType;
        public string slotName;
        public Item equippedItem;
        
        public EquipmentSlot(EquipmentType type, string name)
        {
            equipmentType = type;
            slotName = name;
            equippedItem = null;
        }
    }
    
    public enum EquipmentType
    {
        None,
        Head,
        Chest,
        Legs,
        Feet,
        Weapon,
        Shield,
        Accessory1,
        Accessory2
    }
}
