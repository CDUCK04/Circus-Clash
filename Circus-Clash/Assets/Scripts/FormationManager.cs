using System.Collections.Generic;
using UnityEngine;

public class FormationManager : MonoBehaviour
{
    [Header("Grid (max 5 x 4 = 20)")]
    [Range(1, 5)] public int rows = 5;
    [Range(1, 4)] public int columns = 4;
    public float spacingX = 0.9f;
    public float spacingY = 0.6f;

    [Header("Orientation")]
    public bool playerSideMovesRight = true;

    [Header("Anchor")]
    public Transform formationAnchor; 

    List<UnitBrain> units = new List<UnitBrain>();

    public int Capacity => rows * columns;
    public bool Register(UnitBrain u)
    {
        Cleanup();
        if (units.Count >= Capacity) return false;
        units.Add(u);
        AssignSlot(u, units.Count - 1);
        return true;
    }

    public void Unregister(UnitBrain u)
    {
        int idx = units.IndexOf(u);
        if (idx >= 0) units.RemoveAt(idx);
        ReassignAll();
    }

    public void RecallToFormation()
    {
        Cleanup();
        for (int i = 0; i < units.Count; i++)
            AssignSlot(units[i], i);
    }

    void ReassignAll()
    {
        for (int i = 0; i < units.Count; i++)
            AssignSlot(units[i], i);
    }

    void Cleanup()
    {
        for (int i = units.Count - 1; i >= 0; i--)
            if (units[i] == null) units.RemoveAt(i);
    }

    void AssignSlot(UnitBrain u, int idx)
    {
        if (!u || !formationAnchor) return;
        int row = idx / columns;
        int col = idx % columns;
        u.SetFormationSlot(GetSlotWorld(row, col), row, col);
    }

    public Vector3 GetSlotWorld(int row, int col)
    {
        Vector3 a = formationAnchor ? formationAnchor.position : Vector3.zero;
        float totalH = (rows - 1) * spacingY;
        float yTop = a.y + totalH * 0.5f;
        float y = yTop - row * spacingY;
        float dir = playerSideMovesRight ? -1f : +1f; // columns extend backward
        float x = a.x + col * spacingX * dir;
        return new Vector3(x, y, a.z);
    }
}