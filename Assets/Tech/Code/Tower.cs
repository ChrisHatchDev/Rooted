using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Linq;

public class Tower : IPowerSource
{
    public Animator Anim;
    public TMPro.TMP_Text StatusText;
    [Space(10)]
    public NavMeshAgent Agent;
    public NavMeshObstacle NavObstacle;
    public bool Moving = false;
    [Header("Material Logic")]
    public SkinnedMeshRenderer Renderer;
    public Material PoweredMat;
    public Material NotPoweredMat;
    public MeshRenderer DamageRangeRenderer;
    public MeshRenderer PowerRangeRenderer;
    public MeshRenderer UnderGlow;

    public Dictionary<GameObject, Enemy> EnemiesInRange = new Dictionary<GameObject, Enemy>();
    public int WeaponDamage = 5;
    public int NumEnemiesToShoot = 3;
    public TowerLineController LineController;
    private bool _recentlyMoved;

    public Transform TorsoTransform;
    public Enemy LookAtTarget;

    public AudioSource AudioThing;

    bool lineIsRendering = false;
    IPowerSource currentLineTarget;

    private void Start()
    {
        StartCoroutine(ShootCycle());
        StartCoroutine(ShootSoundCycle());
        StartCoroutine(CheckHasValidPowerEverySecond());
    }

    public void ShowDamageVisuals()
    {
        DamageRangeRenderer.enabled = true;
    }

    public void HideDamageVisuals()
    {
        DamageRangeRenderer.enabled = false;
    }

    public void ShowPowerRangeVisuals()
    {
        PowerRangeRenderer.enabled = true;
    }

    public void HidePowerRangeVisuals()
    {
        PowerRangeRenderer.enabled = false;
    }

    public void AddEnemeyInRange(Enemy enemy)
    {
        if (!EnemiesInRange.ContainsKey(enemy.gameObject))
        {
            EnemiesInRange.TryAdd(enemy.gameObject, enemy);
            LookAtTarget = enemy;
        }
    }

    public void RemoveEnemeyInRange(Enemy enemy)
    {
        if (EnemiesInRange.ContainsKey(enemy.gameObject))
        {
            EnemiesInRange.Remove(enemy.gameObject);
        }

        if (EnemiesInRange.Count == 0)
        {
            LookAtTarget = null;
        }
    }

    void Update()
    {
        string statusText = PowerSource ? "POWERED" : "NOT POWRED";
        string movingText = Moving ? "Moving" : "Stationary";

        StatusText.text = $"{statusText}\n{movingText}";

        if (_recentlyMoved == false && Agent.enabled && Agent.remainingDistance <= Agent.stoppingDistance + 0.05f)
        {
            Agent.enabled = false;
            NavObstacle.enabled = true;
            Moving = false;

            HasValidPower(PowerSource, new List<IPowerSource>());

            Anim.SetTrigger("Sit");
        }

        SetPowerStatusVisuals();

        if (currentLineTarget)
        {
            LineController.Show();
            LineController.SetLinePoints(LineConnectionPoint.position, currentLineTarget.LineConnectionPoint.transform.position);
        }
        
        if (PowerSourceList.Count == 0 || !PowerSourceList.Contains(currentLineTarget))
        {
            LineController.Hide();
            currentLineTarget = null;
        }

        if (LookAtTarget && PowerSource && !Moving)
        {
            TorsoTransform.LookAt(LookAtTarget.transform.position);
            TorsoTransform.transform.rotation = Quaternion.Euler(new Vector3(0, TorsoTransform.rotation.eulerAngles.y, TorsoTransform.rotation.eulerAngles.z));
        }

        Anim.SetBool("Shooting", EnemiesInRange.Count > 0 && !Moving && PowerSource && LookAtTarget);
    }

    public void OnPickUp()
    {
        ShowPowerRangeVisuals();

        if (!Moving)
        {
            Anim.SetTrigger("GetUp");
        }
    }

