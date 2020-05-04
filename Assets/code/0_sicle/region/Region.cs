using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Region : MonoBehaviour {
    [SerializeField]
    private string m_targetSceneName;

    [SerializeField]
    private GameObject m_wallPrefab;
    
    [SerializeField]
    private Vector3Int m_extents = new Vector3Int(10, 10, 1);

    [SerializeField]
    private Tile m_floorTile = null;

    [SerializeField]
    private bool m_showBorderGizmo = false;

    [SerializeField]
    private Color m_borderGizmoColor = Color.white;

    [Header("Boarder Sprite")]

    [SerializeField]
    private Sprite m_borderSprite = null;

    [SerializeField]
    private Color m_borderSpriteColor = Color.white;

    [SerializeField]
    private Vector2 m_allowBeyondExtentsMin = Vector2.one;
    public Vector2 AllowBeyondExtensMin {  get { return m_allowBeyondExtentsMin; } }

    [SerializeField]
    private Vector2 m_allowBeyondExtentsMax = Vector2.one;
    public Vector2 AllowBeyondExtensMax {  get { return m_allowBeyondExtentsMax; } }

    public Vector3Int Extents { get { return m_extents; } }

    public Tile FloorTile {  get { return m_floorTile; } }

    public int Bottom { get { return -Size.y / 2; } }
    public int Left { get { return -Size.x / 2; } }
    public int Right { get { return Size.x / 2; } }
    public int Top { get { return Size.y / 2; } }

    public Vector3Int Size {
        get {
            return new Vector3Int( m_extents.x * 2, m_extents.y * 2, 1 );
        }
    }

    public bool IsInBounds(Vector2 a_pos) {
        return a_pos.x > Left && a_pos.x < Right && a_pos.y > Bottom && a_pos.y < Top;
    }

    class ItemEntry
    {
        public int id;
        public Vector3 pos;
    }

    // while the region is active, we add all item objects to this registry
    private List<GameObject> m_objectRegistry = new List<GameObject>();

    // when the region is deactivated, it stores an entry in this registry for
    // each item in the object registry
    // when the region is reactivated, it uses this registry to reconstruct the
    // objects in the game and adds them to the object registry
    private List<ItemEntry> m_itemRegistry = new List<ItemEntry>();

    public void CreateWalls() {
        var eastWall = Instantiate( m_wallPrefab );
        eastWall.name = "East Wall";
        eastWall.transform.parent = transform;
        eastWall.transform.position = new Vector2( Extents.x + 0.5f, 0.0f );
        eastWall.GetComponent<BoxCollider2D>().size = new Vector2( 1.0f, Size.y );
        m_wallList.Add( eastWall );

        var northWall = Instantiate( m_wallPrefab );
        northWall.name = "North Wall";
        northWall.transform.parent = transform;
        northWall.transform.position = new Vector2( 0.0f, Extents.y + 0.5f );
        northWall.GetComponent<BoxCollider2D>().size = new Vector2( Size.x, 1.0f );
        m_wallList.Add( northWall );

        var southWall = Instantiate( m_wallPrefab );
        southWall.name = "South Wall";
        southWall.transform.parent = transform;
        southWall.transform.position = new Vector2( 0.0f, -Extents.y - 0.5f );
        southWall.GetComponent<BoxCollider2D>().size = new Vector2( Size.x, 1.0f );
        m_wallList.Add( southWall );

        var westWall = Instantiate( m_wallPrefab );
        westWall.name = "West Wall";
        westWall.transform.parent = transform;
        westWall.transform.position = new Vector2( -Extents.x - 0.5f, 0.0f );
        westWall.GetComponent<BoxCollider2D>().size = new Vector2( 1.0f, Size.y );
        m_wallList.Add( westWall );

        var borderRenderer = gameObject.AddComponent<SpriteRenderer>();
        borderRenderer.sprite = m_borderSprite;
        borderRenderer.drawMode = SpriteDrawMode.Sliced;
        borderRenderer.size = new Vector2( Size.x, Size.y );
        borderRenderer.color = m_borderSpriteColor;
    }

    // remove an object from this region (e.g. on destroy)
    public void DeregisterObject(GameObject a_obj) {
        if ( !m_objectRegistry.Contains( a_obj ) ) {
            Debug.LogErrorFormat(
                "Tried to deregister {0} from region {1} but it isn't there", 
                a_obj, gameObject );
            return;
        }
        m_objectRegistry.Remove( a_obj );
    }

    public void DestroyWalls() {
        foreach( var wall in m_wallList)
            Destroy( wall );

        m_wallList.Clear();
    }

    // load objects (when this region becomes active)
    // note that CreateItem automatically registers the item to this region
    public void LoadObjects() {
        if ( m_targetSceneName == null || m_targetSceneName == "" ) return;
        SceneManager.LoadScene( m_targetSceneName, LoadSceneMode.Additive );

        foreach(var entry in m_itemRegistry ) {
            ItemDatabase.instance.CreateItem( entry.id, entry.pos );
        }

        m_itemRegistry.Clear();
    }

    public void PrintRegistries() {
        var msg = "Objects registered (" + m_objectRegistry.Count + "):\n";
        foreach( var obj in m_objectRegistry ) {
            msg += obj + "\n";
        }
        msg = msg.Substring( 0, msg.Length - 2 );
        Debug.Log( msg );

        msg = "Items registered (" + m_itemRegistry.Count + "):\n";
        foreach( var entry in m_itemRegistry ) {
            msg += "#" + entry.id + " @ " + entry.pos + "\n";
        }
        msg = msg.Substring( 0, msg.Length - 2 );
        Debug.Log( msg );
    }

    // set an object as part of this region
    public void RegisterObject( GameObject a_obj ) {
        if ( m_objectRegistry.Contains( a_obj ) ) {
            Debug.LogErrorFormat(
                "Tried to register {0} to region {1} but it's already there", 
                a_obj, gameObject );
            return;
        }

        var item = a_obj.GetComponent<Item>();
        if( item == null ) {
            Debug.LogErrorFormat( 
                "Tried to register non-item {0} into region {1}", a_obj, 
                gameObject );
            return;
        }

        m_objectRegistry.Add( a_obj );
    }

    // unload objects (when this region is deactivated)
    public void UnloadObjects() {
        foreach( var obj in m_objectRegistry ) {
            var item = obj.GetComponent<Item>();
            var entry = new ItemEntry();
            entry.id = item.Id;
            entry.pos = item.transform.position;
            m_itemRegistry.Add( entry );

            Destroy( obj );
        }

        m_objectRegistry.Clear();
    }

    private void Start() {
        transform.position = Vector3.zero;
    }

    private void OnDrawGizmos() {
        if ( !m_showBorderGizmo ) return;
        Gizmos.color = m_borderGizmoColor;
        Gizmos.DrawWireCube( Vector3.zero, Size );
    }

    List<GameObject> m_wallList = new List<GameObject>();

    private Vector3Int GetEntrancePos(Transform a_entrance) {
        return a_entrance == null ? Vector3Int.zero 
            : Vector3Int.FloorToInt( a_entrance.position );
    }
}
