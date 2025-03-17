using System.Collections.Generic;
using UnityEngine;

public interface ILayoutGroupParent<T> where T : ILayoutGroupChild
{
    void GetParent();
    void CreateChild(int count);
    void DeleteChild(int index);
    void RefreshChild(int index, T newChild);
    int GetChildCount();
}

public interface ILayoutGroupChild
{
    void SetParent<T>(ILayoutGroupParent<T> parent) where T : ILayoutGroupChild;
}

