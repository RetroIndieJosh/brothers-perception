using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortSpriteByAxis : MonoBehaviour {
    public enum Axis { X, Y, Z }
    public Axis sortingAxis = Axis.Y;

    private int m_baseSortingOrder;
    private SpriteRenderer m_renderer;

	private void Awake() {
        m_renderer = GetComponent<SpriteRenderer>();
        m_baseSortingOrder = m_renderer.sortingOrder;
	}

    private void Update() {
        updateSortingOrder();
    }

    private void updateSortingOrder() {
        switch(sortingAxis) {
            case Axis.X:
                m_renderer.sortingOrder = m_baseSortingOrder
                    + Mathf.FloorToInt( transform.position.x * 16 );
                break;
            case Axis.Y:
                m_renderer.sortingOrder = m_baseSortingOrder
                    + Mathf.FloorToInt( transform.position.y * 16 );
                break;
            case Axis.Z:
                m_renderer.sortingOrder = m_baseSortingOrder
                    + Mathf.FloorToInt( transform.position.z * 16 );
                break;
        }

    }
}
