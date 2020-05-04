using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour {
    static public DialogueManager instance = null;

    [Header("Font Settings")]

    [SerializeField]
    private float m_textFontSize = 4;
       
    [SerializeField]
    private float m_speakerFontSize = 6;

    public Color textColor = Color.white;

    [Header( "Background Settings" )]

    [SerializeField]
    [Tooltip( "Number of milliseconds between each character printed" )]
    private float m_charWaitTimeMs = 50;

    [SerializeField]
    [Range(1.0f, 10.0f)]
    [Tooltip( "Multiplier for speed at which characters appear when 'go faster' button is held" )]
    float m_charWaitTimeFastMult = 5.0f;

    [SerializeField]
    [Range(0.0f, 0.5f)]
    private float m_heightPercent = 0.2f;

    [Header("Sprites")]

    [SerializeField]
    private Sprite m_backgroundSprite;

    [SerializeField]
    private Sprite m_endPageSprite;

    [SerializeField]
    private Sprite m_endTextSprite;

    [SerializeField]
    private float m_endSymbolBlinkTimeMs = 500.0f;

    [Header("Debug")]

    [SerializeField]
    private bool m_doTest = false;

    public bool IsShowing { get; private set; }

    public bool AcceleratePrint { private get; set; }

    private float m_timeElapsed = 0.0f;

    private bool m_fullPageRevealed = false;

    private string[] m_pageList = null;
    private int m_pageIndex = 0;
    private int m_charIndex = 0;

    private Vector3 m_unitResolution;
    private int m_width;

    private SpriteRenderer m_endPageSymbol;
    private SpriteRenderer m_endTextSymbol;

    private SpriteRenderer m_portraitBackground;
    private SpriteRenderer m_portraitSprite;
    private SpriteRenderer m_textBackground;

    private string m_audioName = null;

    private TextMeshPro m_textMesh;
    private TextMeshPro m_speakerNameMesh;

    private string CurPage { get { return m_pageList[m_pageIndex]; } }

    public string Text {
        set {
            ResizeAndReposition();

            Debug.LogFormat( "set text to {0}", value );
            m_textMesh.text = value;

            m_textBackground.color = Color.white;
            m_portraitBackground.color = Color.white;
            m_portraitSprite.color = Color.white;

            // we recurse back here if we process a speaker or audio, so stop if we do
            if( ProcessSpeaker() ) return;
            if ( ProcessAudio() ) return;

            m_pageIndex = 0;
            m_charIndex = 0;
            StartCoroutine( DisplayPage( true ) );
        }
    }

    private Vector2 PortraitPos {
        get {
            var x = -TextBoxSize.x / 2;
            var y = TextBoxPos.y;
            return new Vector2( x, y );
        }
    }

    private Vector2 PortraitSize {
        get {
            var s = m_width * m_heightPercent;
            return new Vector2( s, s );
        }
    }

    private Vector2 TextBoxPos {
        get {
            var x = PortraitSize.x / 2;
            var y = ( m_unitResolution.y - PortraitSize.y ) / 2;
            return new Vector2( x, y );
        }
    }

    private Vector2 TextBoxSize {
        get {
            var x = m_width * (1.0f - m_heightPercent);
            var y = PortraitSize.y;
            return new Vector2( x, y );
        }
    }

    private void Awake() {
        if ( instance != null ) {
            Destroy( gameObject );
            return;
        }

        instance = this;

        m_endPageSymbol = CreateSprite( m_endPageSprite );
        m_endPageSymbol.name = "[DM] End Page Symbol";
        m_endPageSymbol.sortingLayerName = "Text";

        m_endTextSymbol = CreateSprite( m_endTextSprite );
        m_endTextSymbol.name = "[DM] End Text Symbol";
        m_endTextSymbol.sortingLayerName = "Text";

        m_textBackground = CreateSprite( m_backgroundSprite );
        m_textBackground.drawMode = SpriteDrawMode.Sliced;
        m_textBackground.name = "[DM] Text Background";
        m_textBackground.sortingLayerName = "HUD";

        m_portraitBackground = CreateSprite( m_backgroundSprite );
        m_portraitBackground.drawMode = SpriteDrawMode.Sliced;
        m_portraitBackground.name = "[DM] Portrait Background";
        m_portraitBackground.sortingLayerName = "HUD";

        m_portraitSprite = CreateSprite();
        m_portraitSprite.name = "[DM] Portrait Sprite";
        m_portraitSprite.sortingLayerName = "Text";

        var fontScale = Camera.main.orthographicSize / 5.0f;

        m_textMesh = CreateTextMesh();
        m_textMesh.overflowMode = TextOverflowModes.Page;
        m_textMesh.fontSize = m_textFontSize * fontScale;
        m_textMesh.name = "[DM] Main Text";
        {
            var sorting = m_textMesh.gameObject.AddComponent<SortingOrder>();
            sorting.sortingName = "Text";
        }

        m_speakerNameMesh = CreateTextMesh();
        m_speakerNameMesh.fontSize = m_speakerFontSize * fontScale;
        m_speakerNameMesh.alignment = TextAlignmentOptions.Center;
        m_speakerNameMesh.name = "[DM] Speaker Name";
        {
            var sorting 
                = m_speakerNameMesh.gameObject.AddComponent<SortingOrder>();
            sorting.sortingName = "Text";
        }

        Clear();
    }

    private void Start() {
        ResizeAndReposition();

        if ( !m_doTest ) return;

        var baseText = "This is a testing text \nfor the dialogue manager. ";
        var text = "";
        for( int i = 0; i < 3; ++i ) text += baseText;
        Text = "$testing$^Three Two^" + text;
    }

    private void Clear() {
        m_textMesh.pageToDisplay = 0;
        m_textMesh.text = "";
        m_textMesh.color = Color.clear;

        m_textBackground.color = Color.clear;
        m_portraitBackground.color = Color.clear;
        m_portraitSprite.color = Color.clear;
        m_endPageSymbol.color = Color.clear;
        m_endTextSymbol.color = Color.clear;

        IsShowing = false;
        m_speakerNameMesh.text = "";
        m_timeElapsed = 0.0f;
    }

    private SpriteRenderer CreateSprite( Sprite a_sprite = null ) {
        var obj = new GameObject();
        obj.transform.parent = transform;

        var spriteRenderer = obj.AddComponent<SpriteRenderer>();
        if( a_sprite != null ) spriteRenderer.sprite = a_sprite;
        return spriteRenderer;
    }

    private TextMeshPro CreateTextMesh() {
        var obj = new GameObject();
        obj.transform.parent = transform;

        transform.transform.transform.transform.transform.transform.transform.position = Vector3.zero;

        var textMesh = obj.AddComponent<TextMeshPro>();
        return textMesh;
    }

    private IEnumerator DisplayPage( bool m_initial ) {
        if ( m_initial ) {

            // let text mesh pro do its update thing
            yield return new WaitForEndOfFrame();

            Debug.LogFormat( "Full message has {0} chars", m_textMesh.text.Length );
            var pageCount = m_textMesh.textInfo.pageCount;
            m_pageList = new string[pageCount];
            for ( int i = 0; i < pageCount; ++i ) {
                var start = m_textMesh.textInfo.pageInfo[i].firstCharacterIndex;
                var end = m_textMesh.textInfo.pageInfo[i].lastCharacterIndex;
                var length = end - start + 1;
                Debug.LogFormat( "Page {0} has {1} chars", i, length );
                m_pageList[i] = m_textMesh.text.Substring( start, length );
            }

            Debug.LogFormat( "Initially {0} pages", m_pageList.Length );
            var newPageList = new List<string>();
            int k = 0;
            foreach( var page in m_pageList ) {
                ++k;
                Debug.LogFormat( "Page #{0}", k );
                if ( page.Contains( "\n" ) ) {
                    var split = page.Split( '\n' );
                    Debug.LogFormat( "{0} splits", split.Length );
                    foreach ( var p in split ) {
                        Debug.LogFormat( "Split: '{0}' ({1} chars)", p, p.Length );
                        p.Trim();
                        if ( p.Length == 0 ) continue;
                        newPageList.Add( p );
                    }
                } else newPageList.Add( page );
            }

            m_pageList = newPageList.ToArray();
            Debug.LogFormat( "New page count: {0}", m_pageList.Length );
        }

        if( m_audioName != null ) m_audioSource = SoundManager.instance.PlaySound( m_audioName );

        m_textMesh.color = textColor;
        m_textMesh.text = "";
        m_fullPageRevealed = false;

        IsShowing = true;
    }

    private AudioSource m_audioSource = null;

    public void RevealOrNextPage() {
        if ( !IsShowing ) return;
        m_endPageSymbol.color = Color.clear;
        m_endTextSymbol.color = Color.clear;
        if( !m_fullPageRevealed ) {
            StopAllCoroutines();
            Reveal();
            return;
        }

        if ( m_audioSource != null ) m_audioSource.Stop();
        m_audioName = null;

        ++m_pageIndex;
        if( m_pageIndex >= m_pageList.Length ) {
            Clear();
            return;
        }

        m_charIndex = 0;
        StartCoroutine( DisplayPage(false) );
    }

    private void Update() {
        if ( !IsShowing ) return;

        m_timeElapsed += Time.deltaTime;

        if( m_fullPageRevealed) {
            if( m_timeElapsed > m_endSymbolBlinkTimeMs / 1000.0f) {
                if ( m_pageIndex == m_pageList.Length - 1 ) {
                    if ( m_endTextSymbol.color == Color.white ) m_endTextSymbol.color = Color.clear;
                    else m_endTextSymbol.color = Color.white;
                } else {
                    if ( m_endPageSymbol.color == Color.white ) m_endPageSymbol.color = Color.clear;
                    else m_endPageSymbol.color = Color.white;
                }

                m_timeElapsed -= m_endSymbolBlinkTimeMs / 1000.0f;
            }

            return;
        }

        if ( AcceleratePrint ) m_timeElapsed += Time.deltaTime * ( m_charWaitTimeFastMult - 1.0f );
        if ( m_timeElapsed < m_charWaitTimeMs / 1000.0f ) return;

        m_textMesh.text += CurPage[m_charIndex];
        ++m_charIndex;

        if( m_charIndex >= CurPage.Length ) Reveal();

        m_timeElapsed = 0.0f;
    }

    private void PositionSymbols() {
        var pos = TextBoxPos;
        var extents = TextBoxSize / 2;
        pos += new Vector2( extents.x - 0.75f, -extents.y + 0.5f );
        m_endPageSymbol.transform.position = pos;
        m_endTextSymbol.transform.position = pos;
    }

    private bool ProcessAudio() {
        if ( !m_textMesh.text.Contains( "^" ) ) return false;

        var splits = m_textMesh.text.Split( '^' );
        m_audioName = splits[1];
        Text = splits[0] + splits[2];
        return true;
    }

    private bool ProcessSpeaker() {
        if ( !m_textMesh.text.Contains( "$" ) ) return false;

        // NOTE this only works if we have EXACTLY ONE item marker
        var splits = m_textMesh.text.Split( '$' );
        var itemName = splits[1];

        Text = splits[0] + splits[2];

        var item = ItemDatabase.instance.GetItem( itemName );
        if( item == null ) {
            Debug.LogFormat( 
                "No matching item for '{0}', displaying raw name instead",
                itemName );
            m_speakerNameMesh.text = itemName;
            return true;
        }

        m_speakerNameMesh.text = item.displayName;
        m_portraitSprite.sprite = item.displaySprite;
        return true;
    }

    private void ResizeAndReposition() {
        var height = Camera.main.orthographicSize * 2.0f;
        var width = height * Camera.main.aspect;
        m_unitResolution = new Vector2( width, height );

        m_unitResolution.x = Mathf.FloorToInt( m_unitResolution.x );
        m_unitResolution.y = Mathf.FloorToInt( m_unitResolution.y );

        m_width = Mathf.FloorToInt( m_unitResolution.x );

        ResizeTextBackground();
        ResizeTextArea();

        ResizePortrait();
        ResizePortraitTextArea();

        PositionSymbols();
    }

    private void ResizeTextArea() {
        var textTransform = m_textMesh.GetComponent<RectTransform>();

        textTransform.SetPositionAndRotation( 
            TextBoxPos + (Vector2)Camera.main.transform.position, 
            Quaternion.identity );

        var size = TextBoxSize - Vector2.one;
        textTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal, size.x );
        textTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical, size.y );
    }

    private void ResizeTextBackground() {
        m_textBackground.size = TextBoxSize;
        m_textBackground.transform.position 
            = TextBoxPos + (Vector2)Camera.main.transform.position;
    }

    private void ResizePortrait() {
        m_portraitBackground.size = PortraitSize;
        m_portraitBackground.transform.position 
            = PortraitPos + (Vector2)Camera.main.transform.position;

        m_portraitSprite.transform.position 
            = PortraitPos + (Vector2)Camera.main.transform.position;
    }

    private void ResizePortraitTextArea() {
        var textTransform = m_speakerNameMesh.GetComponent<RectTransform>();

        var pos = PortraitPos + (Vector2)Camera.main.transform.position;
        pos.y -= PortraitSize.y * 0.25f;
        textTransform.SetPositionAndRotation( pos, Quaternion.identity );

        var size = PortraitSize - Vector2.one;
        textTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal, size.x );
        textTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical, size.y );
    }

    private void Reveal() {
        m_charIndex = CurPage.Length;
        m_textMesh.text = CurPage;
        m_fullPageRevealed = true;
    }
}
