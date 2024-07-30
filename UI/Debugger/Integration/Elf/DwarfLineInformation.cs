namespace Mesen.Integration.Elf;

/// <summary>
/// Information about line containing compiled code.
/// </summary>
internal class DwarfLineInformation
{
	/// <summary>
	/// Gets or sets the file information.
	/// </summary>
	public DwarfFileInformation File { get; set; }

	/// <summary>
	/// Gets or sets the relative module address.
	/// </summary>
	public uint Address { get; set; }

	/// <summary>
	/// Gets or sets the line.
	/// </summary>
	public uint Line { get; set; }

	/// <summary>
	/// Gets or sets the column.
	/// </summary>
	public uint Column { get; set; }
}
