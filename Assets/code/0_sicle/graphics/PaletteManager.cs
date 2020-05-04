using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaletteManager : MonoBehaviour {
    static public PaletteManager instance = null;

    [SerializeField]
    private List<Palette> m_paletteList = new List<Palette>();

    [SerializeField]
    private int m_defaultPaletteIndex = 0;

    public Palette DefaultPalette { get { return GetPalette( m_defaultPaletteIndex ); } }
    public int DefaultPaletteIndex {
        get { return m_defaultPaletteIndex; }
        set { m_defaultPaletteIndex = value; }
    }

    public int AddPalette(Palette a_palette) {
        m_paletteList.Add( a_palette );
        return m_paletteList.Count - 1;
    }

    public Palette GetPalette(int i) {
        if( i < 0 || i > m_paletteList.Count) {
            Debug.LogErrorFormat( "Invalid palette index {0}/{1}", i, m_paletteList.Count );
            return null;
        }
        return m_paletteList[i];
    }

	void Awake () {
        if( instance != null ) {
            Debug.LogErrorFormat( "Duplicate palette manager in {0}", gameObject );
            Destroy( gameObject );
            return;
        }
        instance = this;
	}
}
