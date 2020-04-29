using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using FOS.Paymetric.POC.HFSchedulerService.Shared;
using FOS.Paymetric.POC.HFSchedulerService.Shared.Interfaces;
using FOS.Paymetric.POC.HFSchedulerService.Managers;

namespace FOS.Paymetric.POC.HFSchedulerService.Controllers.v1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PlugInsController : ControllerBase
    {
        private readonly ILogger<PlugInsController> _logger;
        private PlugInsManager _plugIsManager;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="scheduleTaskPlugIns"></param>
        public PlugInsController(PlugInsManager plugIsManager, ILogger<PlugInsController> logger)
        {
            _logger = logger;
            _plugIsManager = plugIsManager;
        }

        /// <summary>
        /// Returns a list of the currently loaded plugins
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<List<Tuple<String, String>>> GetPlugIns()
        {
            var loadedPlugIns = _plugIsManager.PlugIns.Select(pi => new
                                                        {
                                                            name = pi.Metadata.Name,
                                                            version = pi.Metadata.Version,
                                                            type = pi.Value.GetType().ToString(),
                                                            dt = pi.Value.GetDTCompiled()
                                                        }).ToList();
                                         
            return Ok(loadedPlugIns);
        }

        /// <summary>
        /// Unload a plugin
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public ActionResult UnloadPlugin(string name, decimal version)
        {
            //var plugIn = _scheduleTaskPlugIns.Where(pi => pi.Metadata.Name == name && pi.Metadata.Version == (double)version).FirstOrDefault();

            //if(plugIn != null)
            //{
            //    _scheduleTaskPlugIns = _scheduleTaskPlugIns.Where(pi => pi.Metadata.Name != name && pi.Metadata.Version != (double)version);
            //    GC.Collect();
            //    return Ok();
            //}
            //else
            //{
                return NotFound($"No plugin found matching name: [{name}], version: [{version}]");
            //}
        }
    }
}