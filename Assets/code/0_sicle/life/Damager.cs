using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( Collider2D ) )]
public class Damager : MonoBehaviour
{
    [System.Serializable]
    private enum DamageType
    {
        Continuous,
        Once,
        OnceDestroy
    }

    [SerializeField]
    [Tooltip("Continuous: Damage applied every frame\n\n"
        + "Once: Apply damage each time we collide\n\n"
        + "OnceDestroy: Apply damage on collision, then destroy this"
    )]
    private DamageType m_damageType = DamageType.OnceDestroy;

    [SerializeField]
    [Tooltip("For Continuous, this is damage per second")]
    float m_damage = 1;

    [SerializeField]
    LayerMask m_affectedLayer;

    private GameObject m_target = null;

    private void Awake() {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Update() {
        if ( m_target == null ) return;
        ApplyDamage( m_target, Time.deltaTime );
    }

    private void OnTriggerEnter2D( Collider2D collision ) {
        if ( m_affectedLayer.value 
            != ( m_affectedLayer.value | ( 1 << collision.gameObject.layer ) ) )
            return;

        if ( m_damageType == DamageType.Continuous ) {
            m_target = collision.gameObject;
            return;
        }

        ApplyDamage( collision.gameObject );
        if ( m_damageType == DamageType.OnceDestroy ) Destroy( gameObject );
    }

    private void OnTriggerExit2D( Collider2D collision ) {
        m_target = null;
    }

    private void ApplyDamage( GameObject a_target, float m_multiplier = 1.0f ) {
        var health = a_target.GetComponent<Health>();
        if ( health == null ) {
            var hitbox = a_target.GetComponent<HitBox>();
            if ( hitbox == null ) return;
            health = hitbox.TargetHealth;
            m_multiplier *= hitbox.DamageMult;
        }
        health.ApplyDamage( m_damage * m_multiplier );
    }
}
