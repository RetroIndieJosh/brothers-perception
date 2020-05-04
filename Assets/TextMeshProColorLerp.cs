using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextMeshProColorLerp : MonoBehaviour {
    public Color StartColor = Color.white;
    public Color EndColor = Color.clear;
    public Counter LerpCounter;

    [SerializeField]
    private TextMeshPro m_textMeshPro = null;

    private void Awake() {
        if ( m_textMeshPro == null ) m_textMeshPro = GetComponent<TextMeshPro>();
    }

    private void Update() {
        if ( LerpCounter == null ) {
            m_textMeshPro.color = StartColor;
            return;
        }

        var t = LerpCounter.Count / LerpCounter.Maximum;
        m_textMeshPro.color = Color.Lerp( StartColor, EndColor, t );
    }
}
