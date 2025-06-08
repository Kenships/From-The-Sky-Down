using UnityEngine;

namespace DefaultNamespace
{
    public class InteractObject : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject dialogue;
        

        public void Interact()
        {
            Instantiate(dialogue, canvas.transform);
        }
    }
}