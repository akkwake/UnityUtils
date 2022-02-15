using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// Extension for reorderable lists to work with abstract types. 
/// Clicking the add button shows a dropdown menu where you can select any class that isn't abstract
/// and inherits <typeparamref name="T"/>, and add it to the list.
/// </summary>
/// <typeparam name="T"></typeparam>
public class DropdownList<T> : ReorderableList
{
	//Types that will be shown on the dropdown
	HashSet<Type> allowedTypes = new HashSet<Type>();

	//Line count for each element; required for the list to have the correct height
	int[] elementLines;

	/// <summary>
	/// Provides a dropdown that allows you to add any class inheriting <typeparamref name="T"/> to the list. Optionally ignore classes with a specific tag (SEE: [DropdownIgnoreTag] attribute).
	/// </summary>
	/// <param name="serializedObject"></param>
	/// <param name="elements"></param>
	/// <param name="ignoreTags"></param>
	public DropdownList(SerializedObject serializedObject, SerializedProperty elements, string[] ignoreTags = null) : base(serializedObject, elements, true, true, true, true)
	{
		GetAllowedTypes(ignoreTags);

		//Set up callbacks
		this.onAddDropdownCallback = OnAddDropdownCallback;
		this.drawElementCallback = OnDrawElementCallback;
		this.elementHeightCallback = ElementHeightCallback;
		this.drawHeaderCallback = OnDrawHeaderCallback;
		this.onReorderCallbackWithDetails = OnReorderCallbackWithDetails;

		//Initialize the elementLines array
		elementLines = new int[elements.arraySize];
	}

	/// <summary>
	/// Returns all allowed types that will be shown in the dropdown.
	/// </summary>
	/// <param name="tags"></param>
	protected void GetAllowedTypes(string[] tags)
    {
		Type type = typeof(T);

		//Get all types in assembly
		Type[] allTypes = type.Assembly.GetTypes();

		//Loop through them
		foreach (Type t in allTypes)
			//Find all types that inherit T and aren't abstract
			if (t.IsSubclassOf(type) && !t.IsAbstract)
				//Ignore classes that have any of the tags
				if (FilterTypeByTags(t, tags))
					//Add them to allowedTypes
					allowedTypes.Add(t);
	}

	/// <summary>
	/// Ignore the type if tags are matching.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="tags"></param>
	/// <returns></returns>
	protected bool FilterTypeByTags(Type type, string[] tags)
	{
		if (tags == null)
			return true;

		//Get all attributes on type
		object[] attributes = type.GetCustomAttributes(true);
		
		//Check if type has an IgnoreTag
		foreach (var attribute in attributes)
			if (attribute is DropdownIgnoreTagAttribute)
				//Check if tags are matching
				foreach (string tag in tags)
				if (((DropdownIgnoreTagAttribute)attribute).tag == tag)
					return false;
		
		return true;
	}
	


	#region Callbacks
	protected void OnDrawHeaderCallback(Rect rect)
	{
		EditorGUI.LabelField(rect, serializedProperty.displayName);
	}
	

	protected void OnDrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
	{
		rect.y += 4f;
		SerializedProperty element = serializedProperty.GetArrayElementAtIndex(index);

		//Draw the element header
		float labelWidth = rect.width - 15;
		string typeName = GetDisplayNameForType(element.type);
		Rect rectLabel = new Rect(rect.x + 15, rect.y - 2, labelWidth, EditorGUIUtility.singleLineHeight + 2);
		GUIStyle style = new GUIStyle(EditorStyles.helpBox) { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
		EditorGUI.LabelField(rectLabel, typeName, style);

		//Draw the property
		EditorGUI.PropertyField(rect, element, GUIContent.none, true);

		//Check if property foldout is expanded and count the lines for the element
		if (element.isExpanded)
			elementLines[index] = element.Copy().CountInProperty();
		else
			elementLines[index] = 1;
	}

	/// <summary>
	/// Switches the indexes.
	/// </summary>
	/// <param name="list"></param>
	/// <param name="oldIndex"></param>
	/// <param name="newIndex"></param>
	protected void OnReorderCallbackWithDetails(ReorderableList list, int oldIndex, int newIndex)
	{
		int index = oldIndex;
		elementLines[oldIndex] = newIndex;
		elementLines[newIndex] = index;
	}

	/// <summary>
	/// Creates the dropdown button.
	/// </summary>
	/// <param name="buttonrect"></param>
	/// <param name="list"></param>
	protected void OnAddDropdownCallback(Rect buttonrect, ReorderableList list)
	{
		if (allowedTypes == null || allowedTypes.Count == 0)
		{
			Debug.Log("No allowed types found.");
			return;
		}

		//Create the menu
		GenericMenu menu = new GenericMenu();
		
		//Add a menu item for each allowed type
		foreach (Type type in allowedTypes) 
		{
			menu.AddItem(new GUIContent(type.Name), false, 
						clickHandler,	//Runs when you click the item on the menu
						Activator.CreateInstance(type)	//Create an object of they type you selected
						);
		}

		//Show menu
		menu.ShowAsContext();
	}

	/// <summary>
	/// Returns the correct height for each element
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	float ElementHeightCallback(int index)
	{
		return (EditorGUIUtility.singleLineHeight + 2f) * elementLines[index] + 2f;
	}
	#endregion

	/// <summary>
	/// Called when clicking at item in the dropdown menu.
	/// </summary>
	/// <param name="target"></param>
	void clickHandler(object target)
	{
		//Index of the new item
		int index = serializedProperty.arraySize;
		
		//Increase array size
		serializedProperty.arraySize++;
		
		//Resize the elementLines array
		Array.Resize<int>(ref elementLines, index + 1);
		//Set the value of the element to the created object
		SerializedProperty element = serializedProperty.GetArrayElementAtIndex(index);
		//Type t = element.GetType();
		element.managedReferenceValue = (T)target;

		serializedProperty.serializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// Returns the name of the Effect.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	string GetDisplayNameForType(string type)
	{
		string name = type;
		int i = name.IndexOf('<') + 1;
		name = name.Substring(i, name.Length - i).Trim('>');
		return name;
	}
}
