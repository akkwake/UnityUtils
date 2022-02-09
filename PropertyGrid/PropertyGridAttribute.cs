using UnityEngine;

public class PropertyGridAttribute : PropertyAttribute
{
	public readonly string propertyName;
	public readonly int size = -1;
	public readonly float width;
	public readonly bool drawHeader;

	/// <summary>
	/// Sets the size of the nested arrays to the value of `propertyName`. `propertyName` needs to correspond to an integer.
	/// </summary>
	/// <param name="propName"></param>
	/// <param name="drawHeader"></param>
    public PropertyGridAttribute(string propertyName, float labelWidth = 120f, bool drawHeader = true)
    {
        this.propertyName = propertyName;
		this.width = labelWidth;
		this.drawHeader = drawHeader;
    }

	/// <summary>
	/// Sets the size of the nested arrays and draws them in a grid.
	/// </summary>
	/// <param name="size"></param>
	/// <param name="drawHeader"></param>
	public PropertyGridAttribute(int size, float labelWidth = 120f, bool drawHeader = true)
	{
		this.size = size;
		this.width = labelWidth;
		this.drawHeader = drawHeader;
	}
}