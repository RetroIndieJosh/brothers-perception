using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteScrollSegment : MonoBehaviour {
    public Facing.Direction ScrollDirection = Facing.Direction.South;
    public Vector3 ScrollVector { get { return Facing.DirectionToVector( ScrollDirection ); } }
    public int OverlapUnits;
    public int SegmentCount;

    private SpriteRenderer m_spriteRenderer;

    private Vector2 Size {  get { return m_spriteRenderer.size; } }
    public Vector2 Center {
        get { return new Vector2( transform.position.x - Size.x / 2, transform.position.y - Size.y / 2 ); }
    }
    private Rect Rectangle {  get { return new Rect( Center, m_spriteRenderer.size ); } }

    private void Awake() {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update() {
        switch ( ScrollDirection ) {
            case Facing.Direction.East: if ( Rectangle.xMin > CameraManager.instance.Rectangle.xMax ) Pop(); break;
            case Facing.Direction.North: if ( Rectangle.yMin > CameraManager.instance.Rectangle.yMax ) Pop(); break;
            case Facing.Direction.South: if ( Rectangle.yMax < CameraManager.instance.Rectangle.yMin ) Pop(); break;
            case Facing.Direction.West: if ( Rectangle.xMax < CameraManager.instance.Rectangle.xMin ) Pop(); break;
        }
    }

    private void OnDrawGizmos() {
        {
            var rect = CameraManager.instance.Rectangle;
            var bottomLeft = new Vector2( rect.xMin, rect.yMin );
            var topRight = new Vector2( rect.xMax, rect.yMax );
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere( bottomLeft, 1.0f );
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere( topRight, 1.0f );
        }

        {
            var rect = Rectangle;
            var bottomLeft = new Vector2( rect.xMin, rect.yMin );
            var topRight = new Vector2( rect.xMax, rect.yMax );
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere( bottomLeft, 1.0f );
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere( topRight, 1.0f );
        }
    }

    private void Pop() {
        var pos = transform.localPosition;
        //Debug.LogFormat( "Size: {0} | Move vec: {1} | Segment count: {2}", m_spriteRenderer.bounds.size.y, MoveVector, SegmentCount );
        var length = Facing.IsVertical( ScrollDirection ) ? m_spriteRenderer.bounds.size.y 
            : m_spriteRenderer.bounds.size.x;
        pos += -ScrollVector.normalized * length * ( SegmentCount );
        transform.localPosition = pos;
    }
}
