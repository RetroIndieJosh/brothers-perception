using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {
    private class ItemEntry
    {
        public int count = 0;
        public int itemId = ItemDatabase.NONE;
    }

    public const int CAPACITY = 10;

    [HideInInspector]
    public int Index;

    private ItemEntry[] itemList = new ItemEntry[CAPACITY];

    private void Awake() {
        Index = 0; 

        for( int i = 0; i < CAPACITY; ++i ) {
            itemList[i] = new ItemEntry();
        }
    }

    // add item by reference
    public bool AddItem(Item a_item) {
        var wasAdded = AddItem( a_item.Id );
        if( wasAdded ) Destroy( a_item.gameObject );
        return wasAdded;
    }

    // returns whether we added item - fails if no space left
    public bool AddItem( int a_itemId ) {

        // matches current slot's item
        if( GetCurrentEntry().itemId == a_itemId ) {
            ++itemList[Index].count;
            Debug.LogFormat( "Increase quantity of item ID({0}) to {1} (current slot)", a_itemId, GetCurrentEntry().count );
            return true;
        }

        var entry = GetEntry( a_itemId );
        if ( entry == null ) {

            // find an open slot if available
            for ( int i = 0; i < CAPACITY; ++i ) {
                if ( itemList[i].itemId == ItemDatabase.NONE ) {
                    itemList[i].itemId = a_itemId;
                    itemList[i].count = 1;
                    Debug.LogFormat( "New item in slot {0}", i );
                    return true;
                }
            }

            Debug.LogFormat( "No open slots" );
            return false;
        }

        // increase quantity (have a duplicate in another slot)
        ++entry.count;
        Debug.LogFormat( "Increase quantity of item ID({0}) to {1}", a_itemId, entry.count );
        return true;
    }

    // grab an item from the inventory
    public int RemoveItem(int a_item = ItemDatabase.NONE) {
        var entry = (a_item == ItemDatabase.NONE) ? GetCurrentEntry() : GetEntry(a_item);
        var item = entry.itemId;

        --entry.count;
        if( entry.count == 0 ) {
            entry.itemId = ItemDatabase.NONE;
        }

        return item;
    }

    private void Update() {
        if( Input.GetKeyDown(KeyCode.I)) {
            PrintContents();
        }
    }

    private ItemEntry GetCurrentEntry() {
        return itemList[Index];
    }

    private ItemEntry GetEntry(int item) {
        foreach( var entry in itemList ) {
            if( entry.itemId == item ) {
                return entry;
            }
        }

        return null;
    }

    private void PrintContents() {
        string inventory = "Inventory: ";
        int count = 0;
        foreach( var entry in itemList ) {
            if( entry.itemId != ItemDatabase.NONE ) {
                inventory += string.Format( "{0} ({1}), ", ItemDatabase.instance.GetItem( entry.itemId ).displayName, entry.count );
                ++count;
            }
        }
        if( count == 0 ) {
            Debug.Log( "Inventory empty" );
            return;
        }
        Debug.Log( inventory.Substring( 0, inventory.Length - 2 ) );
    }
}
