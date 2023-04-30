using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PokemonReview.Dto;
using PokemonReview.Interfaces;
using PokemonReview.Models;

namespace PokemonReview.Controllers
{   
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryController(ICategoryRepository repo, IMapper mapper)
        {
            _categoryRepository = repo;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Category>))]
        public IActionResult GetCategories()
        {
            var categories = _mapper.Map<List<CategoryDto>>(_categoryRepository.GetCategories());
            
            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            return Ok(categories);
        }

        [HttpGet("{categoryId}")]
        [ProducesResponseType(200, Type = typeof(Category))]
        [ProducesResponseType(400)]
        public IActionResult GetCategory(int categoryId)
        {
            if ( !_categoryRepository.CategoryExists(categoryId) )
                return NotFound();

            var category = _mapper.Map<CategoryDto>(_categoryRepository.GetCategory(categoryId));

            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            return Ok(category);
        }

        [HttpGet("pokemon/{categoryId}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Pokemon>))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemonByCategoryId(int categoryId)
        {
            var pokemons = _mapper.Map<List<PokemonDto>>(_categoryRepository.GetPokemonByCategory(categoryId));

            if ( !ModelState.IsValid )
                return BadRequest(ModelState);
            
            return Ok(pokemons); 
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateResult([FromBody] CategoryDto categoryCreate)
        {
            if ( categoryCreate == null )
                return BadRequest(ModelState);

            var category = _categoryRepository.GetCategories()
                                .Where(c => c.Name.Trim().ToUpper() == categoryCreate.Name.Trim().ToUpper())
                                .FirstOrDefault();
            
            if ( category != null )
            {
                ModelState.AddModelError("", $"Category {categoryCreate.Name} already exists");
                return StatusCode(422, ModelState);
            }

            if ( !ModelState.IsValid )
                return BadRequest(ModelState);
            
            var categoryMap = _mapper.Map<Category>(categoryCreate);

            if ( !_categoryRepository.CreateCategory(categoryMap) )
            {
                ModelState.AddModelError("", $"Something went wrong saving {categoryMap.Name}");
                return StatusCode(500, ModelState);
            }

            return Ok("Successful created");
        }

        [HttpPut("{categoryId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult UpdateCategory(int categoryId, [FromBody] CategoryDto updatedCategory)
        {
            if ( updatedCategory == null || categoryId != updatedCategory.Id )
                return BadRequest(ModelState);

            if ( !_categoryRepository.CategoryExists(categoryId) )
                return NotFound();

            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            var categoryMap = _mapper.Map<Category>(updatedCategory);

            if ( !_categoryRepository.UpdateCategory(categoryMap) )
            {
                ModelState.AddModelError("", $"Something went wrong updating {categoryMap.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{categoryId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult DeleteCategory(int categoryId)
        {
            if ( !_categoryRepository.CategoryExists(categoryId) )
                return NotFound();

            var categoryToDelete = _categoryRepository.GetCategory(categoryId);

            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            if ( !_categoryRepository.DeleteCategory(categoryToDelete) )
            {
                ModelState.AddModelError("", $"Something went wrong deleting {categoryToDelete.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}