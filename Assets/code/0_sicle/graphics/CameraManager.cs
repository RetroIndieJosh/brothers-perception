using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor( typeof( CameraManager ) )]
public class CameraManagerEditor : Editor
{
    public override void OnInspectorGUI() {
        serializedObject.Update();

        bool followPlayer = serializedObject.FindProperty( "m_followPlayer" ).boolValue;
        bool useRegionOptions = serializedObject.FindProperty( "UseRegionOptions" ).boolValue;

        var propIter = serializedObject.GetIterator();
        if ( propIter.NextVisible( true ) ) {
            do {
                var prop = serializedObject.FindProperty( propIter.name );
                
                if ( !useRegionOptions && prop.name != "UseRegionOptions" && prop.name.Contains( "Region" ) )
                    continue;
                if( followPlayer && prop.name == "m_followTarget" ) continue;

                EditorGUILayout.PropertyField( prop, true );
            } while ( propIter.NextVisible( false ) );
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif //UNITY_EDITOR

public class CameraManager : MonoBehaviour {
    static public CameraManager instance = null;

#if UNITY_EDITOR
    [SerializeField]
    public bool UseRegionOptions = false;
#endif

    [Header( "Following" )]

    [SerializeField]
    private float m_maxVelocity = 10.0f;

    [SerializeField]
    private float m_dampTime = 0.15f;

    [SerializeField]
    private bool m_followPlayer = false;

    public Transform FollowTarget;

    [SerializeField]
    private bool m_clampAtRegionEdges = true;

    [Header("Sizing")]

    public int maxSize = 10;
    public int minSize = 5;

    [SerializeField]
    private bool m_sizeToRegion = true;

    private float m_regionEdgeOverlap = 1.0f;

    private float m_width;
    private float m_height;

    private float m_xMin;
    private float m_xMax;
    private float m_yMin;
    private float m_yMax;

    public Vector2 Center {
        get { return new Vector2( transform.position.x - m_width / 2, transform.position.y - m_height / 2 ); }
    }

    public Rect Rectangle { get { return new Rect( Center, Size ); } }

    public Vector2 Size { get { return new Vector2( m_width, m_height ); } }

    private Vector2 m_velocity;

    // call whenever the region size changes
    // TODO move this into the region manager's OnLoadedRegion
    public void OnRegionChanged() {
        Resize();
        RecalculateBounds();
        if ( FollowTarget != null ) transform.position = FollowTarget.transform.position;
    }

    private void Resize() {
        if ( RegionManager.instance == null || RegionManager.instance.CurRegion == null || !m_sizeToRegion ) {
            m_height = Camera.main.orthographicSize * 2;
        } else {
            var regionHeight = RegionManager.instance.CurRegion.Size.y;
            m_height = Mathf.Clamp( regionHeight, minSize, maxSize );
            Camera.main.orthographicSize = m_height / 2;
        }

        m_width = m_height * Camera.main.aspect;
    }

    private void RecalculateBounds() {
        var region = RegionManager.instance.CurRegion;
        if ( region == null ) return;

        m_xMin = region.Left + m_width / 2 - region.AllowBeyondExtensMin.x;
        m_xMax = region.Right - m_width / 2 + region.AllowBeyondExtensMax.x;
        if( m_xMin >= m_xMax ) m_xMin = m_xMax = 0;

        m_yMin = region.Bottom + m_height / 2 - region.AllowBeyondExtensMin.y;
        m_yMax = region.Top - m_height / 2 + region.AllowBeyondExtensMax.y;
        if( m_yMin >= m_yMax ) m_yMin = m_yMax = 0;
    }

    private void Awake() {
        if( instance != null ) {
            Destroy( gameObject );
            return;
        }

        instance = this;
        m_height = Camera.main.orthographicSize;
        m_width = m_height * Camera.main.aspect;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube( Rectangle.center, Rectangle.size );
    }

    private void Start() {
        Resize();
    }

    private void Update () {
        if ( FollowTarget == null && m_followPlayer ) {
            var player = GameObject.FindGameObjectWithTag( "Player" );
            if ( player != null ) FollowTarget = player.transform;
        }

        if ( FollowTarget == null ) return;
        if ( DialogueManager.instance != null && DialogueManager.instance.IsShowing ) return;
        if ( RegionManager.instance != null && RegionManager.instance.IsTransitioning ) return;

        var delta = FollowTarget.transform.position - transform.position;
        delta.z = 0;

        transform.position = Vector2.SmoothDamp( transform.position, 
            FollowTarget.transform.position, ref m_velocity, m_dampTime, 
            m_maxVelocity, Time.deltaTime );

        if( m_clampAtRegionEdges) {
            var pos = transform.position;

            pos.x = Mathf.Clamp( pos.x, m_xMin, m_xMax );
            pos.y = Mathf.Clamp( pos.y, m_yMin, m_yMax );

            transform.position = pos;
        }

        {
            var pos = transform.position;
            pos.z = -10;
            transform.position = pos;
        }
	}
}
