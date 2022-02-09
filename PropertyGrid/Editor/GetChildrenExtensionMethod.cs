using System.Collections.Generic;
using UnityEditor;

public static class GetChildrenExtensionMethod
{
    /// <summary>
    /// Extension method to enumerate through all children of a property.
    /// </summary>
    /// <param name="property"></param>
    /// <returns>Enumerable</returns>
    public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty property)
    {
        //Keep reference to start
        property = property.Copy();
        SerializedProperty nextElement = property.Copy();
        
        //MoveNext without looking for possible child properties
        bool hasNextElement = nextElement.NextVisible(false);
        if (!hasNextElement)
            nextElement = null;

        //Look for child properties
        property.NextVisible(true);
        while (true)
        {
            //If next is start you reached the end
            if ((SerializedProperty.EqualContents(property, nextElement)))
                yield break;

            //Else return start
            yield return property;

            //MoveNext without looking for child properties
            bool hasNext = property.NextVisible(false);

            //Go to beginning of loop
            if (!hasNext)
                break;
        }
    }
}
