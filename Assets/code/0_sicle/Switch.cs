using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Switch : MonoBehaviour {
    [System.Serializable]
    enum ActivateType
    {
        Brother,
        Pushable,
        Both
    }

    [System.Serializable]
    public enum Style
    {
        Activate,
        Collide,
        StepOn,
        StayOn
    }

    [System.Serializable]
    public enum State
    {
        Off,
        On
    }

    [SerializeField]
    public Style CurStyle = Style.Activate;

    [SerializeField]
    private ActivateType m_activateType = ActivateType.Both;

    [SerializeField]
    private Sprite m_onSprite = null;

    [SerializeField]
    private Sprite m_offSprite = null;

    [SerializeField]
    private SpriteRenderer m_spriteRenderer = null;

    [SerializeField]
    private State m_state = State.Off;

    [SerializeField]
    public UnityEvent OnSwitchedOn = new UnityEvent();

    [SerializeField]
    public UnityEvent OnSwitchedOff = new UnityEvent();

    public State CurState {
        get { return m_state; }
        private set {
            m_state = value;
            if ( m_state == State.On ) {
                OnSwitchedOn.Invoke();
                m_spriteRenderer.sprite = m_onSprite;
            } else if ( m_state == State.Off ) {
                OnSwitchedOff.Invoke();
                m_spriteRenderer.sprite = m_offSprite;
            }
        }
    }

    public void ToggleState() {
        if ( m_state == State.Off ) CurState = State.On;
        else CurState = State.Off;
    }

    private void OnCollisionEnter2D( Collision2D a_collision ) {
        if ( CurStyle != Style.Collide ) return;
        ToggleState();
    }

    private void OnTriggerEnter2D( Collider2D a_collider ) {
        if ( !CanBeActivatedBy( a_collider.gameObject ) ) return;

        if ( CurStyle == Style.StayOn ) CurState = State.On;
        else if ( CurStyle == Style.StepOn ) ToggleState();
    }

    private void OnTriggerExit2D( Collider2D a_collider ) {
        if ( !CanBeActivatedBy( a_collider.gameObject ) ) return;
        if ( CurStyle != Style.StayOn ) return;

        CurState = State.Off;
    }

    private void Awake() {
        if ( m_spriteRenderer == null ) m_spriteRenderer = GetComponent<SpriteRenderer>();
        if( m_spriteRenderer == null ) {
            Debug.LogErrorFormat( "Switch on {0} is missing Sprite Renderer. Destroying.", name );
            Destroy( gameObject );
            return;
        }
    }

    private void Start() {
        GetComponent<Collider2D>().isTrigger = CurStyle == Style.StepOn || CurStyle == Style.StayOn;
        CurState = m_state;
    }

    private bool CanBeActivatedBy(GameObject a_object) {
        Debug.LogWarning( "Check activate by " + a_object );

        // enable to disable activation from pushable hazards in other world
        //if ( m_activateType == ActivateType.Pushable && a_object.GetComponent<XkcdHazard>() == null ) return false;

        if ( m_activateType == ActivateType.Brother && a_object.GetComponent<XkcdPlayerController>() == null )
            return false;

        return true;
    }
}
