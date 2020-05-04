using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeDisplay : MonoBehaviour {
    private TextMeshPro m_textMesh = null;

    private void Awake() {
        m_textMesh = GetComponent<TextMeshPro>();
    }

    void Update () {
        m_textMesh.text = TimeManager.instance.TimeString;
	}
}
