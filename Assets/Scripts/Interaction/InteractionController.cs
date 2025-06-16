using System;
using System.Collections.Generic;
using CharacterController;
using DialogueSystem.Utilities;
using UnityEngine;

namespace Interaction
{
    public class InteractionController : MonoBehaviour
    {
        [SerializeField] private ScriptableEventRadarBogieInfo radarBogieInfo;
        [SerializeField] private InputReaderSO inputReader;
        [SerializeField] private List<GameObject> bogies = new();
        private readonly List<IInteractable> _objectsInRange = new();

        private void Start()
        {
            inputReader.RequestInteract += Interact;
            inputReader.CancelInteract += CancelInteract;
            radarBogieInfo.OnRaised += HandleBogie;
        }

        private void CancelInteract()
        {
            if (_objectsInRange.Count == 0) return;
            _objectsInRange[0].CancelInteract();
        }

        private void Interact()
        {
            if (_objectsInRange.Count == 0) return;
            _objectsInRange[0].Interact();
        }

        private void HandleBogie(RadarBogieInfo bogieInfo)
        {
            GameObject bogie = bogieInfo.Bogie;

            if (bogie.TryGetComponent(out IInteractable interactable))
            {
                if (bogieInfo.InRange)
                {
                    interactable.HoverSelect();
                    _objectsInRange.Add(interactable);
                    bogies.Add(bogie);
                }
                else
                {
                    interactable.HoverDeselect();
                    interactable.CancelInteract();
                    _objectsInRange.Remove(interactable);
                    bogies.Remove(bogie);
                }
            }
        }
    }
}
