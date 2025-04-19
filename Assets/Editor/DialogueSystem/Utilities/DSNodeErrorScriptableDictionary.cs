using System;
using System.Collections.Generic;
using UnityEngine;
using Obvious.Soap;

namespace DialogueSystem.Utilities
{
    using Data.Error;
    [CreateAssetMenu(fileName = nameof(DSNodeErrorScriptableDictionary), menuName = "Soap/ScriptableDictionary/"+nameof(DSNodeErrorScriptableDictionary))]
    public class DSNodeErrorScriptableDictionary : ScriptableDictionary<string,DSNodeErrorData>
    {
        
    }
}
