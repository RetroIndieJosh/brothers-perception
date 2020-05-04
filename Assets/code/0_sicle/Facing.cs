using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Facing : MonoBehaviour {
    public enum Direction
    {
        East,
        North,
        Northeast,
        Northwest,
        South,
        Southeast,
        Southwest,
        West,
        None
    }

    static public Vector2 DirectionToVector( Direction a_direction ) {
        var vec = Vector2.zero;
        if ( a_direction == Direction.East || a_direction == Direction.Northeast 
            || a_direction == Direction.Southeast ) vec += Vector2.right;
        if ( a_direction == Direction.North || a_direction == Direction.Northeast 
            || a_direction == Direction.Northwest ) vec += Vector2.up;
        if ( a_direction == Direction.South || a_direction == Direction.Southeast 
            || a_direction == Direction.Southwest ) vec += Vector2.down;
        if ( a_direction == Direction.West || a_direction == Direction.Northwest 
            || a_direction == Direction.Southwest ) vec += Vector2.left;
        return vec;
    }

    static public Direction GetHorizontal(Direction a_direction ) {
        switch ( a_direction ) {
            case Direction.West: 
            case Direction.Northwest: 
            case Direction.Southwest: return Direction.West;

            case Direction.East:
            case Direction.Northeast: 
            case Direction.Southeast: return Direction.East;
            default: return Direction.None;
        }
    }

    static public Direction GetVertical(Direction a_direction ) {
        switch ( a_direction ) {
            case Direction.North: 
            case Direction.Northeast: 
            case Direction.Northwest: return Direction.North;

            case Direction.South:
            case Direction.Southeast: 
            case Direction.Southwest: return Direction.South;
            default: return Direction.None;
        }
    }

    static public bool IsVertical(Direction a_direction) {
        return a_direction == Direction.North || a_direction == Direction.South;
    }

    static public Direction Opposite(Direction a_direction ) {
        switch ( a_direction ) {
            case Direction.East: return Direction.West;
            case Direction.North: return Direction.South;
            case Direction.Northeast: return Direction.Southwest;
            case Direction.Northwest: return Direction.Southeast;
            case Direction.South: return Direction.North;
            case Direction.Southeast: return Direction.Northwest;
            case Direction.Southwest: return Direction.Northeast;
            case Direction.West: return Direction.East;
            default: return Direction.East;
        }
    }

    public Direction direction = Direction.South;

    static public Direction VectorToDirection( Vector2 a_vector ) {
        if ( a_vector.x > 0 ) {
            if ( a_vector.y > 0 ) return Direction.Northeast;
            else if ( a_vector.y < 0 ) return Direction.Southeast;
            else return Direction.East;
        } else if ( a_vector.x < 0 ) {
            if ( a_vector.y > 0 ) return Direction.Northwest;
            else if ( a_vector.y < 0 ) return Direction.Southwest;
            else return Direction.West;
        } else if ( a_vector.y > 0 ) return Direction.North;
        return Direction.South;
    }

    static public Direction VectorToDirectionCardinal( Vector2 a_vector ) {
        if ( Mathf.Abs( a_vector.x ) >= Mathf.Abs( a_vector.y ) ) {
            if ( a_vector.x > 0 ) return Direction.East;
            else return Direction.West;
        }
        if ( a_vector.y > 0 ) return Direction.North;
        return Direction.South;
    }

    public Vector3 DirectionVector {
        get { return DirectionToVector( direction ); }
        set { direction = VectorToDirection( value ); }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Utility.GizmoArrow( transform.position, DirectionVector );
    }
}