    public void OnPlaced(Vector3 placedPos)
    {
        // Debug.Log($"Placed Turret now to: {placedPos}");

        NavObstacle.enabled = false;
        Agent.enabled = true;
        Moving = true;

        Agent.SetDestination(placedPos);
        HidePowerRangeVisuals();

        _recentlyMoved = true;

        StartCoroutine(WaitToRenableAgent());
    }

    public void OnCancelPlacement(Vector3 placedPos)
    {
        if (!Moving)
        {
            Anim.SetTrigger("Sit");
        }
    }

    IEnumerator WaitToRenableAgent()
    {
        yield return new WaitForSeconds(1.0f);
        _recentlyMoved = false;
    }

    private IEnumerator ShootCycle()
    {
        yield return new WaitForSeconds(0.5f);
        ShootAllEnemiesInRange();
        StartCoroutine(ShootCycle());
    }

    private IEnumerator ShootSoundCycle()
    {
        yield return new WaitForSeconds(0.3f);

        if (PowerSource && !Moving && EnemiesInRange.Count > 0 && LookAtTarget)
        {
            AudioThing.pitch = Random.Range(1, 3f);
            AudioThing.PlayOneShot(AudioThing.clip);
        }

        StartCoroutine(ShootSoundCycle());
    }

    public void ShootAllEnemiesInRange()
    {
        if(PowerSource && !Moving)
        {
            foreach (var enemeyPair in EnemiesInRange)
            {
                enemeyPair.Value.Damage(Random.Range(3,WeaponDamage));
            }
        }
    }

    public void SetPowerStatusVisuals()
    {
        Renderer.materials = new Material[]{PowerSource ? PoweredMat : NotPoweredMat};
        UnderGlow.enabled = PowerSource;
    }


    IEnumerator CheckHasValidPowerEverySecond()
    {
        yield return new WaitForSeconds(0.75f);
        
        foreach(var ps in PowerSourceList)
        {
            if(HasValidPower(ps, new List<IPowerSource>())){
                PowerSource = ps;

                if (!currentLineTarget)
                {
                    currentLineTarget = PowerSource;
                }
            }else{
                PowerSource = null;
            }
        }

        StartCoroutine(CheckHasValidPowerEverySecond());
    }


    public override bool HasValidPower(IPowerSource ps, List<IPowerSource> seen){
        if(ps == null){
            PowerSource = null;
            return false;
        } else if(ps is Base){
            PowerSource = ps;
            return true;
        } else if(seen.Contains(ps)){
            return false;
        }
        seen.Add(ps);
        foreach(var newps in ps.PowerSourceList){
            return ps.HasValidPower(newps, seen);
        }
        PowerSource=null;
        return false;
    }

    public void OnPowerSourceEnter(Collider other)
    {
        Debug.Log("OnTowerPowerEnter");
        var otherPowerSource = other.gameObject.GetComponentInParent<IPowerSource>();
        PowerSourceList.Add(otherPowerSource);
        //PowerSource = null;
        foreach(var ps in PowerSourceList){
            if(ps is Base){
                PowerSource = ps;
                return;
            }
            if(HasValidPower(ps, new List<IPowerSource>())){
                PowerSource = ps;
            }
        }
    }

    public void OnPowerSourceExit(Collider other)
    {
        Debug.Log("OnTowerPowerExit");
        var otherPowerSource = other.gameObject.GetComponentInParent<IPowerSource>();
        PowerSourceList.Remove(otherPowerSource);
        if(PowerSourceList.Count == 0){
            PowerSource = null;
        }
        foreach(var ps in PowerSourceList){
            if(ps is Base){
                PowerSource = ps;
                return;
            }
            if(HasValidPower(ps, new List<IPowerSource>())){
                PowerSource = ps;
            } 
        }
    }
    public override PowerSourceType GetPowerType()
    {
        return PowerType;
    }
}
