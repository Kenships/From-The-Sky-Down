using Obvious.Soap;
using UnityEngine;

namespace Player.Input
{
    
    [CreateAssetMenu(fileName = "ScriptableVariable" + nameof(InputState), menuName = "Soap/ScriptableVariables/"+ nameof(InputState))]
    public class InputStateVariable : ScriptableVariable<InputState>
    {
            
    }
}
