using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour {
    [SerializeField]
    float m_seconds = 1.0f;

	void Start () {
        Destroy( gameObject, m_seconds );
	}
}
