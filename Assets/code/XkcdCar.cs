using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XkcdCar : MonoBehaviour {
    [SerializeField]
    Vector2 m_velocity = Vector2.left;

    [SerializeField]
    float m_maxDistance = 10.0f;

    private Vector2 m_startPos = Vector2.zero;

    private void OnDrawGizmos() {
        Utility.GizmoArrow( transform.position, m_velocity.normalized * m_maxDistance, 15.0f, 0.1f );
    }

    private void Start() {
        m_startPos = transform.position;
        GetComponent<Rigidbody2D>().velocity = m_velocity;
    }

    private void Update() {
        if ( Vector2.Distance( transform.position, m_startPos ) > m_maxDistance ) transform.position = m_startPos;
    }
}
