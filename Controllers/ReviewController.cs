using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PokemonReview.Dto;
using PokemonReview.Interfaces;
using PokemonReview.Models;

namespace PokemonReview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;
        private readonly IPokemonRepository _pokemonRepository;
        private readonly IReviewerRepository _reviewerRepository;

        public ReviewController(
            IReviewRepository repo,
            IMapper mapper,
            IPokemonRepository pokemonRepository,
            IReviewerRepository reviewerRepository)
        {
            _reviewRepository = repo;
            _mapper = mapper;
            _pokemonRepository = pokemonRepository;
            _reviewerRepository = reviewerRepository;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Review>))]
        public IActionResult GetReviews()
        {
            var reviews = _mapper.Map<List<ReviewDto>>(_reviewRepository.GetReviews());
            
            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            return Ok(reviews);
        }

        [HttpGet("{reviewId}")]
        [ProducesResponseType(200, Type = typeof(Review))]
        [ProducesResponseType(400)]
        public IActionResult GetReview(int reviewId)
        {
            if ( !_reviewRepository.ReviewExists(reviewId) )
                return NotFound();

            var review = _mapper.Map<ReviewDto>(_reviewRepository.GetReview(reviewId));

            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            return Ok(review);
        }

        [HttpGet("{reviewId}/pokemon")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Pokemon>))]
        [ProducesResponseType(400)]
        public IActionResult GetReviewsOfPokemon(int reviewId)
        {

            var reviews = _mapper.Map<List<ReviewDto>>(_reviewRepository.GetReviewsOfPokemon(reviewId));

            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            return Ok(reviews);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateReview([FromQuery] int reviewerId, [FromQuery] int pokeId ,[FromBody] ReviewDto reviewCreate)
        {
            if ( reviewCreate == null )
                return BadRequest(ModelState);

            var reviews = _reviewRepository.GetReviews()
                                .Where(c => c.Title.Trim().ToUpper() == reviewCreate.Title.Trim().ToUpper())
                                .FirstOrDefault();
            
            if ( reviews != null )
            {
                ModelState.AddModelError("", $"Category {reviewCreate.Title} already exists");
                return StatusCode(422, ModelState);
            }

            if ( !ModelState.IsValid )
                return BadRequest(ModelState);
            
            var reviewMap = _mapper.Map<Review>(reviewCreate);

            reviewMap.Reviewer = _reviewerRepository.GetReviewer(reviewerId);
            reviewMap.Pokemon = _pokemonRepository.GetPokemon(pokeId);

            if ( !_reviewRepository.CreateReview(reviewMap) )
            {
                ModelState.AddModelError("", $"Something went wrong saving {reviewMap.Title}");
                return StatusCode(500, ModelState);
            }

            return Ok("Successful created");
        }

    }
}