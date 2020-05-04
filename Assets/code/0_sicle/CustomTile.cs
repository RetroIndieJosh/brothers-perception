using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public class CustomTile : Tile
{
    [HideInInspector]
    public Item item;


#if UNITY_EDITOR
    [MenuItem( "Assets/Create/CustomTile" )]
    public static void CreateCustomTile() {
        string path = EditorUtility.SaveFilePanelInProject( "Save Custom Tile", "New Custom Tile", "Asset", "Save Custom Tile", "Assets" );
        if ( path == "" )
            return;
        AssetDatabase.CreateAsset( ScriptableObject.CreateInstance<CustomTile>(), path );
    }
#endif // UNITY_EDITOR
}
