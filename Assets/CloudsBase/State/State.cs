public class State
{
    public delegate void StateEvent();
    public StateEvent OnEnter, OnExit, OnUpdate;
    public State ParentState;
    public State[] ChildrenStates;
    public State(params State[] childrenStates)
    {
        ChildrenStates = childrenStates;
        foreach (var ele in childrenStates)
        {
            ele.ParentState = this;
        }
    }
    public virtual void EnterState()
    {
        if (OnEnter != null) this.OnEnter();
    }
    public virtual void ExitState()
    {
        if (OnExit != null) this.OnExit();
    }
    public virtual void FrameUpdate()
    {
        if (OnUpdate != null) this.OnUpdate();
    }
}
