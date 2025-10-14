using UnityEngine;
using System.Collections.Generic;
using System;

namespace NowHere.RPG
{
    /// <summary>
    /// 아이템 시스템의 핵심 클래스
    /// 아이템 생성, 관리, 효과를 담당
    /// </summary>
    [System.Serializable]
    public class ItemSystem : MonoBehaviour
    {
        [Header("Item Database")]
        [SerializeField] private List<Item> itemDatabase = new List<Item>();
        [SerializeField] private List<Item> rareItems = new List<Item>();
        [SerializeField] private List<Item> legendaryItems = new List<Item>();
        
        [Header("Drop Settings")]
        [SerializeField] private float baseDropRate = 0.1f;
        [SerializeField] private float rareDropRate = 0.05f;
        [SerializeField] private float legendaryDropRate = 0.01f;
        
        [Header("Item Effects")]
        [SerializeField] private List<ItemEffect> itemEffects = new List<ItemEffect>();
        
        // 이벤트
        public event Action<Item> OnItemCreated;
        public event Action<Item> OnItemDropped;
        public event Action<Item, Vector3> OnItemSpawned;
        
        private void Start()
        {
            InitializeItemDatabase();
        }
        
        private void InitializeItemDatabase()
        {
            // 기본 아이템들 생성
            CreateDefaultItems();
            
            // 희귀 아이템들 생성
            CreateRareItems();
            
            // 전설 아이템들 생성
            CreateLegendaryItems();
        }
        
        private void CreateDefaultItems()
        {
            // 기본 무기들
            itemDatabase.Add(new Item
            {
                id = 1,
                name = "나무 검",
                description = "기본적인 나무로 만든 검",
                itemType = ItemType.Weapon,
                equipmentType = EquipmentType.Weapon,
                rarity = ItemRarity.Common,
                level = 1,
                attackBonus = 5,
                value = 10
            });
            
            itemDatabase.Add(new Item
            {
                id = 2,
                name = "가죽 갑옷",
                description = "기본적인 가죽 갑옷",
                itemType = ItemType.Armor,
                equipmentType = EquipmentType.Chest,
                rarity = ItemRarity.Common,
                level = 1,
                defenseBonus = 3,
                value = 15
            });
            
            // 기본 소모품들
            itemDatabase.Add(new Item
            {
                id = 3,
                name = "체력 포션",
                description = "체력을 회복하는 포션",
                itemType = ItemType.Consumable,
                equipmentType = EquipmentType.None,
                rarity = ItemRarity.Common,
                level = 1,
                healthBonus = 50,
                value = 5,
                isStackable = true,
                maxStackSize = 99
            });
            
            itemDatabase.Add(new Item
            {
                id = 4,
                name = "마나 포션",
                description = "마나를 회복하는 포션",
                itemType = ItemType.Consumable,
                equipmentType = EquipmentType.None,
                rarity = ItemRarity.Common,
                level = 1,
                manaBonus = 30,
                value = 5,
                isStackable = true,
                maxStackSize = 99
            });
        }
        
        private void CreateRareItems()
        {
            rareItems.Add(new Item
            {
                id = 101,
                name = "강철 검",
                description = "강철로 만든 날카로운 검",
                itemType = ItemType.Weapon,
                equipmentType = EquipmentType.Weapon,
                rarity = ItemRarity.Rare,
                level = 5,
                attackBonus = 15,
                criticalChanceBonus = 5,
                value = 100
            });
            
            rareItems.Add(new Item
            {
                id = 102,
                name = "마법사의 로브",
                description = "마법을 강화하는 로브",
                itemType = ItemType.Armor,
                equipmentType = EquipmentType.Chest,
                rarity = ItemRarity.Rare,
                level = 5,
                defenseBonus = 8,
                manaBonus = 20,
                maxManaBonus = 20,
                value = 120
            });
        }
        
        private void CreateLegendaryItems()
        {
            legendaryItems.Add(new Item
            {
                id = 201,
                name = "드래곤 슬레이어",
                description = "드래곤을 잡기 위해 만들어진 전설의 검",
                itemType = ItemType.Weapon,
                equipmentType = EquipmentType.Weapon,
                rarity = ItemRarity.Legendary,
                level = 10,
                attackBonus = 50,
                criticalChanceBonus = 15,
                criticalDamageBonus = 50,
                value = 1000
            });
            
            legendaryItems.Add(new Item
            {
                id = 202,
                name = "불멸의 갑옷",
                description = "불멸의 힘이 깃든 갑옷",
                itemType = ItemType.Armor,
                equipmentType = EquipmentType.Chest,
                rarity = ItemRarity.Legendary,
                level = 10,
                defenseBonus = 30,
                healthBonus = 100,
                maxHealthBonus = 100,
                value = 1200
            });
        }
        
        public Item CreateItem(int itemId)
        {
            Item item = GetItemById(itemId);
            if (item != null)
            {
                Item newItem = new Item(item);
                OnItemCreated?.Invoke(newItem);
                return newItem;
            }
            return null;
        }
        
        public Item CreateRandomItem(int playerLevel = 1)
        {
            float randomValue = UnityEngine.Random.Range(0f, 1f);
            List<Item> targetList;
            
            if (randomValue < legendaryDropRate)
            {
                targetList = legendaryItems;
            }
            else if (randomValue < rareDropRate)
            {
                targetList = rareItems;
            }
            else
            {
                targetList = itemDatabase;
            }
            
            if (targetList.Count > 0)
            {
                Item randomItem = targetList[UnityEngine.Random.Range(0, targetList.Count)];
                Item newItem = new Item(randomItem);
                OnItemCreated?.Invoke(newItem);
                return newItem;
            }
            
            return null;
        }
        
