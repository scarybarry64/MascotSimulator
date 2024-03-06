using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrossKid : Kid
{
    public const int BOOGER_DAMAGE = BASE_HUG_DAMAGE * 4/3;
    public const float BOOGER_SPEED = 10f;//7.5
    public const float BOOGER_DURATION = 5f;

    private Coroutine _coroutineBoogerAttackAI; // not its own KidAIState, since it happens while in Hunting state

    
    protected override void Start()
    {
        base.Start();

        type = KidType.GROSS;

        AttackSpeed = BASE_ATTACK_SPEED / 3f;
    }


    //protected override void Update()
    //{
    //    base.Update();


    //    Debug.Log("Gross kid state: " + _state);
    //    Debug.Log("In princess zone?: " + InPrincessAlertZone);
    //    Debug.Log("Flag for princess?: " + flagAlertedByPrincess);

    //}



    #region AI State Machine

    protected override void SetAIState(KidAIState state)
    {
        if (IsAIState(state) || !gameObject.activeSelf)
        {
            return;
        }

        base.SetAIState(state);

        if (IsAIState(KidAIState.HUNTING) && _coroutineBoogerAttackAI == null)
        {
            _coroutineBoogerAttackAI = StartCoroutine(BoogerAttackAICoroutine());
        }
        else if (!IsAIState(KidAIState.HUNTING) && _coroutineBoogerAttackAI != null)
        {
            StopCoroutine(_coroutineBoogerAttackAI);
            _coroutineBoogerAttackAI = null;
        }
    }

    #endregion


    #region AI Coroutines

    private IEnumerator BoogerAttackAICoroutine()
    {
        while (IsAIState(KidAIState.HUNTING))
        {
            BoogerAttack();

            yield return new WaitForSecondsRealtime(CalculateTimeBetweenAttacks() * 1.5f);
        }

        _coroutineBoogerAttackAI = null;
    }

    protected override void StopAllAICoroutines()
    {
        base.StopAllAICoroutines();

        if (_coroutineBoogerAttackAI != null)
        {
            StopCoroutine(_coroutineBoogerAttackAI);
            _coroutineBoogerAttackAI = null;
        }
    }

    #endregion


    #region Actions

    private void BoogerAttack()
    {
        //DeadlyBoogersProjectile boogers = Instantiate(_boogers, transform.position, Quaternion.identity);
        //boogers.Direction = (_positionPlayerLastSeen - (Vector2)transform.position).normalized;

        AttackSound();
        Vector2 direction = (_positionPlayerLastSeen - (Vector2)transform.position).normalized;


        GameManager.Instance.Projectiles.SpawnProjectile(ProjectileType.DEADLY_BOOGERS, null, transform.position, BOOGER_SPEED, direction);
    }

    #endregion
}
