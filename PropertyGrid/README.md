# PropertyGridAttribute
Use either of the following Attributes to instantly create a grid for your array(s).

```csharp
// The grid will have the size of the property you named.
[PropertyGrid(string propertyName, float labelWidth, bool drawHeader)]

// The grid will have the size you set.
[PropertyGrid(int size, float labelWidth, bool drawHeader)]
```

# Example using PropertyDrawers
```csharp
public class Spell : MonoBehaviour
{
    [SerializeField] int spellRanks;

    [PropertyGrid("spellRanks")]  //<-- This is all you need
    [SerializeField] SpellData spellData;
}

[Serializable]
public class SpellData
{
    [SerializeField] int[] Power;
    [SerializeField] float[] Duration;
    [SerializeField] SecondaryData[] secondaryData;
}

[Serializable]
public class SecondaryData
{
    [SerializeField] int Range;
    [SerializeField] NestedData Nested;
}

[Serializable]
public class NestedData
{
    [SerializeField] string Description;
}
```
The above code looks like this: <br>
![image](https://user-images.githubusercontent.com/22602865/153257310-5c0b170f-cd9b-476d-941b-180bb4dbedf8.png)

Changing the `gridSize` property automatically adjusts the grid and arrays.

Unnecessarily complicated but I wanted to demonstrate ease of use and versatility.

# PropertyDrawer Limitations
Unfortunately, given the way PropertyDrawers work, you need to use a wrapper class for the arrays you want in the grid.
Note that you CAN have arrays for the wrapper class (declaring `SpellData[]` in the above example works perfectly).

You can however use the `PropertyGrid` class in custom editors if you want to avoid using a wrapper, since you can override the way Unity draws lists completely.

# PropertyGrid
There are 2 constructors. One can be used on the `OnInspectorGUI()` method `(custom editors)`, the other one on the `OnGUI()` method
`(ReorderableLists and PropertyDrawers)`.

```csharp
// Use this in properties confined to a rect. Grids on ReorderableLists or PropertyDrawers should use this constructor.
public PropertyGrid(Rect rect, int size, bool drawHeader, string headerName, float labelWidth = 120f)
```

```csharp
// Use this in properties drawn on OnInspectorGUI. Grids on Custom Editors should use this constructor.
public PropertyGrid(int size, bool drawHeader, string headerName, float labelWidth = 120f)
```

## Public Methods
```csharp
// Adds a property to the grid. As long as it's an array it's automatically resized and drawn on the grid.
public bool AddProperty(SerializedProperty serializedProperty)
```


# Custom Editor example
```csharp
...
int gridSize;
int[] intArray;
...

public override void OnInspectorGUI()
{
    ...
    // Find the property for your size
    SerializedProperty gridSizeProp = serializedObject.FindProperty("gridSize");
    int size = gridSizeProp.IntValue;
    
    // Find the property for your array
    SerializedProperty intArrayProp = serializedObject.FindProperty("intArray");

    // Create a grid object
    PropertyGrid propGrid = new PropertyGrid(size, true, "Ranks");
    
    //Add the property to the grid
    propGrid.AddProperty(intArrayProp);
    
    //Profit
    ...
}
```
