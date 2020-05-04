using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Item : MonoBehaviour {
    public string displayName = "Unknown Item";
    
    [TextArea(4, 10)]
    public string description = "A mysterious, nondescript nothing.";

    public Sprite displaySprite = null;

    public bool snapToGrid = false;

    [System.Serializable]
    public enum InteractionType
    {
        None,
        Collect,
        Examine,
        Lift,
        Use
    }

    public InteractionType interactionType = InteractionType.None;

    [Tooltip("Whether to place the item in-world on drop")]
    public bool droppable = false;
    public float slideSpeed = 1.0f;

    public bool isPushable = false;

    [Header("Collect")]

    [SerializeField]
    private UnityEvent m_onCollect;

    [SerializeField]
    private string m_collectCounterName = "";

    public int Id {  get { return m_id; } }

    [SerializeField, HideInInspector]
    private int m_id = ItemDatabase.NONE;

    private bool m_isBeingDestroyed = false;
    private bool m_isDropping = false;
    private bool m_isSliding = false;

    private GameObject m_dropper = null;

    private Counter m_collectCounter = null;

    public void ChangeCounter(int a_change ) {
        if ( m_collectCounter == null ) return;
        m_collectCounter.Add( a_change );
    }

    private void Awake() {
        if ( interactionType == InteractionType.Collect ) {
            m_collectCounter = Counter.FindByName( m_collectCounterName );
            if ( m_collectCounter == null ) {
                Debug.LogFormat( "{0} tried to link to counter {1} but it doesn't exist", gameObject, 
                    m_collectCounterName );
                return;
            }
        }
    }

    private void OnCollisionEnter2D( Collision2D collision ) {
        if ( m_isDropping ) CheckDropOn( collision.gameObject );
        if ( isPushable ) CheckPush( collision.gameObject );
        if ( interactionType == InteractionType.Collect ) CheckCollect( collision.gameObject );
    }

    private void CheckCollect( GameObject a_collector ) {
        var player = a_collector.GetComponent<PlayerController>();
        if ( player == null ) return;
        Debug.LogFormat( "{0} collected {1}", a_collector, gameObject );
        m_onCollect.Invoke();
        Destroy( gameObject );
    }

    // TODO handle give to NPC (target) here
    private void CheckDropOn(GameObject a_target) {
        var item = a_target.GetComponent<Item>();
        var player = m_dropper.GetComponent<PlayerController>();

        // TODO handle giving back to NPC here
        if ( player == null ) return;
        
        // TODO handle giving to NPC here
    }

    private void CheckPush( GameObject a_pusher ) {
        var player = a_pusher.GetComponent<PlayerController>();
        if ( player == null ) return;
        var direction = player.GetComponent<Facing>().DirectionVector;

        // stop if we're going to run into something
        var hit = Physics2D.Raycast( transform.position, direction, 1.0f );
        if ( hit ) return;

        StartCoroutine( StartSlide( direction ) );
    }

    private IEnumerator StartSlide(Vector3 a_direction ) {
        if ( m_isSliding ) yield break;

        m_isSliding = true;
        var moveAmount = 0.0f;

        while( moveAmount < 1.0f ) {
            moveAmount += slideSpeed * Time.deltaTime;
            transform.position += a_direction * slideSpeed * Time.deltaTime;
            yield return null;
        }

        m_isSliding = false;
    }

    private void Start() {
        if( RegionManager.instance != null ) RegionManager.instance.CurRegion.RegisterObject( gameObject );
        SnapToGrid();

        /*
        if ( ItemDatabase.instance.GetId( this ) == ItemDatabase.NONE ) {
            Debug.LogErrorFormat( "Item {0} created but not registered into item database. Destroying item.", 
                displayName );
            Destroy( gameObject );
            return;
        }
        */
    }

    public void Drop(GameObject a_dropper, Vector2 a_pos) {
        m_dropper = a_dropper;
        m_isDropping = true;

        transform.position = a_pos;
        SnapToGrid();
        if ( droppable ) return;

        Destroy( gameObject, 0.5f );
        m_isBeingDestroyed = true;
    }

    // TODO handle for AiController
    //public void Interact( AiController a_actor ) {
    //}

    public void Interact() {
        if ( m_isBeingDestroyed ) return;
        switch(interactionType) {
            case InteractionType.None: return;
            case InteractionType.Examine:
                DialogueManager.instance.Text = description;
                return;
            // TODO make this work without player back reference
            //case InteractionType.Lift: a_player.Pickup( this ); return;
            case InteractionType.Use: return; // TODO find usable component and activate it
        }
    }

    public bool SetId(int a_id) {
        if( a_id != ItemDatabase.instance.GetId(this) ) {
            Debug.LogErrorFormat( "Item DB ID mismatch: expected {0}, got {1}", 
                ItemDatabase.instance.GetId( this ), a_id ); 
            return false;
        }

        m_id = a_id;
        return true;
    }

    private void SnapToGrid() {
        if ( !snapToGrid ) return;

        var prevPos = transform.position;

        var pos = transform.position;
        pos.x = Mathf.Floor( pos.x ) + 0.5f;
        pos.y = Mathf.Floor( pos.y ) + 0.5f;
        transform.position = pos;
    }

    private void OnDestroy() {
        if ( RegionManager.instance == null || RegionManager.instance.IsTransitioning ) return;
        RegionManager.instance.CurRegion.DeregisterObject( gameObject );
    }
}
