namespace Interaction.Interfaces
{
    public interface IInteractable
    {
        public string Name
        {
            get;
            set;
        }
        
        public void Interact();
        public void CancelInteract();
    }
}
