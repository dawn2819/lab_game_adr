using UnityEngine;
using System.Collections;

public class ItemSpawner : MonoBehaviour
{
    public static ItemSpawner Instance;

    public float spawnInterval = 5f;
    private float timer = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Camera.main == null) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnRandomItem();
        }
    }

    private void SpawnRandomItem()
    {
        int rand = Random.Range(0, 3);
        GameObject itemObj = null;

        if (rand == 0) itemObj = CreateItemX();
        else if (rand == 1) itemObj = CreateItemY();
        else itemObj = CreateItemZ();

        if (itemObj != null)
        {
            float randX = Random.Range(0.1f, 0.9f);
            Vector3 spawnPos = Camera.main.ViewportToWorldPoint(new Vector3(randX, 1.1f, 10f));
            spawnPos.z = 0f;
            itemObj.transform.position = spawnPos;
        }
    }

    private GameObject CreateItemX() // Senzu Bean
    {
        GameObject obj = new GameObject("ItemX_Bean");
        obj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("item_senzu_bean");
        sr.sortingOrder = 5;

        // Chữ "+HP/KI" nổi
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(obj.transform, false);
        txtObj.transform.localPosition = new Vector3(0, -1.2f, 0); // Đưa xuống dưới
        TextMesh tm = txtObj.AddComponent<TextMesh>();
        tm.text = "+HP/KI";
        tm.characterSize = 0.15f;
        tm.fontSize = 40;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.green;
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtObj.GetComponent<Renderer>().material = tm.font.material;
        txtObj.GetComponent<Renderer>().sortingOrder = 6;

        ItemController ic = obj.AddComponent<ItemController>();
        ic.itemType = ItemType.X_SenzuBean;
        return obj;
    }

    private GameObject CreateItemY() // Spike Trap
    {
        GameObject obj = new GameObject("ItemY_Spike");
        obj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("item_spike_trap");
        sr.sortingOrder = 5;

        // Chữ "DANGER" cảnh báo
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(obj.transform, false);
        txtObj.transform.localPosition = new Vector3(0, -1.2f, 0);
        TextMesh tm = txtObj.AddComponent<TextMesh>();
        tm.text = "DANGER!";
        tm.characterSize = 0.15f;
        tm.fontSize = 40;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.red;
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtObj.GetComponent<Renderer>().material = tm.font.material;
        txtObj.GetComponent<Renderer>().sortingOrder = 6;

        ItemController ic = obj.AddComponent<ItemController>();
        ic.itemType = ItemType.Y_SpikeTrap;
        return obj;
    }

    private GameObject CreateItemZ() // Mystery Box
    {
        GameObject obj = new GameObject("ItemZ_Box");
        obj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("item_mystery_box");
        sr.sortingOrder = 5;

        // Chữ "+SCORE"
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(obj.transform, false);
        txtObj.transform.localPosition = new Vector3(0, -1.2f, 0);
        TextMesh tm = txtObj.AddComponent<TextMesh>();
        tm.text = "+SCORE";
        tm.characterSize = 0.15f;
        tm.fontSize = 40;
        tm.color = Color.yellow;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtObj.GetComponent<Renderer>().material = tm.font.material;
        txtObj.GetComponent<Renderer>().sortingOrder = 6;

        ItemController ic = obj.AddComponent<ItemController>();
        ic.itemType = ItemType.Z_MysteryBox;
        return obj;
    }
}
