using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ConsoleTableExt;
using FluentAssertions;
using MethodTimer;
using NestedNavigationProperties.DbContext.Ef6;
using NestedNavigationProperties.Models.Ef6;
using Type = NestedNavigationProperties.Models.Ef6.Type;

namespace Ef6Research
{
    public class TestResult
    {
        public string TestName { get; set; }
        public string ElapsedTime { get; set; }
        public string Remarks { get; set; }
        public string MemoryUsage { get; set; }
        public string Result { get; set; }
    }


    public class Program
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern long StrFormatByteSizeW(long qdw, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszBuf,
            int cchBuf);

        [STAThread]
        static void Main(string[] args)
        {
            long MemorySizeBefore;
            long MemorySizeAfter;
            var memoryStringBuilder = new StringBuilder(32);

            Console.Write("Collecting verification data from database...");
            CollectTestData();
            Console.WriteLine("done.");

            try
            {
                TestUsingStringCollectionProperties();
            }
            catch (EntityCommandCompilationException e)
            {
                Console.WriteLine(
                    $"Got expected EntityCommandCompilationException {e.Message} with innerException {e.InnerException.GetType()}: {e.InnerException.Message}");
            }

            try
            {
                TestUsingIncludeProperties();
            }
            catch (EntityCommandCompilationException e)
            {
                Console.WriteLine(
                    $"Got expected EntityCommandCompilationException {e.Message} with innerException {e.InnerException.GetType()}: {e.InnerException.Message}");
            }


            List<TestResult> results = new List<TestResult>();
            var stopwatch = new Stopwatch();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            using (var proc = Process.GetCurrentProcess())
            {
                proc.Refresh();
                MemorySizeBefore = proc.PrivateMemorySize64;
            }

            stopwatch.Start();
            TestSingleCollectionProperty();
            stopwatch.Stop();

            using (var proc = Process.GetCurrentProcess())
            {
                proc.Refresh();
                MemorySizeAfter = proc.PrivateMemorySize64;

                var number = Convert.ToInt64(MemorySizeAfter - MemorySizeBefore);
                StrFormatByteSizeW(number, memoryStringBuilder, memoryStringBuilder.Capacity);
            }

            results.Add(new TestResult
            {
                TestName = "TestSingleCollectionProperty()",
                Remarks = "This is only a timing reference. See method documentation for details.",
                ElapsedTime = stopwatch.Elapsed.ToString(),
                MemoryUsage = memoryStringBuilder.ToString()
            });


            for (var i = 0; i < 32; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                using (var proc = Process.GetCurrentProcess())
                {
                    proc.Refresh();
                    MemorySizeBefore = proc.PrivateMemorySize64;
                }

                var flags = (IndividualDbSetLoadFlags) i;

                Console.WriteLine($"Testing TestUsingIndividualDbSetLoad() with flags {flags}");
                var result = TestUsingIndividualDbSetLoad(flags);


                using (var proc = Process.GetCurrentProcess())
                {
                    proc.Refresh();
                    MemorySizeAfter = proc.PrivateMemorySize64;

                    var number = Convert.ToInt64(MemorySizeAfter - MemorySizeBefore);
                    StrFormatByteSizeW(number, memoryStringBuilder, memoryStringBuilder.Capacity);
                }

                results.Add(new TestResult
                {
                    TestName = "TestUsingIndividualDbSetLoad()",
                    Remarks = "Flags: " + flags,
                    ElapsedTime = result.timing,
                    Result = result.status,
                    MemoryUsage = memoryStringBuilder.ToString()
                });
            }

            ConsoleTableBuilder.From(results).WithFormat(ConsoleTableBuilderFormat.MarkDown).ExportAndWriteLine();
        }

        /// <summary>
        /// This is only a timing reference. Since EF6 with SQLite doesn't support multiple .Include() on the same
        /// nested navigation property (e.g. both "Presets.Modes" and "Presets.Types"), this only includes "Presets.Modes".
        ///
        /// See TestUsingStringCollectionProperties() and TestUsingIncludeProperties() for details.
        /// </summary>
        private static void TestSingleCollectionProperty()
        {
            using (var context = new ApplicationDatabaseContext())
            {
                context.Plugins
                    .Include("Presets")
                    .Include("Presets.Modes")
                    .Load();
            }
        }

        /// <summary>
        /// This is tries to load multiple navigation properties using a string .Include() of a parent navigation
        /// property. Right now in EF6, this will fail using SQlite as the sql generator does not support APPLY joins
        /// </summary>
        private static void TestUsingStringCollectionProperties()
        {
            using (var context = new ApplicationDatabaseContext())
            {
                context.Plugins
                    .Include("Presets.Modes")
                    .Include("Presets.Types")
                    .AsNoTracking().ToList();
            }
        }

        /// <summary>
        /// This is tries to load multiple navigation properties using mapped properties/.Include() of a parent navigation
        /// property. Right now in EF6, this will fail using SQlite as the sql generator does not support APPLY joins
        /// </summary>
        private static void TestUsingIncludeProperties()
        {
            using (var context = new ApplicationDatabaseContext())
            {
                context.Plugins
                    .Include(x => x.Presets.Select(y => y.Modes))
                    .Include(x => x.Presets.Select(y => y.Types))
                    .AsNoTracking().ToList();
            }
        }

