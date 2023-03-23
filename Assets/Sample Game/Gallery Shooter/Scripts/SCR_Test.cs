using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface TestInterface<T>
{
    void TestFunction(T test);
}

[System.Serializable]
public class ParentClass
{
    //nothing
}

public class ChildClassOne: ParentClass
{
    public int testOne;
}

public class ChildClassTwo: ParentClass
{
    public int testTwo;
}

public class SCR_Test : MonoBehaviour, TestInterface<ParentClass> {

    List<ParentClass> testList = new List<ParentClass>();

    public GameObject TestObject;

	// Use this for initialization
	void Start ()
    {
        ChildClassOne test1 = new ChildClassOne();
        test1.testOne = 1;

        ChildClassTwo test2 = new ChildClassTwo();
        test2.testTwo = 2;

        testList.Add(test1);
        testList.Add(test2);

        TestFunction(testList[0]);
        TestObject.GetComponent<TestInterface<ParentClass>>().TestFunction(testList[1]);

        string numberString = "10345";

        int conversion;
        int.TryParse(numberString, out conversion);

        conversion += 100;

        print(conversion);
    }

    public void TestFunction(ParentClass test)
    {
        ChildClassOne testChild = test as ChildClassOne;
        int one = testChild.testOne;
        print(one);
    }


    // Update is called once per frame
    void Update () {
		
	}
}
