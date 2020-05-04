using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Facing))]
[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(SpriteAnimator))]
public class MoveAnimator : MonoBehaviour {
    [System.Serializable]
    private enum HorizontalDupeMode
    {
        FlipEastFromWest,
        FlipWestFromEast,
        NoFlip
    }

    [System.Serializable]
    private enum VerticalDupeMode
    {
        FlipNorthFromSouth,
        FlipSouthFromNorth,
        NoFlip
    }

    [SerializeField]
    private int m_framerate;

    [SerializeField]
    private bool m_useIntercardinal = false;

    [Header("Duplication Options")]

    [SerializeField]
    private HorizontalDupeMode m_horizontalDupeMode = HorizontalDupeMode.NoFlip;

    [SerializeField]
    private VerticalDupeMode m_verticalDupeMode = VerticalDupeMode.NoFlip;

    [Header("Cardinal Horizontal")]

    [SerializeField]
    private List<Sprite> m_animationEast;

    [SerializeField]
    private List<Sprite> m_animationWest;

    [Header("Cardinal Vertical")]

    [SerializeField]
    private List<Sprite> m_animationNorth;

    [SerializeField]
    private List<Sprite> m_animationSouth;

    [Header("Primary Intercardinal Directions")]

    [SerializeField]
    private List<Sprite> m_animationNorthEast;

    [SerializeField]
    private List<Sprite> m_animationNorthWest;

    [SerializeField]
    private List<Sprite> m_animationSouthEast;

    [SerializeField]
    private List<Sprite> m_animationSouthWest;

    private SpriteAnimator m_animator;
    private Facing m_facing;
    private Facing.Direction m_prevFacing = Facing.Direction.East;
    private Mover m_mover;

    private void Awake() {
        m_facing = GetComponent<Facing>();
        m_animator = GetComponent<SpriteAnimator>();
        m_mover = GetComponent<Mover>();

        if ( m_horizontalDupeMode == HorizontalDupeMode.FlipEastFromWest ) {
            m_animationEast = m_animationWest;
            m_animationNorthEast = m_animationNorthWest;
            m_animationSouthEast = m_animationSouthWest;
        } else if ( m_horizontalDupeMode == HorizontalDupeMode.FlipEastFromWest ) {
            m_animationWest = m_animationEast;
            m_animationNorthWest = m_animationNorthEast;
            m_animationSouthWest = m_animationSouthEast;
        }

        if ( m_verticalDupeMode == VerticalDupeMode.FlipNorthFromSouth ) {
            m_animationNorth = m_animationSouth;
            m_animationNorthEast = m_animationSouthEast;
            m_animationNorthWest = m_animationSouthWest;
        } else if ( m_verticalDupeMode == VerticalDupeMode.FlipSouthFromNorth ) {
            m_animationSouth = m_animationNorth;
            m_animationSouthEast = m_animationNorthEast;
            m_animationSouthWest = m_animationNorthWest;
        }

        if( m_animationEast.Count == 0 ) {
            Debug.LogErrorFormat( "{0} has no move east animation", gameObject );                                      
        }

        if( m_animationNorth.Count == 0 ) {
            Debug.LogErrorFormat( "{0} has no move north animation", gameObject );
        }

        if( m_animationSouth.Count == 0 ) {
            Debug.LogErrorFormat( "{0} has no move south animation", gameObject );
        }

        if( m_animationWest.Count == 0 ) {
            Debug.LogErrorFormat( "{0} has no move west animation", gameObject );
        }

        var flipVecHorizontal = new Vector2( -1.0f, 1.0f );
        var flipVecVertical = new Vector2( 1.0f, -1.0f );

        var east = new SpriteAnimator.SpriteAnimation() {
            name = Facing.Direction.East.ToString(),
            flipVector 
                = m_horizontalDupeMode == HorizontalDupeMode.FlipEastFromWest ? flipVecHorizontal : Vector2.one,
            spriteList = m_animationEast,
            framerate = m_framerate
        };
        m_animator.AddAnimation( east );

        var north = new SpriteAnimator.SpriteAnimation() {
            name = Facing.Direction.North.ToString(),
            spriteList = m_animationNorth,
            flipVector = m_verticalDupeMode == VerticalDupeMode.FlipNorthFromSouth ? flipVecVertical : Vector2.one,
            framerate = m_framerate
        };
        m_animator.AddAnimation( north );

        var south = new SpriteAnimator.SpriteAnimation() {
            name = Facing.Direction.South.ToString(),
            spriteList = m_animationSouth,
            flipVector = m_verticalDupeMode == VerticalDupeMode.FlipSouthFromNorth ? flipVecVertical : Vector2.one,
            framerate = m_framerate
        };
        m_animator.AddAnimation( south );

        var west = new SpriteAnimator.SpriteAnimation() {
            name = Facing.Direction.West.ToString(),
            spriteList = m_animationWest,
            flipVector 
                = m_horizontalDupeMode == HorizontalDupeMode.FlipWestFromEast ? flipVecHorizontal : Vector2.one,
            framerate = m_framerate
        };
        m_animator.AddAnimation( west );

        UpdateAnimation();
    }

    private void Update() {
        if ( m_facing.direction == m_prevFacing && m_mover.IsMoving == true ) return;
        UpdateAnimation();
    }

    private void UpdateAnimation() {
        m_prevFacing = m_facing.direction;

        var directionString = m_facing.direction.ToString();

        // if we're sticking to cardinal and not N or S, use horizontal component
        if ( !m_useIntercardinal && !Facing.IsVertical( m_facing.direction ) ) {
            directionString = Facing.GetHorizontal( m_facing.direction ).ToString();
        }

        m_animator.SetAnimation( directionString );
        m_animator.IsPaused = !m_mover.IsMoving;
    }
}
