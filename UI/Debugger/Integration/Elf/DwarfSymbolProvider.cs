using Mesen.Config;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mesen.Integration.Elf;

/// <summary>
/// Gets address offset within module when it is loaded.
/// </summary>
/// <param name="address">Virtual address that points where something should be loaded.</param>
public delegate ulong NormalizeAddressDelegate(ulong address);

/// <summary>
/// DWARF symbol provider that can be used with the <see cref="Context"/>.
/// </summary>
/// <seealso cref="SharpDebug.Engine.SymbolProviders.PerModuleSymbolProvider" />
public class DwarfSymbolProvider
{
	public void LoadModule(string path)
	{
		if(File.Exists(path))
			try {
				using(IDwarfImage image = new ElfImage(path, 0)) {
					var compilationUnits = ParseCompilationUnits(image.DebugData, image.DebugDataDescription, image.DebugDataStrings, image.NormalizeAddress);
					var lineNumberPrograms = ParseLineNumberPrograms(image.DebugLine, image.NormalizeAddress);
					var commonInformationEntries = ParseCommonInformationEntries(image.DebugFrame, image.EhFrame, new DwarfExceptionHandlingFrameParsingInput(image));

					var symbolList = compilationUnits.SelectMany((cu) => cu.Symbols).ToList();
					var filteredSymbols = symbolList.Where(s => s.Tag == DwarfTag.Member || s.Tag == DwarfTag.Variable).ToList();

					/*if(compilationUnits.Length != 0 || lineNumberPrograms.Length != 0 || commonInformationEntries.Length != 0)
						return new DwarfSymbolProviderModule(location, module, compilationUnits, lineNumberPrograms, commonInformationEntries, image.PublicSymbols, image.CodeSegmentOffset, image.Is64bit);*/
				}
			} catch {
			}
		//return null;
	}

	/// <summary>
	/// Parses the compilation units.
	/// </summary>
	/// <param name="debugData">The debug data.</param>
	/// <param name="debugDataDescription">The debug data description.</param>
	/// <param name="debugStrings">The debug strings.</param>
	/// <param name="addressNormalizer">Normalize address delegate (<see cref="NormalizeAddressDelegate"/>)</param>
	private static DwarfCompilationUnit[] ParseCompilationUnits(byte[] debugData, byte[] debugDataDescription, byte[] debugStrings, NormalizeAddressDelegate addressNormalizer)
	{
		using(DwarfMemoryReader debugDataReader = new DwarfMemoryReader(debugData))
		using(DwarfMemoryReader debugDataDescriptionReader = new DwarfMemoryReader(debugDataDescription))
		using(DwarfMemoryReader debugStringsReader = new DwarfMemoryReader(debugStrings)) {
			List<DwarfCompilationUnit> compilationUnits = new List<DwarfCompilationUnit>();

			while(!debugDataReader.IsEnd) {
				DwarfCompilationUnit compilationUnit = new DwarfCompilationUnit(debugDataReader, debugDataDescriptionReader, debugStringsReader, addressNormalizer);

				compilationUnits.Add(compilationUnit);
			}

			return compilationUnits.ToArray();
		}
	}

	/// <summary>
	/// Parses the line number programs.
	/// </summary>
	/// <param name="debugLine">The debug line.</param>
	/// <param name="addressNormalizer">Normalize address delegate (<see cref="NormalizeAddressDelegate"/>)</param>
	private static DwarfLineNumberProgram[] ParseLineNumberPrograms(byte[] debugLine, NormalizeAddressDelegate addressNormalizer)
	{
		using(DwarfMemoryReader debugLineReader = new DwarfMemoryReader(debugLine)) {
			List<DwarfLineNumberProgram> programs = new List<DwarfLineNumberProgram>();

			while(!debugLineReader.IsEnd) {
				DwarfLineNumberProgram program = new DwarfLineNumberProgram(debugLineReader, addressNormalizer);

				programs.Add(program);
			}

			return programs.ToArray();
		}
	}

	/// <summary>
	/// Parses the common information entries.
	/// </summary>
	/// <param name="debugFrame">The debug frame.</param>
	/// <param name="ehFrame">The exception handling frames.</param>
	/// <param name="input">The input data for parsing configuration.</param>
	private static DwarfCommonInformationEntry[] ParseCommonInformationEntries(byte[] debugFrame, byte[] ehFrame, DwarfExceptionHandlingFrameParsingInput input)
	{
		List<DwarfCommonInformationEntry> entries = new List<DwarfCommonInformationEntry>();

		using(DwarfMemoryReader debugFrameReader = new DwarfMemoryReader(debugFrame)) {
			entries.AddRange(DwarfCommonInformationEntry.ParseAll(debugFrameReader, input.DefaultAddressSize));
		}

		using(DwarfMemoryReader ehFrameReader = new DwarfMemoryReader(ehFrame)) {
			entries.AddRange(DwarfExceptionHandlingCommonInformationEntry.ParseAll(ehFrameReader, input));
		}

		return entries.ToArray();
	}
}
