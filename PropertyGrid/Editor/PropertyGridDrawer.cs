using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PropertyGridAttribute))]
public class PropertyGridDrawer : PropertyDrawer
{
	float propertyHeight;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		//Get the attribute
		PropertyGridAttribute gridattribute = (PropertyGridAttribute)attribute;

		int size;
		//Check if attribute was initialized with a set size or a property name
		if (gridattribute.propertyName != null)
		{
			//Find the property
			SerializedProperty sizeProperty = property.serializedObject.FindProperty(gridattribute.propertyName);
			
			//Show error if property given is not an integer
			if (sizeProperty.propertyType != SerializedPropertyType.Integer)
			{
				Debug.LogError("PropertyGridDrawer Error. Given property needs to be an integer.");
				base.OnGUI(position, property, label);	//This doesn't work for some reason.
				return;
			}
			else
				//Get the size from the property
				size = sizeProperty.intValue;
		}
		else
			//Get the size from the attribute
			size = gridattribute.size;


		EditorGUI.BeginProperty(position, label, property);

		//Create the grid and add all nested properties
		PropertyGrid grid = new PropertyGrid(position, size, gridattribute.drawHeader, property.name, gridattribute.width);
		IEnumerable<SerializedProperty> children = property.GetChildren();
		foreach (SerializedProperty child in children)
		{
			grid.AddProperty(child);
		}

		//Calculate property height based on grid height
		EditorGUI.EndProperty();
		propertyHeight = grid.gridHeight;
	}



	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
		//If something goes wrong, use default height
		if (propertyHeight == 0)
			return base.GetPropertyHeight(property, label);
		else
			return propertyHeight;
    }
}