using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour {
    public static ItemDatabase instance;

    public const int NONE = -1;

    public LayerMask itemLayer;

    [SerializeField]
    private List<Item> database = new List<Item>();

    public GameObject CreateItem(Item a_item, Vector3 a_pos ) {
        if ( a_item == null || a_item.Id == NONE ) return null;
        return CreateItem( a_item.Id, a_pos );
    }

    public GameObject CreateItem(int a_id, Vector3 a_pos ) {
        if ( a_id >= database.Count ) return null;
        var item = Instantiate( database[a_id].gameObject, a_pos, 
            Quaternion.identity );
        return item;
    }

    public int GetId(Item a_prefab) {
        for( int i = 0; i < database.Count; ++i ) {
            if( database[i] == a_prefab ) return i;
        }
        return NONE;
    }

    public Item GetItem(int a_id) {
        if ( a_id >= database.Count ) return null;
        return database[a_id];
    }

    public Item GetItem(string a_name ) {
        foreach( var item in database) {
            if ( item.name == a_name ) return item;
        }
        return null;
    }

    private void Awake() {
        if ( instance != null ) {
            Destroy( gameObject );
            return;
        }

        instance = this;
        var id = 0;
        foreach( var item in database) {
            item.SetId( id );
            ++id;
        }
    }
}
