using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class InfiniteScrollActor : MonoBehaviour {
    [SerializeField]
    private InfiniteScroll m_referenceScroller = null;

    private Rigidbody2D m_body = null;

    private void Awake() {
        m_body = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        m_body.velocity = (Vector3)m_referenceScroller.MoveVector;
    }
}
