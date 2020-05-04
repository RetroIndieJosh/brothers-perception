using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( Mover ) )]
public class AiBehaviour : MonoBehaviour
{
    [System.Serializable]
    public enum Behavior
    {
        Avoid,
        Flee,
        Follow,
        Seek
    }

    [System.Serializable]
    public enum TargetType
    {
        GameObject,
        LayerMask,
        Player
    }

    [SerializeField]
    private Behavior m_behavior = Behavior.Avoid;

    [SerializeField]
    private TargetType m_targetType = TargetType.GameObject;

    public GameObject Target;

    [SerializeField]
    private LayerMask m_targetLayerMask;

    // TODO hide for seek and flee
    [SerializeField]
    private float m_minDistance = 1.0f;

    private Mover m_mover = null;

    public void OnSwitchedToAiBehaviour() {
        Debug.LogFormat( "Activate {0} AI Behavior", name );
    }

    public void OnSwitchedFromAiBehaviour() {
        Debug.LogFormat( "Deactivate {0} AI Behavior", name );
    }

    private void Awake() {
        if ( m_targetType == TargetType.Player ) Target = GameObject.FindGameObjectWithTag( "Player" );
        m_mover = GetComponent<Mover>();
    }

    private void Update() {
        if ( m_targetType == TargetType.LayerMask ) Target = GetTargetForLayerMask();
        if ( Target == null ) {
            Debug.LogErrorFormat( "{0} has invalid target settings", gameObject );
            return;
        }

        switch ( m_behavior ) {
            case Behavior.Avoid: Avoid(); break;
            case Behavior.Flee: Flee(); break;
            case Behavior.Follow: Follow(); break;
            case Behavior.Seek: Seek(); break;
        }
    }

    private void Avoid() {
        var diff = Target.transform.position - transform.position;
        if ( diff.magnitude <= m_minDistance ) Flee();
        else m_mover.Stop();
    }

    private void Flee() {
        var diff = Target.transform.position - transform.position;

        m_mover.SetDirection( -diff.normalized );
    }

    private void Follow() {
        var diff = Target.transform.position - transform.position;
        if ( diff.magnitude >= m_minDistance ) Seek();
        else m_mover.Stop();
    }

    private void Seek() {
        var diff = Target.transform.position - transform.position;
        m_mover.SetDirection( diff.normalized );
    }

    private GameObject GetTargetForLayerMask() {
        GameObject target = null;
        var closestDistance = Mathf.Infinity;
        foreach ( var collider in FindObjectsOfType<Collider2D>() ) {
            if ( !m_targetLayerMask.ContainsLayer( collider.gameObject.layer ) ) continue;

            var distance = ( collider.transform.position - transform.position ).magnitude;
            if ( distance < closestDistance ) {
                target = collider.gameObject;
                closestDistance = distance;
            }
        }
        return target;
    }
}
