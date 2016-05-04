using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace elbgb_utilities
{
    public sealed /*partial*/ class Profiler
    {
		public class AutoTimedBlock : IDisposable
		{
			public AutoTimedBlock(string identifier = "", [CallerMemberName] string memberName = "")
			{
				string blockName = memberName + " " + identifier;

				Profiler.Instance.BeginTimedBlock(blockName.Trim());
			}

			public void Dispose()
			{
				Profiler.Instance.EndTimedBlock();
			}
		}

		public class ProfilerRecord
		{
			public string Identifier;
			public long ElapsedTicks;
			public int HitCount;

			public List<ProfilerRecord> Children = new List<ProfilerRecord>();
		}

		struct TimedBlock
		{
			public int Parent;

			public string Identifier;
			public long ElapsedTicks;

			public int HitCount;
		}

		#region Singleton Implementation

		public static readonly Profiler Instance = new Profiler();

		static Profiler()
		{

		}

		#endregion

		int _timedBlockCount;
		TimedBlock[] _timedBlocks;

		int _activeTimedBlock;
		int _nextTimedBlock;

		private Profiler()
		{
			_activeTimedBlock = -1;
			_nextTimedBlock = 0;

			_timedBlockCount = 2048;
			_timedBlocks = new TimedBlock[_timedBlockCount];
		}

//		partial void BeginTimedBlock(string identifier);

//#if PROFILER
//		partial 
		void BeginTimedBlock(string identifier)
		{
			// TODO(david): if we hit this implement dynamic allocation of additional timed blocks
			Debug.Assert(_nextTimedBlock < _timedBlockCount);

			_timedBlocks[_nextTimedBlock].Parent = _activeTimedBlock;
			_timedBlocks[_nextTimedBlock].Identifier = identifier;
			_timedBlocks[_nextTimedBlock].ElapsedTicks = -Stopwatch.GetTimestamp();
			_timedBlocks[_nextTimedBlock].HitCount = 1;

			_activeTimedBlock = _nextTimedBlock++;
		}
//#endif

//		partial void EndTimedBlock();

//#if PROFILER
//		partial
		void EndTimedBlock()
		{
			_timedBlocks[_activeTimedBlock].ElapsedTicks += Stopwatch.GetTimestamp();
			_activeTimedBlock = _timedBlocks[_activeTimedBlock].Parent;
		}
//#endif

		public List<ProfilerRecord> CollateProfilerData()
		{
			var lookup = new Dictionary<int, ProfilerRecord>();
			var rootNodes = new List<ProfilerRecord>();

			// group our timed blocks by parent and identifier
			var groupedBlocks = _timedBlocks.Take(_nextTimedBlock)
				.GroupBy(x => new { x.Parent, x.Identifier })
				.Select(group => new TimedBlock
				{
					Parent = group.First().Parent,
					Identifier = group.First().Identifier,
					ElapsedTicks = group.Sum(r => r.ElapsedTicks),
					HitCount = group.Sum(r => r.HitCount)
				});

			// create tree/s of our grouped timed blocks
			int i = 0;

			foreach (var block in groupedBlocks)
			{
				ProfilerRecord record;

				// populate lookup with block details
				if (lookup.TryGetValue(i, out record))
				{
					// we've seen this block before and added it as a parent
					// populate the data for this parent record now we're at it
					record.Identifier = block.Identifier;
					record.ElapsedTicks = block.ElapsedTicks;
					record.HitCount = block.HitCount;
				}
				else
				{
					// block hasn't been used as a parent, create new record
					record = new ProfilerRecord()
					{
						Identifier = block.Identifier,
						ElapsedTicks = block.ElapsedTicks,
						HitCount = block.HitCount
					};

					lookup.Add(i, record);
				}

				// link this record with its parent or add it as a root node
				if (block.Parent == -1)
				{
					rootNodes.Add(record);
				}
				else
				{
					// this is a child, we need to add it to it's parent record
					ProfilerRecord parentRecord;

					if (!lookup.TryGetValue(block.Parent, out parentRecord))
					{   
						// unknown parent, construct preliminary parent
						parentRecord = new ProfilerRecord();
						lookup.Add(block.Parent, parentRecord);
					}

					parentRecord.Children.Add(record);
				}

				i++;
			}

			return rootNodes;
		}

		public void Reset()
		{
			_activeTimedBlock = -1;
			_nextTimedBlock = 0;
		}
	}
}
