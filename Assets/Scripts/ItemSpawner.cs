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
        // Tạo khối chính hình Capsule
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        obj.name = "ItemX_Bean";
        obj.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        
        Collider col = obj.GetComponent<Collider>();
        if (col != null) Destroy(col); // Xóa collider mặc định để dùng circle 2D trong ItemController

        Renderer r = obj.GetComponent<Renderer>();
        if (r != null)
        {
            r.material = new Material(Shader.Find("Sprites/Default"));
            r.material.color = new Color(0.2f, 0.9f, 0.2f, 1f); // Xanh lá sáng
        }

        // Vầng hào quang (Glow) mờ bên ngoài
        GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        glow.name = "Glow";
        glow.transform.SetParent(obj.transform, false);
        glow.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        Destroy(glow.GetComponent<Collider>());
        Renderer gr = glow.GetComponent<Renderer>();
        gr.material = new Material(Shader.Find("Sprites/Default"));
        gr.material.color = new Color(0f, 1f, 0f, 0.3f);

        // Chữ "+HP / +KI" nổi
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(obj.transform, false);
        txtObj.transform.localPosition = new Vector3(0, 1.2f, 0);
        TextMesh tm = txtObj.AddComponent<TextMesh>();
        tm.text = "+HP";
        tm.characterSize = 0.15f;
        tm.fontSize = 60;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.green;
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtObj.GetComponent<Renderer>().material = tm.font.material;

        ItemController ic = obj.AddComponent<ItemController>();
        ic.itemType = ItemType.X_SenzuBean;
        return obj;
    }

    private GameObject CreateItemY() // Spike Trap
    {
        // Lõi là khối cầu đen sẫm
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.name = "ItemY_Spike";
        obj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        Collider col = obj.GetComponent<Collider>();
        if (col != null) Destroy(col);

        Renderer r = obj.GetComponent<Renderer>();
        if (r != null)
        {
            r.material = new Material(Shader.Find("Sprites/Default"));
            r.material.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Đen
        }

        // Tạo 6 cái gai (Spikes) bằng Cylinder đâm ra các hướng
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        foreach(Vector3 dir in directions)
        {
            GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spike.name = "Spike";
            spike.transform.SetParent(obj.transform, false);
            spike.transform.localScale = new Vector3(0.3f, 0.6f, 0.3f);
            spike.transform.localPosition = dir * 0.5f;
            spike.transform.up = dir;
            Destroy(spike.GetComponent<Collider>());
            Renderer sr = spike.GetComponent<Renderer>();
            sr.material = new Material(Shader.Find("Sprites/Default"));
            sr.material.color = Color.red; // Gai đỏ
        }

        ItemController ic = obj.AddComponent<ItemController>();
        ic.itemType = ItemType.Y_SpikeTrap;
        return obj;
    }

    private GameObject CreateItemZ() // Mystery Box
    {
        // Rương là khối hộp vàng
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = "ItemZ_Box";
        obj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        Collider col = obj.GetComponent<Collider>();
        if (col != null) Destroy(col);

        Renderer r = obj.GetComponent<Renderer>();
        if (r != null)
        {
            r.material = new Material(Shader.Find("Sprites/Default"));
            r.material.color = new Color(1f, 0.8f, 0f, 1f); // Vàng Gold
        }

        // Chữ "?"
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(obj.transform, false);
        txtObj.transform.localPosition = new Vector3(0, 0, -0.55f); // Đưa ra mặt trước
        TextMesh tm = txtObj.AddComponent<TextMesh>();
        tm.text = "?";
        tm.characterSize = 0.15f;
        tm.fontSize = 80;
        tm.color = Color.red;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtObj.GetComponent<Renderer>().material = tm.font.material;

        ItemController ic = obj.AddComponent<ItemController>();
        ic.itemType = ItemType.Z_MysteryBox;
        return obj;
    }
}
