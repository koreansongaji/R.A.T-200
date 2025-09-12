using UnityEngine;
using System.Collections.Generic;

public class PlayerInteractor : MonoBehaviour
{
    // �κ��丮 �ؽ� ��
    private readonly HashSet<string> inventory = new();
    public bool HasItem(string id) => inventory.Contains(id);
    public void AddItem(string id) => inventory.Add(id);
    public bool RemoveItem(string id) => inventory.Remove(id);
}