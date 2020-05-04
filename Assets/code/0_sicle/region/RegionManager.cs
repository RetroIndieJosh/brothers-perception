using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class RegionManager : MonoBehaviour {
    static public RegionManager instance = null;

    public Region startRegion;
    public Tilemap floorTilemap;
    public Vector3Int maxWorldTileExtent = new Vector3Int(100, 100, 1);

    [Header( "Transition" )]

    [SerializeField]
    private Color m_fadeColor = Color.black;

    [SerializeField]
    private float m_transitionHoldPercent = 0.3f;

    [SerializeField]
    private float m_transitionTime = 1.0f;

    [SerializeField]
    private SpriteRenderer m_fadeScreenSprite = null;

    public UnityEvent OnRegionLoaded = new UnityEvent();

    public bool IsTransitioning { get; private set; }

    public Region CurRegion {
        get { return m_curRegion; }
        set { StartCoroutine( Transition( value ) ); }
    }

    private Region m_curRegion = null;

    private void Awake() {
        if( instance != null ) {
            Destroy( gameObject );
            return;
        }

        instance = this;

        if ( floorTilemap != null ) {
            floorTilemap.origin = maxWorldTileExtent * -1;
            floorTilemap.size = maxWorldTileExtent;
            floorTilemap.ResizeBounds();
        }

        IsTransitioning = false;
    }

    private void Start() {
        SetRegion( startRegion );
    }

    private void ClearRegions() {
        // unload all but the main scene
        for( var i = 1; i < SceneManager.sceneCount; ++i) {
            SceneManager.UnloadSceneAsync( SceneManager.GetSceneAt( i ) );
        }
    }

    private void Fill(Tile a_tile) {
        if ( floorTilemap == null ) return;

        floorTilemap.ClearAllTiles();
        for( var x = -CurRegion.Extents.x; x < CurRegion.Extents.x; ++x ) {
            for ( var y = -CurRegion.Extents.y; y < CurRegion.Extents.y; ++y ) {
                floorTilemap.SetTile( new Vector3Int( x, y, 0 ), a_tile );
            }
        }
    }

    // immediately set the region - use this for region stuff including the initial region
    // NOTE this skips unloading previous region - use with caution!
    private void SetRegion(Region a_region) {
        ClearRegions();

        Debug.Log( "New region: " + a_region );
        m_curRegion = a_region;
        if ( m_curRegion != null ) {
            Fill( CurRegion.FloorTile );
            CurRegion.LoadObjects();
            CameraManager.instance.OnRegionChanged();
        }

        a_region.CreateWalls();

        StartCoroutine( FinishLoad() );
        Debug.Log( "region loaded" );
    }

    // the scene isn't fully loaded until the next frame so we have to wait a bit before triggering event
    private IEnumerator FinishLoad() {
        yield return new WaitForSeconds( 0.1f );

        // emulate long loading time
        //yield return new WaitForSeconds( 1.0f );

        OnRegionLoaded.Invoke();
    }

    private IEnumerator Transition(Region a_newRegion) {
        IsTransitioning = true;

        if( CurRegion != null ) CurRegion.DestroyWalls();

        var cam_height = Camera.main.orthographicSize * 2;
        var cam_width = cam_height * Camera.main.aspect;

        var region_width = a_newRegion.Extents.x;
        var region_height = a_newRegion.Extents.y;

        var width = Mathf.Max( cam_width, region_width );
        var height = Mathf.Max( cam_height, region_height );

        m_fadeScreenSprite.size = new Vector2( width, height );

        var x = Camera.main.transform.position.x;
        var y = Camera.main.transform.position.y;
        var z = -5;
        m_fadeScreenSprite.transform.position = new Vector3( x, y, z );

        // fade out
        var fadeTime 
            = m_transitionTime * ( 1.0f - m_transitionHoldPercent / 2 );
        var timeElapsed = 0.0f;
        while( timeElapsed < fadeTime ) {
            timeElapsed += Time.deltaTime;
            var percent = timeElapsed / fadeTime;
            m_fadeColor.a = percent;
            m_fadeScreenSprite.color = m_fadeColor;
            yield return null;
        }

        if ( CurRegion != null ) CurRegion.UnloadObjects();
        SetRegion( a_newRegion );
        yield return new WaitForSeconds( 
            m_transitionTime * m_transitionHoldPercent );

        // fade in
        timeElapsed = 0.0f;
        while( timeElapsed < fadeTime ) {
            timeElapsed += Time.deltaTime;
            var percent = timeElapsed / fadeTime;
            m_fadeColor.a = 1.0f - percent;
            m_fadeScreenSprite.color = m_fadeColor;
            yield return null;
        }

        IsTransitioning = false;
    }
}