        public Item GetItemById(int itemId)
        {
            // 일반 아이템 데이터베이스에서 검색
            foreach (var item in itemDatabase)
            {
                if (item.id == itemId)
                    return item;
            }
            
            // 희귀 아이템에서 검색
            foreach (var item in rareItems)
            {
                if (item.id == itemId)
                    return item;
            }
            
            // 전설 아이템에서 검색
            foreach (var item in legendaryItems)
            {
                if (item.id == itemId)
                    return item;
            }
            
            return null;
        }
        
        public void SpawnItem(Item item, Vector3 position)
        {
            if (item == null)
                return;
            
            // 아이템 오브젝트 생성
            GameObject itemObject = new GameObject($"Item_{item.name}");
            itemObject.transform.position = position;
            
            // 아이템 컴포넌트 추가
            ItemPickup pickup = itemObject.AddComponent<ItemPickup>();
            pickup.SetItem(item);
            
            // 시각적 표현 추가
            MeshRenderer renderer = itemObject.AddComponent<MeshRenderer>();
            MeshFilter filter = itemObject.AddComponent<MeshFilter>();
            
            // 아이템 등급에 따른 색상 설정
            Material material = new Material(Shader.Find("Standard"));
            switch (item.rarity)
            {
                case ItemRarity.Common:
                    material.color = Color.white;
                    break;
                case ItemRarity.Rare:
                    material.color = Color.blue;
                    break;
                case ItemRarity.Epic:
                    material.color = Color.magenta;
                    break;
                case ItemRarity.Legendary:
                    material.color = Color.yellow;
                    break;
            }
            renderer.material = material;
            
            // 기본 큐브 메시 설정
            filter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            
            // 콜라이더 추가
            BoxCollider collider = itemObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            
            OnItemSpawned?.Invoke(item, position);
        }
        
        public void DropItem(Item item, Vector3 position)
        {
            if (item == null)
                return;
            
            SpawnItem(item, position);
            OnItemDropped?.Invoke(item);
        }
        
        public List<Item> GetItemsByType(ItemType type)
        {
            List<Item> result = new List<Item>();
            
            foreach (var item in itemDatabase)
            {
                if (item.itemType == type)
                    result.Add(item);
            }
            
            foreach (var item in rareItems)
            {
                if (item.itemType == type)
                    result.Add(item);
            }
            
            foreach (var item in legendaryItems)
            {
                if (item.itemType == type)
                    result.Add(item);
            }
            
            return result;
        }
        
        public List<Item> GetItemsByRarity(ItemRarity rarity)
        {
            List<Item> result = new List<Item>();
            
            foreach (var item in itemDatabase)
            {
                if (item.rarity == rarity)
                    result.Add(item);
            }
            
            foreach (var item in rareItems)
            {
                if (item.rarity == rarity)
                    result.Add(item);
            }
            
            foreach (var item in legendaryItems)
            {
                if (item.rarity == rarity)
                    result.Add(item);
            }
            
            return result;
        }
    }
    
    [System.Serializable]
    public class Item
    {
        public int id;
        public string name;
        public string description;
        public ItemType itemType;
        public EquipmentType equipmentType;
        public ItemRarity rarity;
        public int level;
        public int value;
        
        // 스탯 보너스
        public int healthBonus;
        public int maxHealthBonus;
        public int manaBonus;
        public int maxManaBonus;
        public int attackBonus;
        public int defenseBonus;
        public int speedBonus;
        public int criticalChanceBonus;
        public int criticalDamageBonus;
        
        // 스택 관련
        public bool isStackable;
        public int maxStackSize;
        public int currentStackSize;
        
        // 아이템 효과
        public List<ItemEffect> effects;
        
        public Item()
        {
            effects = new List<ItemEffect>();
            currentStackSize = 1;
        }
        
        public Item(Item other)
        {
            id = other.id;
            name = other.name;
            description = other.description;
            itemType = other.itemType;
            equipmentType = other.equipmentType;
            rarity = other.rarity;
            level = other.level;
            value = other.value;
            
            healthBonus = other.healthBonus;
            maxHealthBonus = other.maxHealthBonus;
            manaBonus = other.manaBonus;
            maxManaBonus = other.maxManaBonus;
            attackBonus = other.attackBonus;
            defenseBonus = other.defenseBonus;
            speedBonus = other.speedBonus;
            criticalChanceBonus = other.criticalChanceBonus;
            criticalDamageBonus = other.criticalDamageBonus;
            
            isStackable = other.isStackable;
            maxStackSize = other.maxStackSize;
            currentStackSize = other.currentStackSize;
            
            effects = new List<ItemEffect>(other.effects);
        }
    }
    
    [System.Serializable]
    public class ItemEffect
    {
        public string effectName;
        public EffectType effectType;
        public float value;
        public float duration;
        public bool isPermanent;
        
        public ItemEffect(string name, EffectType type, float val, float dur = 0f, bool permanent = false)
        {
            effectName = name;
            effectType = type;
            value = val;
            duration = dur;
            isPermanent = permanent;
        }
    }
    
    public enum ItemType
    {
        Weapon,
        Armor,
        Accessory,
        Consumable,
        Material,
        Quest
    }
    
    public enum ItemRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }
}
