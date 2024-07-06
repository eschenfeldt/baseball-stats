using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaseballApi;
using BaseballApi.Models;

namespace BaseballApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParkController : ControllerBase
    {
        private readonly BaseballContext _context;

        public ParkController(BaseballContext context)
        {
            _context = context;
        }

        // GET: api/Park
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Park>>> GetParks()
        {
            return await _context.Parks.ToListAsync();
        }

        // GET: api/Park/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Park>> GetPark(long id)
        {
            var park = await _context.Parks.FindAsync(id);

            if (park == null)
            {
                return NotFound();
            }

            return park;
        }

        // PUT: api/Park/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPark(long id, Park park)
        {
            if (id != park.Id)
            {
                return BadRequest();
            }

            _context.Entry(park).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParkExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Park
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Park>> PostPark(Park park)
        {
            _context.Parks.Add(park);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPark", new { id = park.Id }, park);
        }

        // DELETE: api/Park/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePark(long id)
        {
            var park = await _context.Parks.FindAsync(id);
            if (park == null)
            {
                return NotFound();
            }

            _context.Parks.Remove(park);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ParkExists(long id)
        {
            return _context.Parks.Any(e => e.Id == id);
        }
    }
}
