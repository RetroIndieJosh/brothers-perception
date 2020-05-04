using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Facing))]
[RequireComponent(typeof(ForwardDetector))]
[RequireComponent(typeof(Shooter))]
[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(AiBehaviour))]
public class XkcdPlayerController : MonoBehaviour {
    [System.Serializable]
    public enum BrotherType {
        Big,
        Little
    }

    [System.Serializable]
    public enum FireMode
    {
        Single,
        Rapid,
        Chain,
        Timed
    }

    enum Action
    {
        Action,
        MoveHorizontal,
        MoveVertical,
        Fire,
        Switch
    }

    [SerializeField]
    private BrotherType m_brotherType = BrotherType.Big;

    [SerializeField]
    public XkcdHazard.World WorldView = XkcdHazard.World.Both;

    [SerializeField]
    public XkcdHazard.World WorldInteract = XkcdHazard.World.Both;

    [SerializeField]
    private bool m_useGamepad = false;

    [SerializeField]
    private SpeechBubble m_speechBubblePrefab = null;

    [SerializeField]
    public FireMode CurFireMode = FireMode.Single;

    [SerializeField]
    private bool m_useSelfRenderer = false;

    [SerializeField]
    private SpriteRenderer m_spriteRenderer;

    public float speed = 10;
    public Transform heldItemTransform;

    public float minInteractDistance = 1;
    public float minInteractDistanceSouth = 1.5f;

    [SerializeField]
    private LayerMask m_wallLayer = 0;

    [SerializeField]
    private OffScreenIndicatorArrow m_offscreenIndicatorArrow = null;

    // TODO hide for big brother (no effect)
    [SerializeField]
    private float m_realWorldFlashTime = 0.1f;

    [SerializeField]
    private float m_maxSwitchDistance = 2.0f;

    private bool m_isActive = false;

    public bool IsHoldingItem { get { return m_heldItem != null; } }

    public XkcdPlayerController Brother {
        get { return GetComponent<AiBehaviour>().Target.GetComponent<XkcdPlayerController>(); }
    }

    private float InteractDistance {
        get {
            if ( m_facing == null ) return 1.0f;
            //return m_facing.direction == Facing.Direction.South ? minInteractDistanceSouth : minInteractDistance;
            return minInteractDistance;
        }
    }

    private Facing m_facing;
    private ForwardDetector m_forwardDetector;
    private Shooter m_gun;
    private Inventory m_inventory;
    private Mover m_mover;

    private XkcdHazard.World m_prevWorldView;

    private Item m_heldItem;
    public SpeechBubble SpeechBubble { get; private set; }

    private string m_switchKeyName;

    private IEnumerator m_messageDelayCoroutine = null;

    [SerializeField]
    private int m_bigBrotherSongId = 0;

    [SerializeField]
    private int m_bigToLittleSongId = 0;

    [SerializeField]
    private int m_littleBrotherSongId = 0;

    [SerializeField]
    private int m_littleToBigSongId = 0;

    public void OnSwitchedToXkcdPlayerController() {
        //Debug.LogFormat( "Activate {0} xkcd player controller", name );

        if( m_brotherType == BrotherType.Big) {
            SongManager.instance.GetSong( m_bigToLittleSongId ).ClearEvents();
            SongManager.instance.GetSong( m_littleToBigSongId ).OnLoop.AddListener( () => {
                SongManager.instance.SetNext( m_bigBrotherSongId );
            } );

            SongManager.instance.SetNext( m_littleToBigSongId, SongManager.TransitionMode.OnBar );
        } else {
            SongManager.instance.GetSong( m_littleToBigSongId ).ClearEvents();
            SongManager.instance.GetSong( m_bigToLittleSongId ).OnLoop.AddListener( () => {
                SongManager.instance.SetNext( m_littleBrotherSongId );
            } );

            SongManager.instance.SetNext( m_bigToLittleSongId, SongManager.TransitionMode.OnBar );
        }

        m_isActive = true;
        GetComponent<SpritePalette>().PaletteIndex = 1;

        UpdateView();

        CameraManager.instance.FollowTarget = transform;
    }

    public void OnSwitchedFromXkcdPlayerController() {
        //Debug.LogFormat( "Deactivate player {0} xkcd player controller", name );

        m_isActive = false;
        GetComponent<SpritePalette>().PaletteIndex = 0;
        if( SpeechBubble != null ) SpeechBubble.Clear();
    }

