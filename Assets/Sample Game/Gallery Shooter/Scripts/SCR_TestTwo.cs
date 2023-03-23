using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_TestTwo : MonoBehaviour, TestInterface<ParentClass> {

	public void TestFunction(ParentClass testClass)
    {
        ChildClassTwo test = testClass as ChildClassTwo;
        print(test.testTwo);
    }
}
