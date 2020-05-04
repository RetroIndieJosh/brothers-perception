using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RegionChanger : MonoBehaviour {
    [SerializeField]
    private Region m_region;

    [SerializeField]
    private RegionChanger m_target;

    [SerializeField]
    private Vector2 m_entryOffset = Vector2.zero;

    private void Awake() {
        if ( m_region == null ) {
            if ( transform.parent == null ) {
                Debug.LogErrorFormat( "Region changer {0} has neither a parent nor assigned region.", gameObject );
                Destroy( gameObject );
                return;
            }
            m_region = transform.parent.GetComponent<Region>();
            if ( transform.parent == null ) {
                Debug.LogErrorFormat( "Region changer {0} has neither a parent nor assigned region.", gameObject );
                Destroy( gameObject );
                return;
            }
        }

        if( m_target == null ) {
            Debug.LogErrorFormat( "Region changer in {0} has no target.", gameObject );
            Destroy( gameObject );
            return;
        }

        if ( m_region == null ) {
            Debug.LogFormat( "Removing ineffective region changer {0}", gameObject );
            Destroy( gameObject );
            return;
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere( (Vector2)transform.position + m_entryOffset, 1.0f );
    }

    private void OnTriggerEnter2D( Collider2D collider ) {
        var player = collider.gameObject.GetComponent<PlayerController>();
        if ( player == null || RegionManager.instance.IsTransitioning || player.IsHoldingItem ) return;

        var pos = player.transform.position;
        var targetPos = (Vector2)m_target.transform.position + m_target.m_entryOffset;
        player.transform.position = new Vector3( targetPos.x, targetPos.y, pos.z );

        RegionManager.instance.CurRegion = m_target.m_region;
    }

    private void Start() {
        GetComponent<Collider2D>().isTrigger = true;
    }
}
