using System;
using UnityEngine;

namespace DialogueSystem.Data.Save
{
    [Serializable]
    public class DSChoiceSaveData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public int Weighting { get; set; }
        [field: SerializeField] public string NodeID { get; set; }
    }
}
