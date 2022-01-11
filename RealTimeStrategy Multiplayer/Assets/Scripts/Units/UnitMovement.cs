using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float chaseRange = 10f;

    private Camera mainCamera;

    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [ServerCallback] //Use this when you wanna do [Server] but don't control the function(eg unity functions like update/start/trigger..)
    private void Update() 
    {
        Targetable target = targeter.GetTarget();
        
        if(target != null)
        {
            //Doing square roots in unity takes awhile and is bad in Update, so this just does a^2 + b^2 + c^2 = d^2(3 dimensional) and compares if > range^2
            if((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
            {
                agent.SetDestination(target.transform.position);
            }
            else if(agent.hasPath)
            {
                agent.ResetPath();
            }

            return;
        }
        
        //We need this because the agent calculates it path and as it does so its remainingDistance could be less than the stopping so it'll reset
        if(!agent.hasPath) { return; }
        
        if(agent.remainingDistance > agent.stoppingDistance) { return; }

        agent.ResetPath();
    }

    [Command]
    public void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }

    [Server]
    public void ServerMove(Vector3 position)
    {
        targeter.ClearTarget();
        
        //Really weird but I think it takes the position and checks if its valid(eg hits). The third number is wiggle room for if someone is a little bit off of
        //the NavMesh(The AI floor we have) so that its still allowed and the fourth is maybe the area to check which we just check it all. The out is given as the hit and
        //we can get the position it has(position could be different due to wiggle room which is why we use it)
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; } 
        agent.SetDestination(hit.position);
    }

    [Server]
    private void ServerHandleGameOver()
    {
        agent.ResetPath();
    }

    #endregion

    //Don't need, done in UnitCommandGiver but left here just in case

/*    #region Client

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        mainCamera = Camera.main;
    }

    [ClientCallback] //This is needed because unity calls update on everything, we just want it called on client
    private void Update()
    {
        if(!hasAuthority) { return; }

        if(!Mouse.current.rightButton.wasPressedThisFrame) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        //Infinity part is that the ray goes forever until it hits something. This if checks when it hits something, so something going forever will return false
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) { return; }

        CmdMove(hit.point);
    }

    #endregion*/
}
