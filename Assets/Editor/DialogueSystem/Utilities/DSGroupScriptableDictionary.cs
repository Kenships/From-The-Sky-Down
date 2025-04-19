using System;
using UnityEngine;
using Obvious.Soap;
using UnityEditor.Experimental.GraphView;

namespace DialogueSystem.Utilities
{
    [CreateAssetMenu(fileName = nameof(DSGroupScriptableDictionary), menuName = "Soap/ScriptableDictionary/"+nameof(DSGroupScriptableDictionary))]
    public class DSGroupScriptableDictionary : ScriptableDictionary<Group,DSNodeErrorScriptableDictionary>
    {
        
    }
}
