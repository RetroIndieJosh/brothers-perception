using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterBar : MonoBehaviour {
    [System.Serializable]
    private enum SizeMode
    {
        Stretch,
        Tiled
    }

    [System.Serializable]
    private enum SizeDirection
    {
        Horizontal,
        Vertical,
        Both
    }

    [SerializeField]
    private Facing.Direction m_anchor = Facing.Direction.None;

    [SerializeField]
    private SizeDirection m_sizeDirection;

    [SerializeField]
    private SizeMode m_stretchMode = SizeMode.Tiled;

    [SerializeField]
    SpriteRenderer m_frontBar = null;

    [SerializeField]
    SpriteRenderer m_backBar = null;

    [SerializeField]
    Counter m_counter = null;

    [SerializeField]
    float m_maxWidth = 1.0f;

    [SerializeField]
    float m_maxHeight = 0.1f;

    [SerializeField]
    float m_minSize = 0.0f;

    private Vector2 WidthFullVec { get { return Vector2.right * m_maxWidth; } }
    private Vector2 WidthMinVec { get { return Vector2.right * m_minSize; } }
    private Vector2 WidthRangeVec {  get { return WidthFullVec - WidthMinVec; } }
    private Vector2 WidthPercentVec { get { return WidthRangeVec * m_counter.Percent; } }

    private Vector2 HeightFullVec { get { return Vector2.up * m_maxHeight; } }
    private Vector2 HeightMinVec { get { return Vector2.up * m_minSize; } }
    private Vector2 HeightRangeVec {  get { return HeightFullVec - HeightMinVec; } }
    private Vector2 HeightPercentVec { get { return HeightRangeVec * m_counter.Percent; } }

    private Vector2 PercentVec {
        get {
            switch ( m_sizeDirection ) {
                case SizeDirection.Both: return WidthPercentVec + WidthMinVec + HeightPercentVec + HeightMinVec;
                case SizeDirection.Horizontal: return WidthPercentVec + WidthMinVec + HeightFullVec;
                case SizeDirection.Vertical: return HeightPercentVec + HeightMinVec + WidthFullVec;
                default: return Vector2.one;
            }
        }
    }
    private Vector2 FullVec {  get { return WidthFullVec + HeightFullVec; } }

    private void Start() {
        if ( m_counter == null ) {
            Debug.LogErrorFormat( "No counter in Counter Bar in {0}. Destroying.", name );
            Destroy( this );
            return;
        }

        if ( m_frontBar == null && m_backBar == null ) {
            Debug.LogWarningFormat( "Created empty Counter Bar in {0}. Destroying.", name );
            Destroy( this );
            return;
        }

        if ( m_frontBar != null ) {
            if ( m_frontBar.gameObject == gameObject ) {
                Debug.LogErrorFormat( "Front Bar for Counter Bar must be on different object, but both on {0}", name );
                Destroy( this );
                return;
            }

            m_frontBar.drawMode = SpriteDrawMode.Tiled;
            if ( m_stretchMode == SizeMode.Stretch ) {
                m_frontBar.drawMode = SpriteDrawMode.Simple;
                m_frontBar.transform.localScale = FullVec;
            } else {
                m_frontBar.drawMode = SpriteDrawMode.Tiled;
                m_frontBar.size = FullVec;
            }

            if ( m_backBar != null ) m_frontBar.sortingOrder = m_backBar.sortingOrder + 10;
        }

        if ( m_backBar != null ) {
            if ( m_backBar.gameObject == gameObject ) {
                Debug.LogErrorFormat( "Back Bar for Counter Bar must be on different object, but both on {0}", name );
                Destroy( this );
                return;
            }

            if ( m_stretchMode == SizeMode.Stretch ) {
                m_backBar.drawMode = SpriteDrawMode.Simple;
                m_backBar.transform.localScale = FullVec;
            } else {
                m_backBar.drawMode = SpriteDrawMode.Tiled;
                m_backBar.size = FullVec;
            }
        }
    }

    private void AnchorEast() {
        var pos = m_frontBar.transform.position;
        pos.x = transform.position.x + UnfilledWidth / 2.0f;
        m_frontBar.transform.position = pos;
    }

    private void AnchorNorth() {
        var pos = m_frontBar.transform.position;
        pos.y = transform.position.y + UnfilledHeight / 2.0f;
        m_frontBar.transform.position = pos;
    }

    private void AnchorSouth() {
        var pos = m_frontBar.transform.position;
        pos.y = transform.position.y - UnfilledHeight / 2.0f;
        m_frontBar.transform.position = pos;
    }

    private float UnfilledHeight {
        get {
            if ( m_stretchMode == SizeMode.Stretch ) return HeightFullVec.y - m_frontBar.transform.localScale.y;
            else return HeightFullVec.y - m_frontBar.size.y;
        }
    }

    private float UnfilledWidth {
        get {
            if ( m_stretchMode == SizeMode.Stretch ) return WidthFullVec.x - m_frontBar.transform.localScale.x;
            else return WidthFullVec.x - m_frontBar.size.x;
        }
    }

    private void AnchorWest() {
        var pos = m_frontBar.transform.position;
        pos.x = transform.position.x - UnfilledWidth / 2.0f;
        m_frontBar.transform.position = pos;
    }

    private void Update() {
        Debug.LogFormat( "Counter: {0}%", m_counter.Percent );
        if ( m_frontBar == null ) return;

        if ( m_stretchMode == SizeMode.Stretch ) m_frontBar.transform.localScale = PercentVec;
        else {
            //m_frontBar.size = m_anchor == Facing.Direction.East ? -PercentVec : PercentVec;
            m_frontBar.size = PercentVec;
            if ( Facing.GetHorizontal( m_anchor ) == Facing.Direction.East ) m_frontBar.flipX = true;
            if ( Facing.GetVertical( m_anchor ) == Facing.Direction.North ) m_frontBar.flipY = true;
        }

        switch( m_anchor) {
            case Facing.Direction.East: AnchorEast(); break;
            case Facing.Direction.North: AnchorNorth(); break;
            case Facing.Direction.Northeast: AnchorEast(); AnchorNorth(); break;
            case Facing.Direction.Northwest: AnchorNorth(); AnchorWest(); break;
            case Facing.Direction.South: AnchorSouth(); break;
            case Facing.Direction.Southeast: AnchorSouth(); AnchorEast(); break;
            case Facing.Direction.Southwest: AnchorSouth(); AnchorEast(); break;
            case Facing.Direction.West: AnchorWest(); break;
        }
    }
}
