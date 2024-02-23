using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrincessKid : Kid
{
    protected override void Start()
    {
        base.Start();

        type = KidType.PRINCESS;
    }


    //protected override void Update()
    //{
    //    base.Update();


    //    Debug.Log("Princess kid state: " + _state);
    //    Debug.Log("In princess zone?: " + InPrincessAlertZone);
    //    Debug.Log("Flag for princess?: " + flagAlertedByPrincess);

    //}



    // if princess becomes hunting, alert nearby kids and set them to hunting
    // alerted kids continue to pursue player regardless of distance, until princess is not hunting (she goes to searching next)
    // if princess becomes searching, disable flag and kids that were hunting and are out of range are now searching (kids that are still in range revert to normal hunting)

    // does she continuosly tell kids to attack? i can ask teammates

    protected override void SetAIState(KidAIState state)
    {
        if (IsAIState(state) || !gameObject.activeSelf)
        {
            return;
        }

        base.SetAIState(state);

        // problem here, need to also skip like base if already same state
        switch (state)
        {
            case KidAIState.HUNTING:

                CommandNearbyKids(true);
                return;

            case KidAIState.IDLE:
            case KidAIState.WANDERING:

                CommandNearbyKids(false);
                return;
        }
    }

    
    // can add animation/ sfx, etc to this
    private void CommandNearbyKids(bool huntPlayer)
    {
        Events.OnPrincessCommanding.Invoke(huntPlayer);
    }

}