    public void AfterRespawn() {
        Brother.SpeechBubble.Clear();
        Brother.transform.position = transform.position + Vector3.right * 2.0f;
        //Activate();
        GetComponent<Health>().AfterRespawn.RemoveListener( AfterRespawn );
    }

    public void OnDeath() {
        WorldView = m_prevWorldView;
        //Deactivate();
        //Brother.Deactivate();
        Brother.SpeechBubble.Display( "NOOOOOOO!!!!" );
        GetComponent<Health>().AfterRespawn.AddListener( AfterRespawn );
    }

    public bool TryExit() {
        if( Vector2.Distance(transform.position, Brother.transform.position) > m_maxSwitchDistance ) {
            SpeechBubble.Display( "Can't leave my brother behind..." );
            return false;
        }
        return true;
    }

    public bool Pickup( Item a_item ) {
        m_heldItem = a_item;
        return true;
    }

    public void SwitchBrother() {
        var distance = Vector2.Distance( Brother.transform.position, transform.position );
        if( distance > m_maxSwitchDistance) {
            SpeechBubble.Display( "{Hmm, I'm too far away to switch.}" );
            return;
        }
        //Brother.Activate();
    }

    private void Awake() {
        m_facing = GetComponent<Facing>();
        m_forwardDetector = GetComponent<ForwardDetector>();
        m_gun = GetComponent<Shooter>();
        m_inventory = GetComponent<Inventory>();
        m_mover = GetComponent<Mover>();

        m_prevWorldView = WorldView;

        if ( m_useSelfRenderer ) m_spriteRenderer = GetComponent<SpriteRenderer>();
        if( m_spriteRenderer == null ) {
            Debug.LogErrorFormat( "No Sprite Renderer set for xkcd Player Controller in {0}. Destroying {0}.", name );
            Destroy( gameObject );
            return;
        }
    }

    private void OnTriggerEnter2D( Collider2D collision ) {
        var hazard = collision.gameObject.GetComponent<XkcdHazard>();
        if ( hazard == null ) return;
        switch( hazard.CurKind) {
            case XkcdHazard.Kind.Damage:
                SpeechBubble.Display( "OUCH" );
                if( m_isActive && !hazard.IsVisible ) StartCoroutine( ShowAllFor( m_realWorldFlashTime) );
                break;

            case XkcdHazard.Kind.SlowDown:
                SpeechBubble.Display( "Man my feet keep getting stuck" );
                break;
        }
    }

    private void OnTriggerExit2D( Collider2D collision ) {
        SpeechBubble.Clear();
    }

    private void OnCollisionEnter2D( Collision2D collision ) {
        var hazard = collision.gameObject.GetComponent<XkcdHazard>();
        if ( hazard == null ) return;

        if( hazard.CurKind == XkcdHazard.Kind.Pushable ) {
            if( !hazard.InWorld(WorldView) ) SpeechBubble.Display( "I feel like I'm\npushing something" );
            return;
        }

        StopCoroutine( HandleStuck() );
        StartCoroutine( m_messageDelayCoroutine );
    }

    private IEnumerator HandleStuck() {
        var timeElapsed = 0.0f;
        while ( timeElapsed < 2.0f ) {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        SpeechBubble.Display( "I'm stuck!" );
    }

    private void OnCollisionExit2D( Collision2D collision ) {
        var hazard = collision.gameObject.GetComponent<XkcdHazard>();
        if ( hazard == null ) return;

        if ( m_messageDelayCoroutine != null ) {
            StopCoroutine( m_messageDelayCoroutine );
            m_messageDelayCoroutine = null;
        }
        SpeechBubble.Clear();
    }

    private void OnDrawGizmosSelected() {
        if ( !Application.isPlaying ) return;

        var rayList = m_forwardDetector.GetRays();
        var ray = rayList[rayList.Length / 2];

        if ( m_facing.direction == Facing.Direction.South ) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere( ray.origin + ray.direction * minInteractDistanceSouth, 0.1f );
        } else {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere( ray.origin + ray.direction * minInteractDistance, 0.1f );
        }
    }

