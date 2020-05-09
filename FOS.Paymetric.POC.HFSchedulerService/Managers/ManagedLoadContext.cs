using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace FOS.Paymetric.POC.HFSchedulerService.Managers
{
    internal class ManagedLoadContext : AssemblyLoadContext
    {
        List<string> _plugInAssyNames;

        internal ManagedLoadContext(string folderName, List<string> plugInAssyNames, bool isCollectible) : base(folderName, isCollectible) 
        {
            _plugInAssyNames = plugInAssyNames;

            this.Resolving += this.OnAssemblyResolve;
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            System.Diagnostics.Debug.WriteLine(assemblyName.Name);

            //if (_plugInAssyNames.Contains(assemblyName.Name))
            //if(assemblyName.Name == @"FOS.Paymetric.POC.HFSchedulerService.Shared")
            if (assemblyName.Name.StartsWith(@"FOS"))
            {

                var xx = AssemblyLoadContext.Default.Assemblies.Where(a => a.GetName() == assemblyName).FirstOrDefault();
                return xx;
            }
            else if (assemblyName.Name.StartsWith(@"FIS"))
            {

                return AssemblyLoadContext.Default.Assemblies.Where(a => a.GetName() == assemblyName).FirstOrDefault();
            }
            else
            {
                return AssemblyLoadContext.Default.Assemblies.Where(a => a.GetName() == assemblyName).FirstOrDefault();
            }
        }

        protected virtual Assembly OnAssemblyResolve(AssemblyLoadContext assemblyLoadContext, AssemblyName assemblyName)
        {
            return AssemblyLoadContext.Default.Assemblies.Where(a => a.GetName() == assemblyName).FirstOrDefault();

            //var assembly = assemblyLoadContext.LoadFromAssemblyName(assemblyName);
            //if (assembly == null)
            //{
            //    assembly = this.HandleDiscovery(assemblyName);
            //}
            //return assembly;
        }


    }
}
