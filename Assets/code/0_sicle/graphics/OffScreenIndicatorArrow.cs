using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffScreenIndicatorArrow : MonoBehaviour {
    public Color Color = Color.black;

    [SerializeField]
    private LineRenderer m_lineRendererMain = null;

    [SerializeField]
    private LineRenderer m_lineRendererTop = null;

    [SerializeField]
    private LineRenderer m_lineRendererBottom = null;

    [SerializeField]
    private float m_lineLength = 1.0f;

    [SerializeField]
    private float m_lineWidth = 0.1f;

    private bool m_isVisible = false;

	void Start () {
        if( m_lineRendererBottom == null || m_lineRendererMain == null || m_lineRendererBottom == null ) {
            Debug.LogErrorFormat("Must set line renderers for {0}. Destroying.", name );
            Destroy( gameObject );
            return;
        }

        if( m_lineRendererBottom == m_lineRendererMain || m_lineRendererBottom == m_lineRendererTop
            || m_lineRendererMain == m_lineRendererTop ) {

            Debug.LogErrorFormat( "Line renderers in {0} must be unique.", name );
            Destroy( gameObject );
            return;
        }

        m_lineRendererBottom.startWidth = m_lineRendererBottom.endWidth =
        m_lineRendererMain.startWidth = m_lineRendererMain.endWidth =
        m_lineRendererTop.startWidth = m_lineRendererTop.endWidth = m_lineWidth;

        var offScreenTrigger = GetComponent<OffScreenTrigger>();
        if ( offScreenTrigger == null ) offScreenTrigger = gameObject.AddComponent<OffScreenTrigger>();
        offScreenTrigger.OnEnterScreen.AddListener( HideArrow );
        offScreenTrigger.OnExitScreen.AddListener( ShowArrow );
	}

    public void HideArrow() {
        m_lineRendererBottom.positionCount = 0;
        m_lineRendererMain.positionCount = 0;
        m_lineRendererTop.positionCount = 0;

        m_isVisible = false;
    }

    public void ShowArrow() {
        var diff = transform.position - Camera.main.transform.position;

        m_lineRendererBottom.startColor = m_lineRendererBottom.endColor = Color;
        m_lineRendererMain.startColor = m_lineRendererMain.endColor = Color;
        m_lineRendererTop.startColor = m_lineRendererTop.endColor = Color;

        m_isVisible = true;
        RecalculatePoints();
    }

    private void OnDrawGizmos() {
        if ( !m_isVisible ) return;

        Gizmos.color = Color;
        var pos = GetPointOfIntersectionWithCameraEdge();
        Gizmos.DrawWireSphere( pos, 0.5f );
    }

    private void Update() {
        if ( !m_isVisible ) return;
        RecalculatePoints();
    }

    private Vector2 GetPointOfIntersectionWithCameraEdge() {
        if ( !m_isVisible ) return Vector2.zero;

        var start = m_lineRendererMain.GetPosition( 0 );
        var end = m_lineRendererMain.GetPosition( 1 );
        return CameraManager.instance.Rectangle.EdgeIntersectPoint( start, end );
    }

    private void RecalculatePoints() {
        var start = (Vector2)Camera.main.transform.position;
        var end = CameraManager.instance.Rectangle.EdgeIntersectPoint( start, transform.position );

        var dirVec = end - start;
        var arrowStart = end - dirVec.normalized * m_lineLength;

        var posList = new Vector3[2];

        posList[0] = arrowStart;
        posList[1] = end;
        m_lineRendererMain.positionCount = posList.Length;
        m_lineRendererMain.SetPositions( posList );

        var angle = 30.0f;
        var lengthMult = 0.1f;

        posList[0] = end;
        posList[1] = end + dirVec.Rotate( 180.0f + angle ) * lengthMult;
        m_lineRendererTop.positionCount = 2;
        m_lineRendererTop.SetPositions( posList );

        posList[0] = end;
        posList[1] = end + dirVec.Rotate( 180.0f - angle ) * lengthMult;
        m_lineRendererBottom.positionCount = 2;
        m_lineRendererBottom.SetPositions( posList );
    }
}
