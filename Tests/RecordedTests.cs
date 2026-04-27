using Mesen.Config;
using Mesen.Interop;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Tests;

[TestClass]
public sealed class RecordedTests
{
	private static HashSet<string> _ignoredFailure = new() {
		"/GB/GBEmulatorShootout/hacktix/bully_gbc.mtp",
		"/GB/GBEmulatorShootout/hacktix/bully.mtp",
		"/GB/GBEmulatorShootout/mooneye/acceptance/boot_div-dmgABCmgb.mtp",
		"/GB/GBEmulatorShootout/mooneye/acceptance/boot_hwio-dmgABCmgb.mtp",
		"/GB/GBEmulatorShootout/mooneye/acceptance/serial/boot_sclk_align-dmgABCmgb.mtp",
		"/GB/GBEmulatorShootout/mooneye/misc/boot_div-cgbABCDE.mtp",
		//"/NES/APU/apu_reset/len_ctrs_enabled_fail.mtp",
		"/NES/Input/read_joy3/count_errors_fast.mtp",
		"/NES/Input/read_joy3/count_errors.mtp",
		"/NES/Mappers/VRC2-4/vrctest23s1.mtp",
		"/NES/PPU/full_palette/flowing_palette.mtp",
		"/NES/PPU/full_palette/full_palette_smooth.mtp",
		"/NES/PPU/full_palette/full_palette.mtp",
		"/PCE/Greyscale_Test.mtp",
		"/PCE/testvblank-rcr-to-vblank.mtp",
		"/PCE/testvdc-rcr-to-start-of-next.mtp",
		"/SNES/blargg_2010-03-14/test_timer_speed_2.mtp",
		"/SNES/blargg_2010-03-14/test_timer_speed.mtp",
		"/SNES/blargg_2010-03-14/test_timer_speed2.mtp",
		"/SNES/test_ppu/test_noise.mtp",
		"/WS/internal.mtp"
	};

	private static string _homeFolder = "";

	[AssemblyInitialize()]
	public static void Initialize(TestContext context)
	{
		string folder = Directory.GetCurrentDirectory();
		_homeFolder = folder;
		Directory.SetCurrentDirectory(Path.Combine(folder.Substring(0, folder.LastIndexOf("Tests")), "bin", "win-x64", "Release"));
	}

	[TestMethod]
	public void RunRecordedTests()
	{
		string testFolder = ConfigManager.TestFolder;
		ConcurrentDictionary<string, RomTestResult> results = new();
		EmuApi.InitializeEmu(_homeFolder, IntPtr.Zero, IntPtr.Zero, false, true, true, true);

		List<string> testFiles = new();
		if(Directory.Exists(testFolder)) {
			testFiles = Directory.EnumerateFiles(testFolder, "*.mtp", SearchOption.AllDirectories).ToList();
		}
		if(testFiles.Count == 0) {
			testFolder = Path.Combine(_homeFolder, "Tests");
			if(Directory.Exists(testFolder)) {
				testFiles = Directory.EnumerateFiles(testFolder, "*.mtp", SearchOption.AllDirectories).ToList();
			}
		}

		if(testFiles.Count == 0) {
			Assert.Fail("No tests found");
		}

		Parallel.ForEach(testFiles, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount - 2 }, (string testFile) => {
			string entryName = testFile.Substring(testFolder.Length);
			if(entryName.Contains("GBA")) {
				return;
			}

			results[entryName] = TestApi.RunRecordedTest(testFile, true);
		});

		int failedTestCount = 0;
		List<string> entries = results.Keys.ToList();
		entries.Sort();

		StringBuilder sb = new();
		sb.AppendLine("==================");
		foreach(var entry in entries) {
			if(results[entry].State == RomTestState.Failed) {
				if(!_ignoredFailure.Contains(entry.Replace("\\", "/"))) {
					failedTestCount++;
					sb.AppendLine("  Failed: " + entry);
				}
			}
		}
		sb.AppendLine("==================");
		sb.AppendLine("Tests failed: " + failedTestCount);
		sb.AppendLine("==================");

		Assert.AreEqual(0, failedTestCount, message: sb.ToString());
	}
}
