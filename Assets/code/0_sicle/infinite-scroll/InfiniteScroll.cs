using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteScroll : MonoBehaviour
{
    [SerializeField]
    private Facing.Direction m_scrollDirection = Facing.Direction.South;

    [SerializeField]
    private float m_speed = 10.0f;
    
    [SerializeField]
    int m_segmentCount = 2;

    [SerializeField]
    int m_overlapUnits = 0;

    [SerializeField]
    Sprite m_sprite = null;

    [SerializeField]
    Color m_color = Color.white;

    [SerializeField]
    int m_sortingOrder = 0;

    [SerializeField]
    string m_sortingLayerName = "Default";

    public Vector2 MoveVector {  get { return m_speed * Facing.DirectionToVector( m_scrollDirection ); } }

    private Rigidbody2D m_body;

    List<GameObject> m_segmentList = new List<GameObject>();

    private void Awake() {
        var length = Facing.IsVertical( m_scrollDirection ) ? m_sprite.bounds.size.y : m_sprite.bounds.size.x;
        var step = length - m_overlapUnits;
        for( int count = 0; count < m_segmentCount; ++count ) {
            var segment = new GameObject();

            var pos = transform.position - (Vector3)MoveVector.normalized * count * step;
            segment.transform.parent = transform;
            segment.transform.position = pos;

            var sr = segment.AddComponent<SpriteRenderer>();
            sr.sprite = m_sprite;
            sr.color = m_color;
            sr.sortingOrder = m_sortingOrder;
            sr.sortingLayerName = m_sortingLayerName;

            var iss = segment.AddComponent<InfiniteScrollSegment>();
            iss.SegmentCount = m_segmentCount;
            iss.OverlapUnits = m_overlapUnits;
            iss.ScrollDirection = m_scrollDirection;

            m_segmentList.Add( segment );
        }

        m_body = gameObject.AddComponent<Rigidbody2D>();
        m_body.gravityScale = 0.0f;
    }

    private void Start() {
        m_body.velocity = MoveVector;
    }
}
