using System;

public class DropdownIgnoreTagAttribute : Attribute
{
	public readonly string tag;

	/// <summary>
	/// Allows Dropdown Lists to ignore classes with this tag.
	/// </summary>
	/// <param name="tag"></param>
	public DropdownIgnoreTagAttribute(string tag)
	{
		this.tag = tag;
	}
}