using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LayoutGroupParent : BaseLayoutGroupParent<LayoutGroupChild>
{
    private void Start()
    {
        CreateChild(3); // ����: ���� �� 3���� �ڽ� ����
    }

}
