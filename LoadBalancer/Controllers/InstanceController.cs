using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoadBalancer.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LoadBalancer.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class InstanceController : ControllerBase {

        private readonly AppDbContext _context;
        public InstanceController( AppDbContext context )
        {
            _context = context;
        }


        // POST: api/Instance
        [HttpPost]
        public async Task<ActionResult<Instance>> PostInstance(Instance instance)
        {
            _context.Instances.Add(instance);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInstance", new { id = instance.Id }, instance);
        }

        // DELETE: api/Instance/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Instance>> DeleteInstance(int id)
        {
            var instance = await _context.Instances.FindAsync(id);
            if (instance == null)
            {
                return NotFound();
            }

            _context.Instances.Remove(instance);
            await _context.SaveChangesAsync();

            return instance;
        }

        // Just for testing if instance is correctly added or deleted.
        // GET: api/Instance/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Instance>> GetInstance(int id)
        {
            var instance = await _context.Instances.FindAsync(id);

            if (instance == null)
            {
                return NotFound();
            }

            return instance;
        }

        // Just for testing if instance is correctly added or deleted.
        // GET: api/Instance/5
        [HttpGet]
        public ActionResult<List<Instance>> Get()
        {
            var instance = _context.Instances.ToList();
            return instance;
        }
    }
}
