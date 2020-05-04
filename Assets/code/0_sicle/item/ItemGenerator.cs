using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGenerator : MonoBehaviour
{
    [System.Serializable]
    class ItemProbability
    {
        public Item item = null;
        public int weight = 1;
    }

    [SerializeField]
    private List<ItemProbability> itemList;

    [SerializeField]
    private int numToGenerate = 5;

    [SerializeField]
    private bool m_repeat = false;

    [SerializeField]
    private bool m_allowOverlap = false;

    [SerializeField]
    private float m_repeatDelaySec = 1.0f;

    [SerializeField]
    private Vector2 m_size = Vector2.one * 10;

    private int Bottom { get { return Mathf.FloorToInt( transform.position.y - m_size.y / 2 ); } }
    private int Left { get { return Mathf.FloorToInt( transform.position.x - m_size.x / 2 ); } }
    private int Right { get { return Mathf.FloorToInt( transform.position.x + m_size.x / 2 ); } }
    private int Top { get { return Mathf.FloorToInt( transform.position.y + m_size.y / 2 ); } }

    private float m_nextSpawnSec = 0.0f;

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube( transform.position, m_size );
    }

    private void Start() {
        Spawn();
        m_nextSpawnSec = m_repeatDelaySec;
    }

    private void Update() {
        if ( !m_repeat ) return;
        m_nextSpawnSec -= Time.deltaTime;
        if ( m_nextSpawnSec > 0.0f ) return;
        Spawn();
        m_nextSpawnSec = m_repeatDelaySec;
    }

    private Item SelectItem() {
        var max = 0.0f;
        foreach ( var prob in itemList ) max += prob.weight;
        var roll = Random.Range( 0.0f, max );
        foreach ( var prob in itemList ) {
            roll -= prob.weight;
            if ( roll <= 0 ) return prob.item;
        }
        return null;
    }

    private void Spawn() {
        var availablePositions = new List<Vector2Int>();
        for ( var x = Left; x < Right; ++x ) {
            for ( var y = Bottom; y < Top; ++y ) {
                availablePositions.Add( new Vector2Int( x, y ) );
            }
        }

        for ( int i = 0; i < numToGenerate; ++i ) {
            var posIndex = Random.Range( 0, availablePositions.Count );
            Vector2 pos = availablePositions[posIndex];
            var item = SelectItem();
            var instance = ItemDatabase.instance.CreateItem( SelectItem(), pos );
            if ( instance == null ) {
                Debug.LogErrorFormat( "Failed to create item '{0}'. Did you add the prefab to the database?",
                    item.GetComponent<Item>().displayName );
                continue;
            }
            if ( !m_allowOverlap ) availablePositions.RemoveAt( posIndex );
        }
    }
}
