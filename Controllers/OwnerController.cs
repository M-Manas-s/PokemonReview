using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PokemonReview.Dto;
using PokemonReview.Interfaces;
using PokemonReview.Models;

namespace PokemonReview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerController : Controller
    {
        private readonly IOwnerRepository _ownerRepository;
        private readonly IMapper _mapper;
        private readonly ICountryRepository _countryRepository;
        public OwnerController(IOwnerRepository repo,ICountryRepository countryRepository, IMapper mapper)
        {
            _ownerRepository = repo;
            _mapper = mapper;
            _countryRepository = countryRepository;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Owner>))]
        public IActionResult GetOwners()
        {
            var owners = _mapper.Map<List<OwnerDto>>(_ownerRepository.GetOwners());
            
            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            return Ok(owners);
        }

        [HttpGet("{ownerId}")]
        [ProducesResponseType(200, Type = typeof(Owner))]
        [ProducesResponseType(400)]
        public IActionResult GetOwner(int ownerId)
        {
            if ( !_ownerRepository.OwnerExists(ownerId) )
                return NotFound();

            var owner = _mapper.Map<OwnerDto>(_ownerRepository.GetOwner(ownerId));

            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            return Ok(owner);
        }

        [HttpGet("{ownerId}/pokemon")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Pokemon>))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemonByOwner(int ownerId)
        {
            if ( !_ownerRepository.OwnerExists(ownerId) )
                return NotFound();

            var pokemons = _mapper.Map<List<PokemonDto>>(_ownerRepository.GetPokemonByOwner(ownerId));

            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            return Ok(pokemons);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateOwner([FromQuery] int countryId, [FromBody] OwnerDto ownerCreate)
        {
            if ( ownerCreate == null )
                return BadRequest(ModelState);

            var owners = _ownerRepository.GetOwners()
                                .Where(c => c.LastName.Trim().ToUpper() == ownerCreate.LastName.Trim().ToUpper())
                                .FirstOrDefault();
            
            if ( owners != null )
            {
                ModelState.AddModelError("", $"Category {ownerCreate.LastName} already exists");
                return StatusCode(422, ModelState);
            }

            if ( !ModelState.IsValid )
                return BadRequest(ModelState);
            
            var ownersMap = _mapper.Map<Owner>(ownerCreate);
            ownersMap.Country = _countryRepository.GetCountry(countryId);

            if ( !_ownerRepository.CreateOwner(ownersMap) )
            {
                ModelState.AddModelError("", $"Something went wrong saving {ownersMap.LastName}");
                return StatusCode(500, ModelState);
            }

            return Ok("Successful created");
        }

    }
}