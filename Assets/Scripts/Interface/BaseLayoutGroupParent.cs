using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseLayoutGroupParent<T> : MonoBehaviour, ILayoutGroupParent<T> where T : MonoBehaviour, ILayoutGroupChild
{
    [SerializeField] protected T childPrefab;
    protected List<T> childList = new List<T>();

    public virtual void GetParent()
    {
        
    }
    public virtual void CreateChild(int count)
    {
        for (int i = 0; i < count; i++)
        {
            T child = Instantiate(childPrefab, transform);
            child.SetParent(this);
            childList.Add(child);
        }
    }

    public virtual void DeleteChild(int index)
    {
        if (index >= 0 && index < childList.Count)
        {
            Destroy(childList[index].gameObject);
            childList.RemoveAt(index);
        }
    }

    public virtual void RefreshChild(int index, T newChild)
    {
        if (index >= 0 && index < childList.Count)
        {
            Destroy(childList[index].gameObject);
            childList[index] = newChild;

            newChild.SetParent(this);
        }
    }

    public int GetChildCount() => childList.Count;
}
