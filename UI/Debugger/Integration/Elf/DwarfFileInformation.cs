using System.Collections.Generic;

namespace Mesen.Integration.Elf;

/// <summary>
/// File metadata with line information
/// </summary>
internal class DwarfFileInformation
{
	/// <summary>
	/// Gets or sets the file name.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the directory.
	/// </summary>
	public string Directory { get; set; }

	/// <summary>
	/// Gets or sets the path.
	/// </summary>
	public string Path { get; set; }

	/// <summary>
	/// Gets or sets the last modification.
	/// </summary>
	public uint LastModification { get; set; }

	/// <summary>
	/// Gets or sets the length.
	/// </summary>
	public uint Length { get; set; }

	/// <summary>
	/// Gets or sets the lines information.
	/// </summary>
	public List<DwarfLineInformation> Lines { get; set; } = new List<DwarfLineInformation>();

	/// <summary>
	/// Returns a <see cref="System.String" /> that represents this instance.
	/// </summary>
	/// <returns>
	/// A <see cref="System.String" /> that represents this instance.
	/// </returns>
	public override string ToString()
	{
		return Name;
	}
}
