using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PropertyGrid
{
    //Minimum allowed width for columns
    public const float minimumWidth = 25f;
    //Space between columns
    public const float fieldSpacing = 0f;
    //Space for labels
    public readonly float prefixWidth = 120f;

    // Required to get properly drawn reorderable lists
    public int LineCount { get; private set; }
    
    // Required to get properly drawn on property drawers
    public float gridHeight
    {
        get { return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * LineCount;  }
    }

    readonly int size;
    Rect[] gridColumns;


    /// <summary>
    /// Use this in properties confined to a rect. Grids on ReorderableLists or PropertyDrawers should use this constructor.
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="size"></param>
    /// <param name="drawHeader"></param>
    /// <param name="headerName"></param>
    public PropertyGrid(Rect rect, int size, bool drawHeader, string headerName, float labelWidth = 120f)
    {
        this.size = size < 1 ? 1 : size;
        this.prefixWidth = labelWidth;

        InitializeGrid(rect);
        if (drawHeader)
            CreateHeaderConfined(headerName);
    }

    /// <summary>
    /// Use this in properties drawn on OnInspectorGUI. Grids on Custom Editors should use this constructor.
    /// </summary>
    /// <param name="size">Number of columns on the grid.</param>
    /// <param name="drawHeader"></param>
    /// <param name="headerName"></param>
    public PropertyGrid(int size, bool drawHeader, string headerName, float labelWidth = 120f)
    {
        this.size = size < 1 ? 1 : size;
        this.prefixWidth = labelWidth;

        if (drawHeader)
            CreateHeaderUnconfined(headerName);
    }

    /// <summary>
    /// Adds a property to the grid. It needs to be an array and is automatically resized to fit the grid.
    /// </summary>
    /// <param name="serializedProperty"></param>
    /// <returns></returns>
    public bool AddProperty(SerializedProperty serializedProperty)
    {
        if (!isSerializedPropertyValid(serializedProperty))
            return false;

		//Resize the array to the size of the grid
        ResizeArray(serializedProperty, this.size);
		
		//Check which constructor was used
        if (gridColumns == null)
            AddPropertyUnconfined(serializedProperty);
        else
            AddPropertyConfined(serializedProperty);

        return true;
    }

    /// <summary>
    /// Separates rect into columns. Property fields will then be drawn on these Rects.
    /// </summary>
    /// <param name="rect"></param>
    void InitializeGrid(Rect rect)
    {
        gridColumns = new Rect[size];
        rect.x = rect.x + prefixWidth;

        //Calculate the width of each column
        float columnWidth = (rect.width / size) - (prefixWidth / size);
		
		//Make sure it doesn't get below minimumWidth
        columnWidth = Mathf.Max(minimumWidth, columnWidth);
		
		//Populate the gridColumns array
        for (int i = 0; i < size; i++)
        {
            Rect r = new Rect(rect.x + (columnWidth * i) , rect.y, columnWidth - fieldSpacing, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            gridColumns[i] = r;
        }
    }



	
    void AddPropertyUnconfined(SerializedProperty serializedProperty)
    {
        EditorGUILayout.BeginHorizontal();

		//Check if property has children
        if (serializedProperty.hasChildren || serializedProperty.Copy().CountInProperty() == 1)
            EditorGUILayout.LabelField(serializedProperty.displayName, GUILayout.Width(prefixWidth));

		//Child properties will be stored here
        Dictionary<string, List<SerializedProperty>> propDict = new Dictionary<string, List<SerializedProperty>>();
        for (int i = 0; i < size; i++)
        {
            if (i >= serializedProperty.arraySize)
                continue;

			//Get array element
            SerializedProperty elementProperty = serializedProperty.GetArrayElementAtIndex(i);
            
            //Necessary for the fields to show
			elementProperty.isExpanded = true;
			
            if (!elementProperty.hasVisibleChildren || serializedProperty.Copy().CountInProperty() == 1)
				//Create the field if it doesn't have children
                EditorGUILayout.PropertyField(elementProperty, GUIContent.none, GUILayout.MinWidth(minimumWidth));
            else
				//Recursively find children and add them to dictionary
                GetChildProperties(elementProperty, ref propDict);
        }
        EditorGUILayout.EndHorizontal();

		//Loop through child properties
        foreach (KeyValuePair<string, List<SerializedProperty>> propItem in propDict)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(propItem.Key, GUILayout.Width(prefixWidth));
            for (int i = 0; i < propItem.Value.Count; i++)
            {
				//Create fields for child properties
                EditorGUILayout.PropertyField(propItem.Value[i], GUIContent.none, GUILayout.MinWidth(minimumWidth));
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    void AddPropertyConfined(SerializedProperty serializedProperty)
    {
        Rect prefixLabelRect = GetLabelRectForLine();
		
		//Child properties will be stored here
        Dictionary<string, List<SerializedProperty>> propDict = new Dictionary<string, List<SerializedProperty>>();
        List<SerializedProperty> arrayProps = new List<SerializedProperty>();

        //Create label at prefix column
        if (serializedProperty.hasChildren)
            EditorGUI.LabelField(prefixLabelRect, serializedProperty.displayName);

        for (int i = 0; i < size; i++)
        {
			//Get array element
            SerializedProperty elementProperty = serializedProperty.GetArrayElementAtIndex(i);
            
            //Sometimes necessary for the fields to show
            elementProperty.isExpanded = true;

            if (!elementProperty.hasVisibleChildren)
                //Create the field if it doesn't have children
                EditorGUI.PropertyField(gridColumns[i], elementProperty, GUIContent.none);
            else
				//Recursively find children and add them to dictionary
                GetChildProperties(elementProperty, ref propDict);
        }
		
		//Move the grid columns down one line
        NextLine();

		//Loop through child properties
        foreach (KeyValuePair<string, List<SerializedProperty>> propItem in propDict)
        {
            //Create the label
            prefixLabelRect = GetLabelRectForLine();
            EditorGUI.LabelField(prefixLabelRect, propItem.Key);
            
            //Draw fields for child properties found
            for (int i = 0; i < propItem.Value.Count; i++)
            {
                EditorGUI.PropertyField(gridColumns[i], propItem.Value[i], GUIContent.none);
            }
            NextLine();
        }
    }

    /// <summary>
    /// The area in a line where the label will be drawn. Needs to be renamed.
    /// </summary>
    /// <returns></returns>
    Rect GetLabelRectForLine()
    {
        Rect labelRect = gridColumns[0];
        labelRect.x -= prefixWidth;
        labelRect.width = prefixWidth;
        return labelRect;
    }

    /// <summary>
    /// Recursively gets all child properties. I made it ignore arrays in an effort to retain my sanity.
    /// </summary>
    /// <param name="serializedProperty"></param>
    /// <param name="propertyDict"></param>
    void GetChildProperties(SerializedProperty serializedProperty, ref Dictionary<string, List<SerializedProperty>> propertyDict)
    {
        if (serializedProperty.hasVisibleChildren && !serializedProperty.isArray)
        {
            //Store child properties in a list
            List<SerializedProperty> propList;
            IEnumerable<SerializedProperty> children = serializedProperty.GetChildren();
            foreach (SerializedProperty child in children)
            {
                if (!child.hasVisibleChildren)
                {
                    //This is what will be shown on the label
                    string name = serializedProperty.name.Equals("data") ? name = "•  " + child.name : "•  " + serializedProperty.name + '.' + child.name;
                    
                    if (propertyDict.TryGetValue(name, out propList))
                    {
                        //If the string is already found on the array, add it to the list
                        propList.Add(child.Copy());
                    }
                    else
                    {
                        //Else create a new list and add to dictionary
                        propList = new List<SerializedProperty>();
                        propList.Add(child.Copy());
                        propertyDict.Add(name, propList);
                    }
                }
                else
                    //Recursion
                    GetChildProperties(child, ref propertyDict);
            }
        }
    }

    /// <summary>
    /// Moves all columns down one line.
    /// </summary>
    void NextLine()
    {
        for (int i = 0; i < gridColumns.Length; i++)
        {
            gridColumns[i].y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
        LineCount++;
    }

    /// <summary>
    /// Creates the header for the grid.
    /// </summary>
    /// <param name="header"></param>
    void CreateHeaderUnconfined(string header)
    {
        EditorGUILayout.BeginHorizontal();
        //Eye candy
        GUIStyle rankLabelStyle = new GUIStyle() { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
        GUIStyle rankCountStyle = new GUIStyle(EditorStyles.helpBox) { alignment = TextAnchor.MiddleCenter };
        //Prefix label
        EditorGUILayout.LabelField(header + ':', rankLabelStyle, GUILayout.Width(prefixWidth));
        for (int i = 0; i < this.size; i++)
        {
            //Rank labels
            EditorGUILayout.LabelField(i.ToString(), rankCountStyle /*new GUIStyle {alignment = TextAnchor.MiddleCenter}*/, GUILayout.MinWidth(minimumWidth));
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Creates the header for the grid.
    /// </summary>
    /// <param name="header"></param>
    void CreateHeaderConfined(string header)
    {
        GUIStyle rankCountStyle = new GUIStyle(EditorStyles.helpBox) { alignment = TextAnchor.MiddleCenter };

        Rect rect = GetLabelRectForLine();

        EditorGUI.LabelField(rect, header + ':', new GUIStyle() { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft });
        UnderlineRect(rect);

        for (int i = 0; i < gridColumns.Length; i++)
        {
            EditorGUI.LabelField(gridColumns[i], i.ToString(), rankCountStyle);
        }
        NextLine();
    }

    /// <summary>
    /// Underlines a rect. Kind of.
    /// </summary>
    /// <param name="rect"></param>
    void UnderlineRect(Rect rect)
    {
        rect.y += EditorGUIUtility.singleLineHeight;
        rect.height -= EditorGUIUtility.singleLineHeight + 2;
        rect.width = 35f;
        EditorGUI.DrawRect(rect, Color.black);
    }

    bool isSerializedPropertyValid(SerializedProperty serializedProperty)
    {
        if (serializedProperty == null)
        {
            Debug.LogError("PropertyGrid: Property not found. Check property name and make sure it's serializable and serialized.");
            return false;
        }
        if (!serializedProperty.isArray)
        {
            Debug.LogError("PropertyGrid: Property '" + serializedProperty.name + "' is not an array. It cannot be added to a grid.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Resizes the array to the given length.
    /// </summary>
    /// <param name="serializedProperty"></param>
    /// <param name="newLength"></param>
    void ResizeArray(SerializedProperty serializedProperty, int newLength)
    {
        if (serializedProperty == null) return;

        if (serializedProperty.arraySize != newLength)
        {
            serializedProperty.arraySize = newLength;
        }
    }
}