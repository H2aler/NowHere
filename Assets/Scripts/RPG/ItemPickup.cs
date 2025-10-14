using UnityEngine;
using NowHere.Player;

namespace NowHere.RPG
{
    /// <summary>
    /// 아이템 픽업을 처리하는 컴포넌트
    /// 플레이어가 아이템을 획득할 수 있도록 함
    /// </summary>
    public class ItemPickup : MonoBehaviour
    {
        [Header("Pickup Settings")]
        [SerializeField] private float pickupRange = 2f;
        [SerializeField] private float pickupDelay = 0.5f;
        [SerializeField] private bool autoPickup = true;
        [SerializeField] private LayerMask playerLayer = 1;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject pickupEffect;
        [SerializeField] private AudioClip pickupSound;
        
        private Item item;
        private float spawnTime;
        private bool canPickup = false;
        private AudioSource audioSource;
        
        // 이벤트
        public System.Action<Item> OnItemPickedUp;
        
        private void Start()
        {
            spawnTime = Time.time;
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void Update()
        {
            // 픽업 가능 시간 체크
            if (!canPickup && Time.time - spawnTime >= pickupDelay)
            {
                canPickup = true;
            }
            
            // 자동 픽업 처리
            if (autoPickup && canPickup)
            {
                CheckForPlayerPickup();
            }
        }
        
        private void CheckForPlayerPickup()
        {
            // 주변 플레이어 감지
            Collider[] players = Physics.OverlapSphere(transform.position, pickupRange, playerLayer);
            
            foreach (var player in players)
            {
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    PickupItem(playerController);
                    break;
                }
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!canPickup) return;
            
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                PickupItem(playerController);
            }
        }
        
        public void PickupItem(PlayerController player)
        {
            if (item == null || !canPickup)
                return;
            
            // 플레이어의 인벤토리에 아이템 추가
            CharacterSystem characterSystem = player.GetComponent<CharacterSystem>();
            if (characterSystem != null)
            {
                bool success = characterSystem.AddItemToInventory(item);
                if (success)
                {
                    // 픽업 효과 재생
                    PlayPickupEffects();
                    
                    // 이벤트 발생
                    OnItemPickedUp?.Invoke(item);
                    
                    // 아이템 오브젝트 제거
                    Destroy(gameObject);
                    
                    Debug.Log($"아이템 '{item.name}'을 획득했습니다!");
                }
                else
                {
                    Debug.Log("인벤토리가 가득 찼습니다!");
                }
            }
        }
        
        private void PlayPickupEffects()
        {
            // 시각적 효과
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }
            
            // 사운드 효과
            if (pickupSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }
        }
        
        public void SetItem(Item newItem)
        {
            item = newItem;
            
            // 아이템 등급에 따른 시각적 효과
            if (item != null)
            {
                UpdateVisualEffects();
            }
        }
        
        private void UpdateVisualEffects()
        {
            // 아이템 등급에 따른 색상 및 효과 설정
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = renderer.material;
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
                        // 전설 아이템은 빛나는 효과 추가
                        material.SetFloat("_Emission", 0.5f);
                        break;
                }
            }
            
            // 전설 아이템의 경우 특별한 효과 추가
            if (item.rarity == ItemRarity.Legendary)
            {
                // 빛나는 파티클 효과 추가
                GameObject glowEffect = new GameObject("GlowEffect");
                glowEffect.transform.SetParent(transform);
                glowEffect.transform.localPosition = Vector3.zero;
                
                // 간단한 빛나는 효과 (실제로는 파티클 시스템 사용 권장)
                Light light = glowEffect.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = Color.yellow;
                light.intensity = 2f;
                light.range = 3f;
            }
        }
        
        public Item GetItem()
        {
            return item;
        }
        
        public bool CanPickup()
        {
            return canPickup;
        }
        
        private void OnDrawGizmosSelected()
        {
            // 픽업 범위 시각화
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, pickupRange);
        }
    }
}