        private static Dictionary<string, int> countTestData = new Dictionary<string, int>();
        private static Dictionary<int, int> pluginPresetCounts = new Dictionary<int, int>();
        private static Dictionary<string, int> presetModeCounts = new Dictionary<string, int>();
        private static Dictionary<string, int> presetTypeCounts = new Dictionary<string, int>();

        [Time]
        private static void CollectTestData()
        {
            using (var context = new ApplicationDatabaseContext())
            {
                context.Database.Log = s => Debug.WriteLine(s);

                countTestData.Add("Plugins", context.Plugins.Count());
                countTestData.Add("Presets", context.Presets.Count());
                countTestData.Add("Modes", context.Modes.Count());
                countTestData.Add("Types", context.Types.Count());

                var presetCounts = (from plugin in context.Plugins
                    group plugin by new {pluginId = plugin.Id, presetCount = plugin.Presets.Count}
                    into foo
                    select new
                    {
                        id = foo.Key.pluginId,
                        count = foo.Key.presetCount
                    }).ToDictionary(t => t.id, t => t.count);

                foreach (var presetCount in presetCounts)
                {
                    pluginPresetCounts.Add(presetCount.Key, presetCount.Value);
                }

                var presetModeTypeCounts = (from preset in context.Presets
                    group preset by new
                        {presetId = preset.PresetId, modesCount = preset.Modes.Count, typesCount = preset.Types.Count}
                    into foo
                    select new
                    {
                        id = foo.Key.presetId,
                        modesCount = foo.Key.modesCount,
                        typesCount = foo.Key.typesCount
                    }).ToList();

                foreach (var presetModeTypeCount in presetModeTypeCounts)
                {
                    presetTypeCounts.Add(presetModeTypeCount.id, presetModeTypeCount.typesCount);
                    presetModeCounts.Add(presetModeTypeCount.id, presetModeTypeCount.modesCount);
                }
            }
        }

        [Flags]
        enum IndividualDbSetLoadFlags
        {
            AutoDetectChangesEnabled = 1,
            LazyLoadingEnabled = 2,
            UseDatabaseNullSemantics = 4,
            ProxyCreationEnabled = 8,
            UseLocal = 16
        }

        private static (string status, string timing) TestUsingIndividualDbSetLoad(IndividualDbSetLoadFlags flags)
        {
            ICollection<Plugin> pluginList;
            ICollection<Mode> modesList;
            ICollection<Type> typesList;
            ICollection<Preset> presetsList;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            using (var context = new ApplicationDatabaseContext())
            {
                context.Configuration.AutoDetectChangesEnabled =
                    flags.HasFlag(IndividualDbSetLoadFlags.AutoDetectChangesEnabled);
                context.Configuration.LazyLoadingEnabled = flags.HasFlag(IndividualDbSetLoadFlags.LazyLoadingEnabled);
                context.Configuration.UseDatabaseNullSemantics =
                    flags.HasFlag(IndividualDbSetLoadFlags.UseDatabaseNullSemantics);
                context.Configuration.ProxyCreationEnabled =
                    flags.HasFlag(IndividualDbSetLoadFlags.ProxyCreationEnabled);

                if (flags.HasFlag(IndividualDbSetLoadFlags.UseLocal))
                {
                    context.Plugins.Load();
                    context.Modes.Load();
                    context.Types.Load();
                    context.Presets.Include(p => p.Modes).Include(p => p.Types).Load();
                    pluginList = context.Plugins.Local;
                    modesList = context.Modes.Local;
                    typesList = context.Types.Local;
                    presetsList = context.Presets.Local;
                }
                else
                {
                    pluginList = context.Plugins.ToList();
                    modesList = context.Modes.ToList();
                    typesList = context.Types.ToList();
                    presetsList = context.Presets.Include(p => p.Modes).Include(p => p.Types).ToList();
                }
            }

            stopwatch.Stop();

            try
            {
                pluginList.Count.Should().Be(countTestData["Plugins"]);
                presetsList.Count.Should().Be(countTestData["Presets"]);
                modesList.Count.Should().Be(countTestData["Modes"]);
                typesList.Count.Should().Be(countTestData["Types"]);

                foreach (var plugin in pluginList)
                {
                    plugin.Presets.Count.Should().Be(pluginPresetCounts[plugin.Id]);

                    foreach (var preset in plugin.Presets)
                    {
                        preset.Modes.Count.Should().Be(presetModeCounts[preset.PresetId]);
                        preset.Types.Count.Should().Be(presetTypeCounts[preset.PresetId]);
                    }
                }
            }
            catch (Exception e)
            {
                return (status: $"failure, got {e}", timing: stopwatch.Elapsed.ToString());
            }

            return (status: "ok", timing: stopwatch.Elapsed.ToString());
        }
    }
}