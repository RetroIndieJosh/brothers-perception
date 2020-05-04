using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeechBubble : MonoBehaviour {
    [SerializeField]
    private LineRenderer m_lineRenderer = null;

    [SerializeField]
    private TextMeshPro m_textMesh = null;

    [SerializeField]
    [Range(0.0f, 0.5f)]
    private float m_fromPlayerOffsetPercent = 0.1f;

    [SerializeField]
    [Range(0.0f, 0.5f)]
    private float m_fromTextOffsetPercent = 0.1f;

    [SerializeField]
    private Vector2 m_lineLength = Vector2.one;

    [SerializeField]
    private Color m_color = Color.black;

    [SerializeField]
    private float m_startWidth = 0.1f;

    [SerializeField]
    private float m_endWidthMult = 2.0f;

    public Color Color {
        set {
            m_color = value;
            if ( IsVisible ) SetColor( m_color );
        }
    }

    public bool IsVisible { get; private set; }

    public void Clear() {
        IsVisible = false;
        m_lineRenderer.positionCount = 0;
        m_lineRenderer.SetPositions( new Vector3[0] );
        SetColor( Color.clear );
    }

    public void Display( string a_message, float a_delayTimeSec = 0.0f, float a_stayTimeSec = 0.0f ) {
        if( a_stayTimeSec > 0.0f ) StartCoroutine( DelayClear( a_delayTimeSec + a_stayTimeSec ) );

        if ( a_delayTimeSec > 0.0f ) {
            StartCoroutine( DelayDisplay( a_message, a_delayTimeSec ) );
            return;
        }

        IsVisible = true;
        m_textMesh.text = a_message;
        SetColor( m_color );
    }

    private void Start() {
        Clear();

        if( m_lineRenderer == null || m_textMesh == null ) {
            Debug.LogError( "Speech Bubble '{0}' requires Line Renderer and Text Mesh. Destroying." );
            Destroy( gameObject );
            return;
        }

        m_lineRenderer.startWidth = m_startWidth;
        m_lineRenderer.endWidth = m_startWidth * m_endWidthMult;
    }

    private void Update() {
        if ( !IsVisible ) return;

        var positions = new Vector3[2];
        if ( CameraManager.instance.Rectangle.Contains( transform.position ) ) {
            var distance = Vector2.Distance( Camera.main.transform.position, transform.position );

            var diff = Camera.main.transform.position - transform.position;
            var x = distance > 4.0f ? Mathf.Sign( diff.x ) : 1.0f;
            var y = distance > 4.0f ? Mathf.Sign( diff.y ) : 1.0f;
            var lineVec = new Vector3( m_lineLength.x * x, m_lineLength.y * y );

            positions[0] = transform.position + lineVec * m_fromPlayerOffsetPercent;
            positions[1] = transform.position + lineVec * ( 1.0f - m_fromTextOffsetPercent );
            m_textMesh.transform.position = transform.position + lineVec;
        } else {
            positions[0] = CameraManager.instance.Rectangle.EdgeIntersectPoint( Camera.main.transform.position, 
                transform.position );
            var vecToPlayer = ( Camera.main.transform.position - transform.position ).normalized;
            positions[1] = positions[0] + vecToPlayer * m_textMesh.renderedHeight * 2.0f;

            var textOffset = new Vector2( vecToPlayer.x * m_textMesh.renderedWidth, 
                vecToPlayer.y * m_textMesh.renderedHeight );

            var xVec = Mathf.Sign( vecToPlayer.x ) * Vector3.right * m_textMesh.renderedWidth / 2.0f;
            var yVec = Mathf.Sign( vecToPlayer.y ) * Vector3.up * m_textMesh.renderedHeight / 2.0f;
            m_textMesh.transform.position = positions[1] + xVec + yVec;
        }

        m_lineRenderer.positionCount = 2;
        m_lineRenderer.SetPositions( positions );
    }
    
    private IEnumerator DelayDisplay(string a_message, float a_delaySec) {
        var timeElapsed = 0.0f;
        while( timeElapsed < a_delaySec ) {
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        Display( a_message );
    }

    private IEnumerator DelayClear( float a_delaySec ) {
        var timeElapsed = 0.0f;
        while( timeElapsed < a_delaySec ) {
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        Clear();
    }

    private void SetColor(Color a_color) {
        m_textMesh.color = a_color;
        m_lineRenderer.startColor = m_lineRenderer.endColor = a_color;
    }
}
