using UnityEngine;
using Unity.MLAgents;



public class TankDecisionRequester : DecisionRequester {

    [SerializeField] private TankFireActuator _tankFireActuator;



    protected override bool ShouldRequestDecision(DecisionRequestContext context) {
        return !(_tankFireActuator.ActiveProjectile) && base.ShouldRequestDecision(context);
    }

    

}