using System.Collections.Generic;

public class Statemachine<T> where T : State
{
    public bool CanChange;
    public T CurState;
    public T[] listStates;
    public Statemachine(params T[] values)
    {
        listStates = values;
    }
    public virtual void Initialize(T state)
    {
        this.CanChange = true;
        this.CurState = state;
        this.CurState.EnterState();
    }
    public virtual void ChangeState(int index)
    {
        if (listStates[index] == CurState) return;
        if (!CanChange) return;
        ExitState(CurState);
        CurState = listStates[index];
        EnterState(CurState);
    }
    protected virtual void ExitState(T State)
    {
        Queue<T> StateTree = new Queue<T>();
        T state = State;
        while (state != null)
        {
            StateTree.Enqueue(state);
            state = (T)state.ParentState;
        }
        StateTree.Dequeue().ExitState();
    }
    protected virtual void EnterState(T State)
    {
        Stack<T> StateTree = new Stack<T>();
        T state = State;
        while (state != null)
        {
            StateTree.Push(state);
            state = (T)state.ParentState;
        }
        StateTree.Pop().EnterState();
    }
}
