using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteColorLerp : MonoBehaviour {
    public Color StartColor = Color.white;
    public Color EndColor = Color.clear;
    public Counter LerpCounter;

    [SerializeField]
    private SpriteRenderer m_spriteRenderer = null;

    private void Awake() {
        if( m_spriteRenderer == null ) m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update() {
        if ( LerpCounter == null ) {
            m_spriteRenderer.color = StartColor;
            return;
        }

        var t = LerpCounter.Count / LerpCounter.Maximum;
        m_spriteRenderer.color = Color.Lerp( StartColor, EndColor, t );
    }
}
