using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XkcdHazard : MonoBehaviour {
    [System.Serializable]
    public enum World
    {
        Imaginary,
        Real,
        Both,
        None
    }

    [System.Serializable]
    public enum Kind
    {
        Damage,
        Pushable,
        SlowDown,
        Switch,
        Solid,
        None
    }

    [SerializeField]
    [Tooltip("If enabled, uses enum references to multisprite for current kind; otherwise, uses Sprite Renderer.")]
    private bool m_useMultiSprite = false;

    [SerializeField]
    private Kind m_kind = Kind.Damage;
    public Kind CurKind {
        get { return m_kind; }
        set {
            m_kind = value;
            if ( m_kind == Kind.Pushable || m_kind == Kind.Solid ) GetComponent<Collider2D>().isTrigger = false;
            if ( m_kind == Kind.None || m_kind == Kind.SlowDown ) GetComponent<Collider2D>().isTrigger = true;
            UpdateSprite();
        }
    }

    [SerializeField]
    private World m_world = World.Real;
    public World CurWorld {  get { return m_world; } }

    [SerializeField]
    private SpriteRenderer m_spriteRenderer = null;

    [SerializeField]
    private string m_lookDescription = "";
    public string LookDescription {  get { return m_lookDescription; } }

    [SerializeField]
    private bool m_overrideWorldCollisionLayer = false;

    private Color m_color;
    public Color Color {  get { return m_color; } }
    bool m_isVisible;
    public bool IsVisible {
        get { return m_isVisible; }
        set {
            m_isVisible = value;
            m_spriteRenderer.color = m_isVisible ? m_color : Color.clear;
        }
    }

    public bool InWorld(World a_world) {
        switch( a_world) {
            case World.Both: return true;
            case World.Imaginary: return m_world == World.Imaginary || m_world == World.Both;
            case World.Real: return m_world == World.Real || m_world == World.Both;
            default: return false;
        }
    }

    public void SetKind( string a_kindStr ) {
        try {
            CurKind = (Kind)System.Enum.Parse( typeof( Kind ), a_kindStr );
        } catch( System.ArgumentException ) {
            Debug.LogErrorFormat( "Tried to set hazard to invalid kind '{0}'.", a_kindStr );
        }
    }

    private void Awake() {
        if ( m_spriteRenderer == null ) m_spriteRenderer = GetComponent<SpriteRenderer>();
        if( m_spriteRenderer == null ) {
            Debug.LogErrorFormat( "{0} requires Sprite Renderer", name );
            Destroy( gameObject );
            return;
        }
        m_color = m_spriteRenderer.color;
    }

    private void Start() {
        UpdateSprite();

        if ( !m_overrideWorldCollisionLayer ) {
            if ( m_world == World.Imaginary ) gameObject.layer = XkcdGameManager.instance.ImaginaryLayerInt;
            else if ( m_world == World.Real ) gameObject.layer = XkcdGameManager.instance.RealLayerInt;
        }
    }

    private void UpdateSprite() {
        if ( !m_useMultiSprite ) return;

        var multiSprite = GetComponent<MultiSprite>();

        if( !multiSprite.HasSprite(m_kind.ToString())) {
            Debug.LogErrorFormat( "Tried to set sprite in {0} for hazard kind {1} but no such sprite.", name, 
                m_kind );
            return;
        }

        multiSprite.SetSprite( m_kind.ToString() );
    }
}
