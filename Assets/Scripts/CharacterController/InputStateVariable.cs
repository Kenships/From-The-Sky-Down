using UnityEngine;
using Obvious.Soap;

namespace CharacterController
{
    
    [CreateAssetMenu(fileName = "ScriptableVariable" + nameof(InputState), menuName = "Soap/ScriptableVariables/"+ nameof(InputState))]
    public class InputStateVariable : ScriptableVariable<InputState>
    {
            
    }
}
