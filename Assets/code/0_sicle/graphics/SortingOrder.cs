using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SortingOrder : MonoBehaviour {
    public int sortingOrder = 0;
    public string sortingName = "Default";

    private MeshRenderer m_renderer;

    private void Awake() {
        m_renderer = GetComponent<MeshRenderer>();
    }

    private void Start() {
        m_renderer.sortingLayerName = sortingName;
        m_renderer.sortingOrder = sortingOrder;
    }
}
