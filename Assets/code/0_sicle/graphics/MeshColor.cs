using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class MeshColor : MonoBehaviour {
    [SerializeField]
    private Color m_color;

	void Awake() {
        GetComponent<MeshRenderer>().material.color = m_color;
	}
}
