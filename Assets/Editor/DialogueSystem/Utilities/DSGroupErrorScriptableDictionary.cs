using System;
using UnityEngine;
using Obvious.Soap;

namespace DialogueSystem.Utilities
{
    using Data.Error;
    [CreateAssetMenu(fileName = nameof(DSGroupErrorScriptableDictionary), menuName = "Soap/ScriptableDictionary/"+nameof(DSGroupErrorScriptableDictionary))]
    public class DSGroupErrorScriptableDictionary : ScriptableDictionary<string,DSGroupErrorData>
    {
        
    }
}
