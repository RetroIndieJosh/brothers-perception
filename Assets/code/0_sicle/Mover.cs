using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour {
    [System.Serializable]
    enum ClampMode
    {
        None,
        Direction,
        Horizontal,
        Vertical,
        Cardinal,
        PrimaryIntercardinal
    }

    [SerializeField]
    private bool m_useSelfFacing = false;

    [SerializeField]
    private Facing m_facing = null;

    [SerializeField]
    private Facing.Direction m_initialFacing = Facing.Direction.East;

    [SerializeField]
    private float m_speed = 5.0f;

    [SerializeField]
    ClampMode m_clampMode = ClampMode.None;

    [SerializeField]
    Facing.Direction m_clampDirection = Facing.Direction.East;

    [SerializeField]
    bool m_is2D;

    public float SpeedMultiplier = 1.0f;

    public bool IsMoving { get; private set; }

    private Rigidbody2D m_body2D = null;
    private Rigidbody m_body = null;

    private float Speed {  get { return m_speed * SpeedMultiplier; } }

    public void SetDirection(Vector2 a_directionVec) {
        if( a_directionVec.magnitude == 0) {
            Stop();
            return;
        }

        a_directionVec = ClampDirection( a_directionVec );

        if ( m_is2D ) m_body2D.velocity = a_directionVec * Speed;
        else m_body.velocity = a_directionVec * Speed;

        m_facing.DirectionVector = a_directionVec;

        IsMoving = true;
    }

    public void Stop() {
        if ( m_is2D ) m_body2D.velocity = Vector2.zero;
        else m_body.velocity = Vector3.zero;
        IsMoving = false;
    }

    private void Awake() {
        if ( m_is2D ) {
            m_body2D = GetComponent<Rigidbody2D>();
            if ( m_body2D == null ) {
                Debug.LogErrorFormat( "{0} requires Rigid Body 2D for Mover. Destroying.", name );
                Destroy( gameObject );
                return;
            }
        } else {
            m_body = GetComponent<Rigidbody>();
            if ( m_body == null ) {
                Debug.LogErrorFormat( "{0} requires Rigid Body for Mover. Destroying.", name );
                Destroy( gameObject );
                return;
            }
        }
    }

    private void Start() {
        if ( m_useSelfFacing ) m_facing = GetComponent<Facing>();

        if( m_facing == null ) {
            Debug.LogErrorFormat( "No Facing set for Mover in {0}. Destroying {0}.", name );
            Destroy( gameObject );
            return;
        }
        m_facing.direction = m_initialFacing;
    }

    private Vector2 ClampDirection( Vector2 a_directionVec ) {
        var clampedVec = a_directionVec.normalized;

        if ( Mathf.Abs( clampedVec.magnitude ) < Mathf.Epsilon )
            return clampedVec;

        if ( m_clampMode == ClampMode.Direction ) {
            switch ( m_clampDirection ) {
                case Facing.Direction.East:
                    if ( clampedVec.x > 0 ) return Vector2.right;
                    break;
                case Facing.Direction.North:
                    if ( clampedVec.y > 0 ) return Vector2.up;
                    break;
                case Facing.Direction.Northeast:
                    if ( clampedVec.x > 0 && clampedVec.y > 0 ) return Vector2.right + Vector2.up;
                    break;
                case Facing.Direction.Northwest:
                    if ( clampedVec.x < 0 && clampedVec.y > 0 ) return Vector2.left + Vector2.up;
                    break;
                case Facing.Direction.South:
                    if ( clampedVec.y < 0 ) return Vector2.down;
                    break;
                case Facing.Direction.Southeast:
                    if ( clampedVec.x > 0 && clampedVec.y < 0 ) return Vector2.right + Vector2.down;
                    break;
                case Facing.Direction.Southwest:
                    if ( clampedVec.x < 0 && clampedVec.y < 0 ) return Vector2.left + Vector2.down;
                    break;
                case Facing.Direction.West:
                    if ( clampedVec.x < 0 ) return Vector2.left;
                    break;
            }

            return Vector2.zero;
        } else if ( m_clampMode == ClampMode.Horizontal ) {
            if ( clampedVec.x > 0 ) return Facing.DirectionToVector( Facing.Direction.East );
            else if( clampedVec.x < 0 ) return Facing.DirectionToVector( Facing.Direction.West );
            return Vector2.zero;
        } else if ( m_clampMode == ClampMode.Vertical ) {
            if ( clampedVec.y > 0 ) return Facing.DirectionToVector( Facing.Direction.North );
            else if( clampedVec.y < 0 ) return Facing.DirectionToVector( Facing.Direction.South );
            return Vector2.zero;
        } else if ( m_clampMode == ClampMode.Cardinal ) {
            var dir = clampedVec.ToDirection( true );
            return Facing.DirectionToVector( dir );
        } else if( m_clampMode == ClampMode.PrimaryIntercardinal) {
            var dir = clampedVec.ToDirection();
            return Facing.DirectionToVector( dir );
        }
        return clampedVec;
    }
}
