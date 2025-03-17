using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LayoutGroupParent : BaseLayoutGroupParent<LayoutGroupChild>
{
    private void Start()
    {
        CreateChild(3); // 예제: 시작 시 3개의 자식 생성
    }

}
