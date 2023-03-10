using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Base BaseInScene;

    public GameObject TestPositionCube;
    public LayerMask GroundMask;
    public LayerMask TowerMask;

    public Tower TargetTower;
    public bool towerPickedUp = false;

    Vector3 currentPointedPos;
    public bool pointingAtGround = false;

    public GameObject TowerPrefab;
    public bool PlacingATowerMode;
    public int InitialNumberOfTowers = 3;

    void Start()
    {
        BaseInScene = FindObjectOfType<Base>();
        EnterTowerToPlaceMode();
    }

    public void EnterTowerToPlaceMode()
    {
        PlacingATowerMode = true;
        TestPositionCube.SetActive(true);
    }
    
    public void PlaceNewTurret(Vector3 position)
    {
        // Debug.Log($"Place new object at this position: {position}");

        // If base is destroyed the game is over and we can't spawn new towers
        if (BaseInScene == null) return;

        Tower newTower = Instantiate(TowerPrefab, BaseInScene.transform.position, Quaternion.identity).GetComponent<Tower>();
        newTower.OnPlaced(position);

        if (InitialNumberOfTowers > 1)
        {
            InitialNumberOfTowers -= 1;
        }
        else
        {
            PlacingATowerMode = false;
            TestPositionCube.SetActive(false);
            StartGame();
        }
    }

    public void StartGame()
    {
        // Trigger enemey spawner here
    }

    void Update()
    {
        if (MainUI.GameIsActive == false)
        {
            return;
        }

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
            if (Input.GetMouseButtonDown(0) && TargetTower == null)
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

        if (Input.GetMouseButtonDown(0))
        {
            if (PlacingATowerMode && pointingAtGround)
            {
                PlaceNewTurret(currentPointedPos);
            }
            else
            {
                if (towerPickedUp && TargetTower)
                {
                    PlaceTower(TargetTower);
                }

                if (towerPickedUp == false && TargetTower)
                {
                    PickupTower(TargetTower);
                }
            }
        }


        if (Input.GetMouseButtonDown(1) && towerPickedUp && TargetTower)
        {
            CancelPlaceTower(TargetTower);
        }
    }

    public void PlaceTower(Tower tower)
    {
        if (pointingAtGround)
        {
            TargetTower = null;
            towerPickedUp = false;
            tower.OnPlaced(currentPointedPos);
            TestPositionCube.SetActive(false);
            
            // Debug.Log("Placed Turret");

            // HIDE all power ranges when in NOT in pickup mode
            foreach (var item in FindObjectsOfType<Tower>())
            {
                item.HidePowerRangeVisuals();
            }
            // BaseInScene.HidePowerRangeVisuals();
        }
        else
        {
            Debug.Log("Can't place not on ground");
        }
    }

    public void CancelPlaceTower(Tower tower)
    {
        Debug.Log("Cancelled Tower Placement");
        TargetTower = null;
        towerPickedUp = false;

        tower.OnCancelPlacement(currentPointedPos);
        
        TestPositionCube.SetActive(false);
        
        foreach (var item in FindObjectsOfType<Tower>())
        {
            item.HidePowerRangeVisuals();
        }
    }

    public void PickupTower(Tower tower)
    {
        // Debug.Log("PICKED UP TOWER");
        towerPickedUp = true;
        tower.OnPickUp();

        // Show all power ranges when in pickup mode
        foreach (var item in FindObjectsOfType<Tower>())
        {
            item.ShowPowerRangeVisuals();
        }
        // BaseInScene.ShowPowerRangeVisuals();

        TestPositionCube.SetActive(true);
    }
}
