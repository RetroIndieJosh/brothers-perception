using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereGizmo : MonoBehaviour {
    [SerializeField]
    Color m_color = Color.white;

    [SerializeField]
    float m_radius = 0.5f;

    [SerializeField]
    bool m_wire = true;

    [SerializeField]
    [Tooltip("If false, only draws when selected")]
    bool m_alwaysDraw = false;

    private void OnDrawGizmos() {
        if ( !m_alwaysDraw ) return;
        Draw();
    }

    private void OnDrawGizmosSelected() {
        if ( m_alwaysDraw ) return;
        Draw();
    }

    private void Draw() {
        Gizmos.color = m_color;
        if ( m_wire ) Gizmos.DrawWireSphere( transform.position, m_radius );
        else Gizmos.DrawSphere( transform.position, m_radius );
    }
}
