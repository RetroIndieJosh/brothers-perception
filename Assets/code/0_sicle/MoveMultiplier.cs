using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MoveMultiplier : MonoBehaviour {
    [SerializeField]
    private float m_speedMultiplier = 0.5f;

    private void OnTriggerEnter2D( Collider2D collision ) {
        var target = collision.gameObject.GetComponent<Mover>();
        if ( target == null ) return;
        target.SpeedMultiplier = m_speedMultiplier;
    }

    private void OnTriggerExit2D( Collider2D collision ) {
        var target = collision.gameObject.GetComponent<Mover>();
        if ( target == null ) return;
        target.SpeedMultiplier = 1.0f;
    }

    private void Awake() {
        GetComponent<Collider2D>().isTrigger = true;
    }
}
