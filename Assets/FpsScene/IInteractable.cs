using UnityEngine;

// Tüm etkileþimli nesneler için ortak arayüz
public interface IInteractable
{
    string GetInteractPrompt();
    void OnInteract();
    bool CanInteract();
}