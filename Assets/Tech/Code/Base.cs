using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Base))]
public class BaseEditor: Editor
{
    private void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        Base _target = target as Base;

        GUILayout.Label($"Towers in Range (${_target.TowersInRange.Count})");
    }

}

public class Base : MonoBehaviour
{
    // 1-10. 10 Being full health
    public int Health = 10;

    public Dictionary<GameObject, Tower> TowersInRange = new Dictionary<GameObject, Tower>();

    public void Damage(int damage)
    {
        Health -= damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Tower")
        {
            if (!TowersInRange.ContainsKey(other.gameObject))
            {
                Tower newTower = other.GetComponent<Tower>();
                newTower.SetPowerStatus(true);

                TowersInRange.TryAdd(other.gameObject, newTower);
            }
        }   
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Tower")
        {
            Debug.Log("Tower is Leaving base!");
            if (TowersInRange.ContainsKey(other.gameObject))
            {
                TowersInRange.GetValueOrDefault(other.gameObject).SetPowerStatus(false);
                TowersInRange.Remove(other.gameObject);
            }
        } 
    }
}