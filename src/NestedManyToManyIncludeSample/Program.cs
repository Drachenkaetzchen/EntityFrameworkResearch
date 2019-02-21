using System.Data.Entity;
using NestedNavigationProperties.DbContext.Ef6;

namespace NestedManyToManyIncludeSample
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new ApplicationDatabaseContext())
            {
                // Works fine
                context.Plugins
                    .Include("DefaultTypes")
                    .Include("DefaultModes")
                    .Include("Presets")
                    .Include("Presets.Types")
                    .Load();
                
                // EntityCommandCompilationException An error occurred while preparing the command definition. See the inner exception for details.
                // innerException System.NotSupportedException: APPLY joins are not supported
                context.Plugins
                    .Include("Presets.Modes")
                    .Include("Presets.Types")
                    .Load();
            }
        }
    }
}