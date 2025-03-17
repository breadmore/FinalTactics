using UnityEngine;

public abstract class BaseLayoutGroupChild<T> : MonoBehaviour, ILayoutGroupChild where T : ILayoutGroupChild
{
    protected ILayoutGroupParent<T> parent;

    public virtual void SetParent<U>(ILayoutGroupParent<U> newParent) where U : ILayoutGroupChild
    {
        parent = newParent as ILayoutGroupParent<T>;
    }

    protected virtual void Start()
    {

    }
}
