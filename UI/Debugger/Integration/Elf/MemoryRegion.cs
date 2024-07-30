using System;
using System.Collections.Generic;
using System.Linq;

namespace Mesen.Integration.Elf;

/// <summary>
/// Region of memory that application is using.
/// </summary>
public struct MemoryRegion
{
	/// <summary>
	/// The base address
	/// </summary>
	public ulong BaseAddress;

	/// <summary>
	/// The region size
	/// </summary>
	public ulong RegionSize;

	/// <summary>
	/// Gets the start of the memory region.
	/// </summary>
	internal ulong MemoryStart
	{
		get
		{
			return BaseAddress;
		}
	}

	/// <summary>
	/// Gets or sets the end of the memory region.
	/// </summary>
	internal ulong MemoryEnd
	{
		get
		{
			return BaseAddress + RegionSize;
		}

		set
		{
			RegionSize = value - BaseAddress;
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
		return $"0x{MemoryStart:X} - 0x{MemoryEnd:X}";
	}
}

/// <summary>
/// Helper class that is doing searches over memory regions to find where address is located.
/// </summary>
internal class MemoryRegionFinder
{
	/// <summary>
	/// The tries bucket size in bits
	/// </summary>
	internal const int BucketSizeBits = 8;

	private ulong triesStartMask;
	private int triesStartBits;

	/// <summary>
	/// The initial tries buckets
	/// </summary>
	private TriesElement[] buckets;

	/// <summary>
	/// Initializes a new instance of the <see cref="MemoryRegionFinder"/> class.
	/// </summary>
	/// <param name="regions">The memory regions.</param>
	public MemoryRegionFinder(IReadOnlyList<MemoryRegion> regions)
	{
		if(regions.Count > 0) {
			ulong minValue = regions[0].MemoryStart;
			ulong maxValue = regions[regions.Count - 1].MemoryEnd;

			triesStartBits = 64 - BucketSizeBits;
			triesStartMask = ((1UL << BucketSizeBits) - 1) << triesStartBits;
			while((triesStartMask & (maxValue - 1)) == 0) {
				triesStartMask >>= BucketSizeBits;
				triesStartBits -= BucketSizeBits;
			}

			Tuple<int, MemoryRegion>[] regionsTuple = new Tuple<int, MemoryRegion>[regions.Count];
			for(int i = 0; i < regionsTuple.Length; i++)
				regionsTuple[i] = Tuple.Create(i, regions[i]);
			TriesElement element = new TriesElement(regionsTuple, triesStartMask, triesStartBits);

			buckets = element.buckets;
			if(buckets == null) {
				buckets = new TriesElement[] { element };
			}
		} else {
			buckets = new TriesElement[0];
		}
	}

	/// <summary>
	/// Finds the index of memory region where the specified address is located or -1 if not found.
	/// </summary>
	/// <param name="address">The address.</param>
	/// <returns>The index of memory region where the specified address is located or -1 if not found.</returns>
	public int Find(ulong address)
	{
		ulong mask = triesStartMask;
		int offset = triesStartBits;
		ulong bucketIndex = (address & mask) >> offset;
		if(bucketIndex < (ulong)buckets.LongLength) {
			TriesElement bucket = buckets[bucketIndex];

			while(bucket != null && bucket.buckets != null) {
				mask >>= BucketSizeBits;
				offset -= BucketSizeBits;
				bucketIndex = (address & mask) >> offset;
				bucket = bucket.buckets[bucketIndex];
			}

			if(bucket != null) {
				return bucket.location;
			}
		}

		return -1;
	}

	/// <summary>
	/// Tries element for searching memory regions
	/// </summary>
	private class TriesElement
	{
		/// <summary>
		/// The buckets inside current element
		/// </summary>
		public TriesElement[] buckets;

		/// <summary>
		/// The index of this element inside the memory regions array
		/// </summary>
		public int location;

		public TriesElement(IReadOnlyList<Tuple<int, MemoryRegion>> regions, ulong triesStartMask, int triesStartBits, ulong minValue = 0, ulong maxValue = ulong.MaxValue)
		{
			if(regions.Count > 1) {
				bool same = true;
				for(int i = 1; i < regions.Count && same; i++)
					same = regions[0].Item2.BaseAddress == regions[i].Item2.BaseAddress;
				if(same) {
					location = regions.OrderByDescending(r => r.Item2.MemoryEnd).First().Item1;
				} else {
					var division = new List<Tuple<int, MemoryRegion>>[1 << BucketSizeBits];

					foreach(var region in regions) {
						ulong bucketStart = (Math.Max(minValue, region.Item2.MemoryStart) & triesStartMask) >> triesStartBits;
						ulong bucketEnd = (Math.Min(maxValue, region.Item2.MemoryEnd - 1) & triesStartMask) >> triesStartBits;

						for(ulong j = bucketStart; j <= bucketEnd; j++) {
							if(division[j] == null)
								division[j] = new List<Tuple<int, MemoryRegion>>();
							division[j].Add(region);
						}
					}

					buckets = new TriesElement[1 << BucketSizeBits];
					for(int i = 0; i < 1 << BucketSizeBits; i++)
						if(division[i] != null)
							buckets[i] = new TriesElement(division[i], triesStartMask >> BucketSizeBits, triesStartBits - BucketSizeBits, minValue | ((ulong)i << triesStartBits), minValue | (((ulong)i + 1) << triesStartBits) - 1);
				}
			} else {
				location = regions[0].Item1;
			}
		}
	}
}
