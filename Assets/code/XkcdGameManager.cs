using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor( typeof( XkcdGameManager ) )]
public class XkcdGameManagerScriptEditor : Editor
{
    public override void OnInspectorGUI() {
        if ( !Application.isEditor || Application.isPlaying ) return;

        DrawDefaultInspector();

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        XkcdHazard.World view 
            = (XkcdHazard.World)( serializedObject.FindProperty( "m_showHazardsInEditor" ).enumValueIndex );
        foreach ( var hazard in FindObjectsOfType<XkcdHazard>() )
            hazard.GetComponent<SpriteRenderer>().enabled = hazard.InWorld( view );
    }
}
#endif // UNITY_EDITOR

public class XkcdGameManager : MonoBehaviour {
    static public XkcdGameManager instance = null;
    
    [SerializeField]
    private XkcdPlayerController m_startCharacter = null;

    [SerializeField]
    private XkcdHazard.World m_showHazardsInEditor = XkcdHazard.World.None;

    [SerializeField]
    private string m_titleSceneName = "";

    [SerializeField]
    private string m_endGameSceneName = "";

    [SerializeField]
    private LayerMask m_realLayerMask;
    public int RealLayerInt {  get { return m_realLayerMask.ToLayerInt(); } }

    [SerializeField]
    private LayerMask m_imaginaryLayerMask;
    public int ImaginaryLayerInt {  get { return m_imaginaryLayerMask.ToLayerInt(); } }

    [SerializeField]
    private SpriteRenderer m_loadingOverlay = null;

    [SerializeField]
    private TextMeshPro m_loadingText = null;

    public void GoToEndScene() {
        SceneManager.LoadScene( m_endGameSceneName );
    }

    public void GoToTitleScene() {
        SceneManager.LoadScene( m_titleSceneName );
    }

    private void Awake() {
        if( instance != null ) {
            Debug.LogErrorFormat( "Duplicate xkcd Game Manager in {0}. Destroying.", instance );
            Destroy( gameObject );
            return;
        }
        instance = this;
    }

    private void Start() {
        Debug.Log( "Start game manager" );
        if ( m_startCharacter == null ) {
            Debug.LogError( "Must set start character." );
            return;
        }

        // re-enable sprites disabled during edit phase
        foreach ( var hazard in FindObjectsOfType<XkcdHazard>() )
            hazard.GetComponent<SpriteRenderer>().enabled = true;

        ShowLoading();

        // set the correct initial character when region is loaded
        RegionManager.instance.OnRegionLoaded.AddListener( () => {
            foreach( var switcher in FindObjectsOfType<SwitcherBase>() ) switcher.DoSwitch();
            HideLoading();
        } );
    }

    private void HideLoading() {
        m_loadingOverlay.color = m_loadingText.color = Color.clear;
    }

    private void ShowLoading() {
        m_loadingText.color = Color.white;
        m_loadingText.text = "Loading";

        m_loadingOverlay.color = Color.black;
        m_loadingOverlay.size = Vector2.one * 100.0f;
    }
}
