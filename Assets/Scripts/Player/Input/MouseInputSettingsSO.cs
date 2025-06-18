using UnityEngine;

namespace Player.Input
{
    [CreateAssetMenu(fileName = "MouseInputSettingsSO", menuName = "Global Settings/MouseInputSettingsSO")]
    public class MouseInputSettingsSO : ScriptableObject
    {
        [Header("Mouse Settings")]
        [field: SerializeField] public float MouseSensitivityX { get; set; } = 10f;
        [field: SerializeField] public float MouseSensitivityY { get; set; } = 10f;
    }
}
