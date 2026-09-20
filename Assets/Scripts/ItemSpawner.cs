using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnableItem
    {
        public string label;           
        public GameObject prefab;     
        [Min(0f)] public float weight = 1f; 
    }

    [Header("Item List")]
   private SpawnableItem[] items;      // ลิสต์ไอเทมทั้งหมดที่จะสุ่ม พร้อม weight

    [Header("Spawn Settings")]
   private float spawnInterval = 1f;   // เว้นระยะกี่วินาทีต่อการ spawn 1 ครั้ง
    private float spawnY = 20f;          // ตำแหน่ง Y ที่จะ spawn (ขอบบนจอ)
    private float minX = -14f;           // ขอบซ้ายสุดที่ spawn ได้
   private float maxX = 17f;            // ขอบขวาสุดที่ spawn ได้

    private float timer;
    private float totalWeight;

    void Start()
    {
        RecalculateTotalWeight();
        timer = spawnInterval; // ให้ spawn ทันทีตัวแรกโดยไม่ต้องรอ
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnRandomItem();
        }
    }

    void RecalculateTotalWeight()
    {
        totalWeight = 0f;
        foreach (var item in items)
        {
            totalWeight += item.weight;
        }
    }

    GameObject PickRandomPrefab()
    {
        if (items == null || items.Length == 0 || totalWeight <= 0f)
        {
            Debug.LogWarning("ItemSpawner: ยังไม่ได้ตั้งค่า items หรือ weight รวมเป็น 0");
            return null;
        }

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var item in items)
        {
            cumulative += item.weight;
            if (roll <= cumulative)
            {
                return item.prefab;
            }
        }

        // เผื่อกรณี floating point คลาดเคลื่อน ให้ return ตัวสุดท้าย
        return items[items.Length - 1].prefab;
    }

    void SpawnRandomItem()
    {
        GameObject prefab = PickRandomPrefab();
        if (prefab == null) return;

        float spawnX = Random.Range(minX, maxX);
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}