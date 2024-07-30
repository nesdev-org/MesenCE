using System.Collections.Generic;

namespace Mesen.Integration.Elf;

/// <summary>
/// DWARF symbol instance.
/// </summary>
internal class DwarfSymbol
{
	/// <summary>
	/// Gets or sets the symbol tag.
	/// </summary>
	public DwarfTag Tag { get; internal set; }

	/// <summary>
	/// Gets or sets the attributes.
	/// </summary>
	public IReadOnlyDictionary<DwarfAttribute, DwarfAttributeValue> Attributes { get; internal set; }

	/// <summary>
	/// Gets or sets the children.
	/// </summary>
	public List<DwarfSymbol> Children { get; internal set; }

	/// <summary>
	/// Gets or sets the parent.
	/// </summary>
	public DwarfSymbol Parent { get; internal set; }

	/// <summary>
	/// Gets or sets the offset.
	/// </summary>
	internal int Offset { get; set; }

	/// <summary>
	/// Gets the name.
	/// </summary>
	public string Name
	{
		get
		{
			DwarfAttributeValue nameValue;

			if(Attributes.TryGetValue(DwarfAttribute.Name, out nameValue)) {
				return nameValue.String;
			}

			return null;
		}
	}

	/// <summary>
	/// Gets the full name.
	/// </summary>
	public string FullName
	{
		get
		{
			string name = Name;

			if(Parent != null && Parent.Tag != DwarfTag.CompileUnit && name != null) {
				return Parent.FullName + "::" + name;
			}

			return name;
		}
	}

	/// <summary>
	/// Returns a <see cref="System.String" /> that represents this instance.
	/// </summary>
	/// <returns>
	/// A <see cref="System.String" /> that represents this instance.
	/// </returns>
	public override string ToString()
	{
		return $"{Tag} (Offset = {Offset}, Attributes = {Attributes.Count}, Children = {Children?.Count}";
	}

	/// <summary>
	/// Gets the constant attribute value if available.
	/// </summary>
	/// <param name="attribute">The attribute.</param>
	/// <param name="defaultValue">The default value if attribute is not available.</param>
	/// <returns>Attribute value if available; default value otherwise</returns>
	public ulong GetConstantAttribute(DwarfAttribute attribute, ulong defaultValue = 0)
	{
		DwarfAttributeValue value;

		if(Attributes.TryGetValue(attribute, out value)) {
			return value.Constant;
		}

		return defaultValue;
	}
}