    private void Update() {
        if ( !CameraManager.instance.Rectangle.Contains( transform.position ) ) {
            if ( SpeechBubble.IsVisible ) m_offscreenIndicatorArrow.HideArrow();
            else m_offscreenIndicatorArrow.ShowArrow();
        }

        if ( RegionManager.instance != null && RegionManager.instance.IsTransitioning ) {
            m_spriteRenderer.color = Color.clear;
            m_mover.Stop();
            return;
        }

        // TODO: WHY?!
        m_spriteRenderer.color = Color.white;

        if( m_heldItem != null ) m_heldItem.transform.position = heldItemTransform.position;

        if ( !m_isActive ) return;

        if ( InputManager.instance.IsDown( "Setup Inputs" ) ) {
            SetupInputs( !m_useGamepad );
            Brother.SetupInputs( m_useGamepad );
        }

        if ( m_brotherType == BrotherType.Little ) {
            var hitList = new List<RaycastHit2D>();
            foreach( Facing.Direction dir in System.Enum.GetValues(typeof(Facing.Direction))) {
                if ( dir == Facing.Direction.None ) continue;

                var dirVec = Facing.DirectionToVector( dir );
                var hitArr = Physics2D.RaycastAll( transform.position, dirVec, 1.0f );
                hitList.AddRange( hitArr );
            }

            bool didHit = false;
            foreach( var hit in hitList) {
                if ( hit ) {
                    var hazard = hit.collider.GetComponent<XkcdHazard>();
                    if ( hazard != null && hazard.CurKind == XkcdHazard.Kind.Damage 
                        && hazard.CurWorld == XkcdHazard.World.Real ) {

                        Brother.SpeechBubble.Display( "WATCH OUT!" );

                        Debug.Log( "Hit on " + hit.collider );

                        didHit = true;
                        break;
                    } 
                } 
            }
            if( !didHit) Brother.SpeechBubble.Clear();
        }

        // INPUT

        if ( DialogueManager.instance != null && DialogueManager.instance.IsShowing
            && InputManager.instance != null ) {

            if ( InputManager.instance.IsDown( Action.Action.ToString() ) ) {
                DialogueManager.instance.RevealOrNextPage();
            }
            DialogueManager.instance.AcceleratePrint = InputManager.instance.IsHeld( Action.Fire.ToString() );
            return;
        }

        if ( InputManager.instance.IsDown( m_switchKeyName ) ) SwitchBrother();

        HandleMove();

        if ( InputManager.instance.IsDown( "test" ) ) {
            if ( SpeechBubble.IsVisible ) SpeechBubble.Clear();
            else SpeechBubble.Display( string.Format( "I am {0} Brother", m_brotherType.ToString() ) );

            if ( RegionManager.instance != null ) RegionManager.instance.startRegion.PrintRegistries();
        }

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
            if ( m_heldItem == null ) {
                if ( !TryInteract() ) SpeechBubble.Clear();
            } else TryThrow();
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

    private void Start() {
        if( InputManager.instance == null ) {
            Debug.LogErrorFormat( "Player Controller requires Input Manager. Destroying in {0}.", name );
            Destroy( this );
            return;
        }

        if( m_speechBubblePrefab == null ) {
            Debug.LogErrorFormat( "xkcd Player Controller requires a Speech Bubble prefab. Destroying {0}.", name );
            Destroy( gameObject );
            return;
        }

        SpeechBubble = Instantiate( m_speechBubblePrefab );
        SpeechBubble.transform.parent = transform;
        SpeechBubble.transform.position = transform.position;

        m_switchKeyName = Action.Switch.ToString() + m_brotherType.ToString();
        SetupInputs( false );

        var health = GetComponent<Health>();
        if( health == null ) {
            Debug.LogErrorFormat( "{0} is missing Health.", health );
            return;
        }
        health.OnDeath.AddListener( OnDeath );
    }

    public void SetupInputs(bool a_useGamepad) {
        m_useGamepad = a_useGamepad;
        if ( m_useGamepad ) {
            Debug.Log( "Switch to gamepad" );

            InputManager.instance.AddAxis( Action.MoveHorizontal.ToString(), InputManager.GamepadAxis.LeftHorizontal,
                0.3f );
            InputManager.instance.AddAxis( Action.MoveVertical.ToString(), InputManager.GamepadAxis.LeftVertical, 
                0.3f, true );
            InputManager.instance.AddButton( Action.Action.ToString(), KeyCode.Joystick1Button0 );
            InputManager.instance.AddButton( Action.Fire.ToString(), KeyCode.Joystick1Button1 );
            //InputManager.instance.AddButton( m_switchKeyName, m_switchButton );
            InputManager.instance.AddButton( "Setup Inputs", KeyCode.Return );
        } else {
            Debug.Log( "Switch to keyboard" );

            InputManager.instance.AddButton( Action.MoveHorizontal.ToString(), KeyCode.RightArrow, KeyCode.LeftArrow );
            InputManager.instance.AddButton( Action.MoveVertical.ToString(), KeyCode.UpArrow, KeyCode.DownArrow );
            InputManager.instance.AddButton( Action.Action.ToString(), KeyCode.Z );
            InputManager.instance.AddButton( Action.Fire.ToString(), KeyCode.X );
            //InputManager.instance.AddButton( m_switchKeyName, m_switchKey );
            InputManager.instance.AddButton( "Setup Inputs", KeyCode.JoystickButton0 );
        }
    }

    private IEnumerator ShowAllFor(float a_sec) {
        if ( WorldView == XkcdHazard.World.Both ) yield break;

        m_prevWorldView = WorldView;
        WorldView = XkcdHazard.World.Both;
        UpdateView();

        var timeElapsed = 0.0f;
        while( timeElapsed < a_sec) {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        WorldView = m_prevWorldView;
        UpdateView();
    }

    private void UpdateView() {
        foreach ( var hazard in FindObjectsOfType<XkcdHazard>() )
            hazard.IsVisible = hazard.InWorld( WorldView );
    }

    private void HandleMove() {
        var move = new Vector2( InputManager.instance.GetAxis( Action.MoveHorizontal.ToString() ), 
            InputManager.instance.GetAxis( Action.MoveVertical.ToString() ) );

        m_forwardDetector.MaxDistance = InteractDistance;
        m_mover.SetDirection( move );
    }

    private bool TryInteract() {
        Debug.Log( "Try interact" );

        var target = m_forwardDetector.GetTarget();
        if ( target == null ) {
            Debug.Log( "no target" );
            return false;
        }
        Debug.Log( "Target is " + target );

        var hazard = target.GetComponent<XkcdHazard>();
        if ( hazard == null ) {
            Debug.Log( "no hazard" );
            return false;
        }

        if( hazard.CurKind == XkcdHazard.Kind.Switch ) {
            Debug.Log( "Trying to toggle switch" );
            var s = hazard.GetComponent<Switch>();
            if( s.CurStyle == Switch.Style.Activate ) s.ToggleState();
            return true;
        }

        if ( hazard.LookDescription == null || hazard.LookDescription.Length == 0 ) {
            Debug.Log( "Showing default look description" );
            switch ( hazard.CurKind ) {
                case XkcdHazard.Kind.Damage: SpeechBubble.Display( "Looks kinda dangerous!" ); break;
                case XkcdHazard.Kind.None: SpeechBubble.Display( "Dunno what that is..." ); break;
                case XkcdHazard.Kind.Pushable: SpeechBubble.Display( "I could push this." ); break;
                case XkcdHazard.Kind.SlowDown: SpeechBubble.Display( "This would slow my progress." ); break;
                case XkcdHazard.Kind.Solid: SpeechBubble.Display( "Looks pretty solid." ); break;
            }
        } else {
            Debug.Log( "Showing custom look description" );
            SpeechBubble.Display( hazard.LookDescription );
        }
        return true;

        /*
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
        */
    }

    bool TryThrow() {
        var wall = m_forwardDetector.GetTarget( m_wallLayer, InteractDistance );

        if ( wall != null || ItemDatabase.instance == null ) return false;

        var target = m_forwardDetector.GetTarget( ItemDatabase.instance.itemLayer, InteractDistance );

        // TODO continue if it's an NPC or bin you can give the item to
        if ( target != null ) return false;

        var rayList = m_forwardDetector.GetRays();
        var ray = rayList[rayList.Length / 2];
        var placement = ray.origin + ray.direction * InteractDistance;

        if ( !RegionManager.instance.CurRegion.IsInBounds( placement ) ) return false;

        m_heldItem.Drop( gameObject, placement );
        m_heldItem = null;
        return true;
    }
}
