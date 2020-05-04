using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ScriptEditor : Editor
{
    private List<string> m_propSkipList = new List<string>();

    public ScriptEditor()  {
        SkipProperty( "m_Script" );
    }

    protected void SkipProperty(string a_propName ) {
        m_propSkipList.Add( a_propName );
    }

    protected void ShowDefaultInspector() {
        var propIter = serializedObject.GetIterator();
        if ( propIter.NextVisible( true ) ) {
            do {
                var prop = serializedObject.FindProperty( propIter.name );

                bool doSkip = false;
                foreach ( var skipName in m_propSkipList ) {
                    if ( skipName == prop.name ) {
                        doSkip = true;
                        break;
                    }
                }
                if ( doSkip ) continue;

                UnityEditor.EditorGUILayout.PropertyField( prop, true );
            } while ( propIter.NextVisible( false ) );
        }

        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
    }
}

