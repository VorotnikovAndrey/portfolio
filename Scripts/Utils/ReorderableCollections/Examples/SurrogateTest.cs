using UnityEngine;
using ZeroVector.Common.Reorderable;

public class SurrogateTest : MonoBehaviour {

	[SerializeField]
	private MyClass[] objects;

	[SerializeField, Reorderable(surrogateType = typeof(GameObject), surrogateProperty = "gameObject")]
	private MyClassList myClassList;

	[System.Serializable]
	public class MyClass {

		public string name;
		public GameObject gameObject;
	}

	[System.Serializable]
	public class MyClassList : ReorderableList<MyClass> {
	}
}
