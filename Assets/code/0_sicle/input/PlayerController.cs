using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Facing))]
[RequireComponent(typeof(ForwardDetector))]
[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour {
    [System.Serializable]
    public enum FireMode
    {
        Single,
        Rapid,
        Chain,
        Timed
    }

    [SerializeField]
    private bool m_useGamepad = false;

    [SerializeField]
    public FireMode CurFireMode = FireMode.Single;

    [SerializeField]
    private bool m_useSelfRenderer = false;

    [SerializeField]
    private SpriteRenderer m_spriteRenderer;

    public float speed = 10;
    public Transform heldItemTransform;

    public float minPickupDistance = 1;
    public float minPickupDistanceSouth = 1.5f;

    [SerializeField]
    private LayerMask m_wallLayer;

    public bool IsHoldingItem { get { return m_heldItem != null; } }

    private float PickupDistance {
        get {
            if ( m_facing == null ) return 1.0f;
            return m_facing.direction == Facing.Direction.South ? minPickupDistanceSouth : minPickupDistance;
        }
    }

    private Facing m_facing;
    private ForwardDetector m_forwardDetector;
    private Shooter m_gun;
    private Inventory m_inventory;
    private Mover m_mover;

    private Item m_heldItem;

    public bool Pickup( Item a_item ) {
        m_heldItem = a_item;
        return true;
    }

    private void Awake() {
        m_facing = GetComponent<Facing>();
        m_forwardDetector = GetComponent<ForwardDetector>();
        m_gun = GetComponent<Shooter>();
        m_inventory = GetComponent<Inventory>();
        m_mover = GetComponent<Mover>();
    }

    private void OnDrawGizmosSelected() {
        if ( !Application.isPlaying ) return;

        var rayList = m_forwardDetector.GetRays();
        var ray = rayList[rayList.Length / 2];

        if ( m_facing.direction == Facing.Direction.South ) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere( ray.origin + ray.direction * minPickupDistanceSouth, 0.1f );
        } else {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere( ray.origin + ray.direction * minPickupDistance, 0.1f );
        }

    }

    enum Action
    {
        Action,
        MoveHorizontal,
        MoveVertical,
        Fire
    }

    private void Start() {
        if( InputManager.instance == null ) {
            Debug.LogErrorFormat( "Player Controller requires Input Manager. Destroying in {0}.", name );
            Destroy( this );
            return;
        }

        if ( m_useSelfRenderer ) m_spriteRenderer = GetComponent<SpriteRenderer>();
        if( m_spriteRenderer == null ) {
            Debug.LogErrorFormat( "No Sprite Renderer set for Player Controller in {0}. Destroying {0}.", name );
            Destroy( gameObject );
            return;
        }

        if ( m_useGamepad ) {
            InputManager.instance.AddAxis( Action.MoveHorizontal.ToString(), InputManager.GamepadAxis.LeftHorizontal,
                0.3f );
            InputManager.instance.AddAxis( Action.MoveVertical.ToString(), InputManager.GamepadAxis.LeftVertical, 
                0.3f );
            InputManager.instance.AddButton( Action.Action.ToString(), KeyCode.Joystick1Button0 );
            InputManager.instance.AddButton( Action.Fire.ToString(), KeyCode.Joystick1Button1 );
        } else {
            InputManager.instance.AddButton( Action.MoveHorizontal.ToString(), KeyCode.RightArrow, KeyCode.LeftArrow );
            InputManager.instance.AddButton( Action.MoveVertical.ToString(), KeyCode.UpArrow, KeyCode.DownArrow );
            InputManager.instance.AddButton( Action.Action.ToString(), KeyCode.Z );
            InputManager.instance.AddButton( Action.Fire.ToString(), KeyCode.X );
        }
    }

    private void Update () {
        if ( RegionManager.instance != null && RegionManager.instance.IsTransitioning ) {
            m_spriteRenderer.color = Color.clear;
            m_mover.Stop();
            return;
        }

        m_spriteRenderer.color = Color.white;

        if( Input.GetKeyDown(KeyCode.R)) {
            RegionManager.instance.startRegion.PrintRegistries();
        }

        if( m_heldItem != null ) {
            m_heldItem.transform.position = heldItemTransform.position;
        }

        if ( DialogueManager.instance != null && DialogueManager.instance.IsShowing
            && InputManager.instance != null ) {

            if ( InputManager.instance.IsDown( Action.Action.ToString() ) ) {
                DialogueManager.instance.RevealOrNextPage();
            }
            DialogueManager.instance.AcceleratePrint = InputManager.instance.IsHeld( Action.Fire.ToString() );
            return;
        }

        HandleMove();

        switch ( CurFireMode ) {
            case FireMode.Chain:
                if ( InputManager.instance.IsDown( Action.Fire.ToString() ) ) m_gun.StartFire();
                if ( InputManager.instance.IsUp( Action.Fire.ToString() ) ) m_gun.FireForSec( 2.0f );
                break;

            case FireMode.Rapid:
                if ( InputManager.instance.IsDown( Action.Fire.ToString() ) ) m_gun.StartFire();
                if ( InputManager.instance.IsUp( Action.Fire.ToString() ) ) m_gun.StopFire();
                break;

            case FireMode.Single:
                if ( InputManager.instance.IsDown( Action.Fire.ToString() ) ) m_gun.Fire();
                break;

            case FireMode.Timed:
                if ( InputManager.instance.IsDown( Action.Fire.ToString() ) ) m_gun.FireForSec( 2.0f );
                break;
        }

        // TODO handle "give" here
        if ( InputManager.instance.IsDown( Action.Action.ToString() ) ) {
            if ( m_heldItem == null ) TryInteract();
            else TryThrow();
        }

        if( Input.GetKeyDown(KeyCode.Return)) {
            if( m_heldItem == null ) {
                Debug.Log( "Not holding an item" );
            } else {
                bool wasAdded = m_inventory.AddItem( m_heldItem );
                if ( wasAdded ) Debug.LogFormat( "Placed {0} in inventory", m_heldItem.displayName );
                else Debug.Log( "Failed to put item in inventory" );
            }
        }
	}

    void HandleMove() {
        var move = new Vector2( InputManager.instance.GetAxis( Action.MoveHorizontal.ToString() ), 
            InputManager.instance.GetAxis( Action.MoveVertical.ToString() ) );

        m_mover.SetDirection( move );
    }

    bool TryInteract() {
        Debug.Log( "Try interact" );

        if ( ItemDatabase.instance == null ) return false;
        var target = m_forwardDetector.GetTarget( ItemDatabase.instance.itemLayer, PickupDistance );
        if( target == null ) {
            Debug.Log( "No target in reach" );
            return false;
        }
        Debug.Log( "Has a target" );

        var item = target.GetComponent<Item>();
        if ( item == null ) {
            Debug.Log( "Target isn't an item" );
            return false;
        }

        item.Interact();
        return true;
    }

    bool TryThrow() {
        var wall = m_forwardDetector.GetTarget( m_wallLayer, PickupDistance );

        if ( wall != null || ItemDatabase.instance == null ) return false;

        var target = m_forwardDetector.GetTarget( ItemDatabase.instance.itemLayer, PickupDistance );

        // TODO continue if it's an NPC or bin you can give the item to
        if ( target != null ) return false;

        var rayList = m_forwardDetector.GetRays();
        var ray = rayList[rayList.Length / 2];
        var placement = ray.origin + ray.direction * PickupDistance;

        if ( !RegionManager.instance.CurRegion.IsInBounds( placement ) ) return false;

        m_heldItem.Drop( gameObject, placement );
        m_heldItem = null;
        return true;
    }
}
