using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

using FOS.Paymetric.POC.HFSchedulerService.Shared;
using FOS.Paymetric.POC.HFSchedulerService.Shared.Interfaces;

namespace FOS.Paymetric.POC.HFSchedulerService.Managers
{
    public class PlugInsManager
    {
        // this collection holds the dynamically loaded assys   
        //  IEventPublisher is the common interface that all the sample plug-ins will implement
        //  MessageSenderType has a custom property that will allow us to pick a specific plug-in
        [ImportMany()]
        private static IEnumerable<Lazy<IJobPlugIn, JobPlugInType>> _scheduleTaskPlugIns { get; set; }

        string _plugInsFolder;

        public PlugInsManager(string plugInsFolder)
        {
            _plugInsFolder = plugInsFolder;

            Compose(_plugInsFolder);
        }

        public IEnumerable<Lazy<IJobPlugIn, JobPlugInType>> PlugIns
        {
            get { return _scheduleTaskPlugIns;  }
        }

        #region Helpers

        /// <summary>
        /// Build the composition host from assys that are dynamically loaded from a specific subfolder.
        /// </summary>
        private static void Compose(string plugInsFolder)
        {
            // build the correct path to load the plug-in assys from
            var executableLocation = Assembly.GetEntryAssembly().Location;
            var path = Path.Combine(Path.GetDirectoryName(executableLocation), plugInsFolder);

            // get a list of only the managed dlls
            var managedDlls = GetListOfManagedAssemblies(path, SearchOption.AllDirectories);

            // load the assys
            var assemblies = managedDlls
                        .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath)
                        .ToList();

            // build a composition container
            var configuration = new ContainerConfiguration()
                        .WithAssemblies(assemblies);

            try
            {
                // load the plug-in assys that export the correct attribute
                using (var container = configuration.CreateContainer())
                {
                    // note: noraml parameter widening
                    _scheduleTaskPlugIns = container.GetExports<Lazy<IJobPlugIn, JobPlugInType>>();
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                Console.WriteLine(errorMessage);
            }
        }

        /// <summary>
        /// Gets the list of managed assemblies.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="searchOption">The search option.</param>
        /// <returns>List&lt;System.String&gt;.</returns>
        /// <remarks>
        /// Some of the dlls in the target folder may be unmanaged dlls referenced by managed dlls, we need to exclude those
        /// </remarks>
        private static List<string> GetListOfManagedAssemblies(string folderPath, SearchOption searchOption)
        {
            List<string> assyPathNames = new List<string>();

            var files = Directory.GetFiles(folderPath, "*.dll", searchOption);

            foreach (var filePathName in files)
            {
                try
                {
                    // this call will throw an exception if this is not a managed dll
                    System.Reflection.AssemblyName testAssembly = System.Reflection.AssemblyName.GetAssemblyName(filePathName);

                    assyPathNames.Add(filePathName);
                }
                catch
                {
                    // swallow the exception and continue
                }
            }

            return assyPathNames;
        }

        /// <summary>
        /// A research spike into loading each plugin's assys into an isolated AssemblyLoadContext
        /// </summary>
        /// <param name="assysToIgnore">The assys to ignore.</param>
        /// <remarks>
        /// assysToIgnore is a list of assys NOT to load into the plug-in AssemblyLoadContext so the types will
        /// match and the a GetExports call will recognize them as the same type
        /// </remarks>
        private static void ComposeIsolated(string plugInsFolder, List<string> assysToIgnore)
        {
            // build the correct path to load the plug-in assys from
            var executableLocation = Assembly.GetEntryAssembly().Location;
            var path = Path.Combine(Path.GetDirectoryName(executableLocation), plugInsFolder);

            // find the names of all the plug-in subfolders
            var plugInFolderPathNames = Directory.GetDirectories(path);

            Dictionary<string, IEnumerable<Lazy<IJobPlugIn, JobPlugInType>>> test = new Dictionary<string, IEnumerable<Lazy<IJobPlugIn, JobPlugInType>>>();

            // loop thru each plug-in subfolder and load the assys into a separate (isolated) load context.
            foreach (var plugInFolderPathName in plugInFolderPathNames)
            {
                var plugInFolderName = Path.GetFileName(plugInFolderPathName);

                var assyLoadContext = new AssemblyLoadContext(plugInFolderName);

                // get a list of all the assys from that path
                var assemblies = Directory
                            .GetFiles(plugInFolderPathName, "*.dll", SearchOption.AllDirectories)
                            .Where(f => !f.Contains(assysToIgnore[0]))
                            .Select(assyLoadContext.LoadFromAssemblyPath)
                            .ToList();

                var configuration = new ContainerConfiguration()
                            .WithAssemblies(assemblies);

                // load the plug-in assys that export the correct attribute
                using (var container = configuration.CreateContainer())
                {
                    _scheduleTaskPlugIns = container.GetExports<Lazy<IJobPlugIn, JobPlugInType>>();
                }

                test.Add(plugInFolderName, _scheduleTaskPlugIns);

                _scheduleTaskPlugIns = (IEnumerable<Lazy<IJobPlugIn, JobPlugInType>>)test.Values.ToList();
            }
        }
        #endregion
    }
}

