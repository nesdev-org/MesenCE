using ELFSharp.ELF.Sections;
using Mesen.Config;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mesen.Integration.Elf;

public delegate ulong NormalizeAddressDelegate(ulong address);

public class DwarfSymbolProvider
{
	private ElfImage _elf;

	public IEnumerable<SymbolEntry<uint>> Symbols => _elf.PublicSymbols;

	private DwarfSymbolProvider(ElfImage elf)
	{
		_elf = elf;
		try {
			//var compilationUnits = ParseCompilationUnits(_elf.DebugData, _elf.DebugDataDescription, _elf.DebugDataStrings, _elf.NormalizeAddress);
			var lineNumberPrograms = ParseLineNumberPrograms(_elf.DebugLine, _elf.NormalizeAddress);
			//var commonInformationEntries = ParseCommonInformationEntries(_elf.DebugFrame, _elf.EhFrame, new DwarfExceptionHandlingFrameParsingInput(_elf));

			//var symbolList = compilationUnits.SelectMany((cu) => cu.Symbols).ToList();
			//var filteredSymbols = symbolList.Where(s => s.Tag == DwarfTag.Member || s.Tag == DwarfTag.Variable).ToList();
			//var filtered2 = symbolList.Where(s => s.Attributes.Values.Any(v => v.Type == DwarfAttributeValueType.Address && v.Address != 0)).ToList();
			/*if(compilationUnits.Length != 0 || lineNumberPrograms.Length != 0 || commonInformationEntries.Length != 0)
				return new DwarfSymbolProviderModule(location, module, compilationUnits, lineNumberPrograms, commonInformationEntries, _elf.PublicSymbols, _elf.CodeSegmentOffset, _elf.Is64bit);*/
		} catch {
		}
		//return null;
	}

	public static DwarfSymbolProvider? Load(string path)
	{
		if(File.Exists(path)) {
			return new DwarfSymbolProvider(new ElfImage(path));
		}
		return null;
	}

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
