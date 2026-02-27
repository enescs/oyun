using System.Collections.Generic;
using UnityEngine;

public class MapDecorPlacer : MonoBehaviour
{
    [Range(8, 32)]  public int cellSize = 14;
    [Range(0f, 1f)] public float coverage = 0.55f;
    public float pixelsPerUnit = 100f;
    public float spriteZ = -0.5f;

    private List<GameObject> decorObjects = new List<GameObject>();

    public void Repaint(MapGenerator map, BiomePaintSettings settings)
    {
        Clear();
        if (settings == null) return;

        int cols = Mathf.CeilToInt((float)map.width  / cellSize);
        int rows = Mathf.CeilToInt((float)map.height / cellSize);
        float halfW = map.width  * 0.5f / pixelsPerUnit;
        float halfH = map.height * 0.5f / pixelsPerUnit;

        for (int col = 0; col < cols; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                if (Random.value > coverage) continue;

                int tx = Mathf.Clamp(col * cellSize + Random.Range(0, cellSize), 0, map.width  - 1);
                int ty = Mathf.Clamp(row * cellSize + Random.Range(0, cellSize), 0, map.height - 1);

                if (!map.IsLand(tx, ty)) continue;
                if (map.GetFog(tx, ty) > 0.6f) continue;

                Sprite sprite = PickDecorSprite(map.GetBiome(tx, ty), settings);
                if (sprite == null) continue;

                PlaceSprite(sprite, tx, ty, halfW, halfH);
            }
        }
    }

    public void Clear()
    {
        foreach (var go in decorObjects)
            if (go != null) Destroy(go);
        decorObjects.Clear();
    }

    Sprite PickDecorSprite(int biome, BiomePaintSettings s)
    {
        List<Sprite> pool;
        switch (biome)
        {
            case 1: pool = s.agriculturalDecor; break;
            case 2: pool = s.citiesDecor;       break;
            case 3: pool = s.industrialDecor;   break;
            case 4: pool = s.urbanDecor;        break;
            default: return null;
        }
        if (pool == null || pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }

    void PlaceSprite(Sprite sprite, int tx, int ty, float halfW, float halfH)
    {
        GameObject go = new GameObject("Decor");
        go.transform.SetParent(transform);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        float jitterRange = (cellSize * 0.3f) / pixelsPerUnit;
        float wx = (tx / pixelsPerUnit) - halfW + Random.Range(-jitterRange, jitterRange);
        float wy = (ty / pixelsPerUnit) - halfH + Random.Range(-jitterRange, jitterRange);
        go.transform.position = new Vector3(wx, wy, spriteZ);

        float s = Random.Range(0.75f, 1.25f);
        go.transform.localScale = new Vector3(s, s, 1f);
        sr.flipX = Random.value > 0.5f;
        sr.color = new Color(1f, 1f, 1f, Random.Range(0.80f, 0.95f));

        decorObjects.Add(go);
    }
}