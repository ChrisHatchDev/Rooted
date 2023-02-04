using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject TestPositionCube;
    public LayerMask GroundMask;
    public LayerMask TowerMask;

    public Tower TargetTower;
    public bool towerPickedUp = false;

    Vector3 currentPointedPos;
    public bool pointingAtGround = false;

    void Start()
    {
        
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100000, GroundMask))
        {
            TestPositionCube.transform.position = hit.point;
            currentPointedPos = hit.point;
            pointingAtGround = true;
        }
        else
        {
            currentPointedPos = Vector3.zero;
            pointingAtGround = false;
        }

        Ray rayTower = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitTower;

        if (Physics.Raycast(rayTower, out hit, 100000, TowerMask))
        {
            if (Input.GetMouseButtonDown(0))
            {
                TargetTower = hit.collider.GetComponent<Tower>();
            }
            Debug.Log("Pointing at tower!");
        }
        else if (towerPickedUp == false)
        {
            TargetTower = null;
            //Debug.Log("NOT POINTING at tower!");
        }

        if (Input.GetMouseButtonDown(0) && TargetTower)
        {
            if (towerPickedUp)
            {
                PlaceTurret(TargetTower);
            }

            if (towerPickedUp == false && TargetTower)
            {
                PickupTurret(TargetTower);
            }

        }
    }

    public void PlaceTurret(Tower tower)
    {
        if (pointingAtGround)
        {
            TargetTower = null;
            tower.Agent.SetDestination(currentPointedPos);
            towerPickedUp = false;
            
            Debug.Log("Placed Turret");
        }
        else
        {
            Debug.Log("Can't place not on ground");
        }
    }

    public void PickupTurret(Tower tower)
    {
        Debug.Log("PICKED UP TOWER");
        towerPickedUp = true;
    }
}
