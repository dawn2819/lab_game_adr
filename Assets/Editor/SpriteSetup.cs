using UnityEditor;
using UnityEngine;

public class SpriteSetup
{
    [InitializeOnLoadMethod]
    static void SetupSprites()
    {
        string[] files = { "btn_melee", "btn_shield", "btn_solar_flare", "btn_ki_blast", "item_senzu_bean", "item_spike_trap", "item_mystery_box" };
        bool changed = false;
        foreach (string f in files)
        {
            string path = "Assets/Resources/" + f + ".png";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
                changed = true;
            }
        }
        if (changed) Debug.Log("Converted new PNGs to Sprites!");
    }
}
