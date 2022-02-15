# DropdownList `<T>` : ReorderableList
Extension for reorderable lists to work with abstract types. 
Clicking the add element button shows a dropdown menu where you can select any class that inherits `<T>`, 
and add it to the list.



## Usage
You don't need to set this up like you would a ReorderableList. It automatically creates the list, 
the same way the default inspector does. All it does is add the dropdown.
#### Constructor
```csharp
public DropdownList<T>(SerializedObject serializedObject, SerializedProperty property, string[] ignoreTags = null)
```

## Example
```csharp
[CustomEditor(typeof(Spell))]
class SpellEditor : Editor
{
    DropdownList<Effect> effectList;

    public void OnEnable()
    {
        //Get the property
        SerializedProperty effects = serializedObject.FindProperty("effectList");

        //Create the DropdownList object
        effectList = new DropdownList<Effect>(serializedObject, effects);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        effectList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}
```
![dropdownlist](https://user-images.githubusercontent.com/22602865/154069333-48461b1d-6069-402d-b3c9-26f94ac95c14.png)

The items on the dropdown menu inherit `abstract class Effect` in this example.
## DropdownIgnoreTag Attribute
```csharp
[DropdownIgnoreTagAttribute(string tag)]
```
Allows you to choose which options will appear on the menu. Tag a class, and choose if you wanna exclude 
classes that have that tag when you make your DropdownList.