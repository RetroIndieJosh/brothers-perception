using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour {
    [SerializeField]
    private Health m_targetHealth;

    [SerializeField]
    private float m_damageMult = 1.0f;

    public float DamageMult {  get { return m_damageMult; } }
    public Health TargetHealth {  get { return m_targetHealth; } }
}
