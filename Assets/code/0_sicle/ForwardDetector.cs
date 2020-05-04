using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Facing))]
public class ForwardDetector : MonoBehaviour
{
    public float horizontalCenter = 0;
    public float horizontalSpacing = 1;
    public int horizontalRayCount = 3;

    public float verticalCenter = 0;
    public float verticalSpacing = 0;
    public int verticalRayCount = 3;

    [SerializeField]
    private float m_backOffsetHorizontal = 0.0f;

    public float MaxDistance = 1.0f;

    private Ray2D[] m_verticalRayList;
    private Ray2D[] m_horizontalRayList;

    private Facing m_facing;

    public Ray2D[] GetRays() {
        if ( m_facing.direction == Facing.Direction.East || m_facing.direction == Facing.Direction.West ) {
            return m_horizontalRayList;
        } 

        return m_verticalRayList;
    }

    public GameObject GetTarget() {
        return GetTarget( MaxDistance );
    }

    public GameObject GetTarget( float a_maxDistance ) {
        var hit = new RaycastHit2D();
        var rayList = GetRays();

        foreach( var ray in rayList ) {
            hit = Physics2D.Raycast( ray.origin, ray.direction, a_maxDistance );
            if ( hit ) break;
        }

        if ( !hit ) return null;

        return hit.collider.gameObject;
    }

    public GameObject GetTarget(LayerMask a_layerMask) {
        return GetTarget( a_layerMask, MaxDistance );
    }

    public GameObject GetTarget( LayerMask a_layerMask, float a_maxDistance ) {
        var hit = new RaycastHit2D();
        var rayList = GetRays();

        foreach( var ray in rayList ) {
            hit = Physics2D.Raycast( ray.origin, ray.direction, a_maxDistance, a_layerMask );
            if ( hit ) break;
        }

        if ( !hit ) return null;

        return hit.collider.gameObject;
    }

    private void Awake() {
        m_facing = GetComponent<Facing>();

        m_horizontalRayList = new Ray2D[horizontalRayCount];
        m_verticalRayList = new Ray2D[verticalRayCount];
    }

    private void OnDrawGizmosSelected() {
        if ( !Application.isPlaying ) return;

        var rayList = GetRays();
        Color[] colors = { Color.green, Color.yellow, Color.magenta };
        for ( int i = 0; i < rayList.Length; ++i ) {
            Gizmos.color = colors[i % 3];
            Utility.GizmoArrow( rayList[i].origin, rayList[i].direction * MaxDistance );
        }
    }

    private void Update() {
        RecalculateRays();
    }

    [SerializeField]
    private float m_backOffsetNorth = 0.0f;

    [SerializeField]
    private float m_backOffsetSouth = 0.0f;

    // TODO also handle separate east/west
    private Ray2D MakeRay(float a_offset, Vector2 a_direction, bool a_isHorizontal ) {
        if( !a_isHorizontal) a_direction *= 1.5f;
        var spacingVector = a_isHorizontal ? Vector2.down : Vector2.right;

        var backOffset = a_direction;
        if ( a_isHorizontal ) backOffset *= m_backOffsetHorizontal;
        else if ( a_direction.y > 0 ) backOffset *= m_backOffsetNorth;
        else backOffset *= m_backOffsetSouth;
        var start = (Vector2)transform.position - backOffset + a_offset * spacingVector;
        return new Ray2D( start, a_direction );
    }

    private void RecalculateRays() {
        if ( !Application.isPlaying ) return;

        var facingVec = (Vector2)m_facing.DirectionVector;
        if ( m_facing.direction == Facing.Direction.East || m_facing.direction == Facing.Direction.West ) {
            var offset = horizontalCenter - horizontalSpacing;
            var inc = ( horizontalSpacing * 2 ) / ( horizontalRayCount - 1 );
            for( int i = 0; i < horizontalRayCount; ++i ) {
                m_horizontalRayList[i] = MakeRay( offset, facingVec, true );
                offset += inc;
            }
        } else {
            var offset = verticalCenter - verticalSpacing;
            var inc = ( verticalSpacing * 2 ) / ( verticalRayCount - 1 );
            for( int i = 0; i < verticalRayCount; ++i ) {
                m_verticalRayList[i] = MakeRay( offset, facingVec, false );
                offset += inc;
            }
        }
    }
} 
