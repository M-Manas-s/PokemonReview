using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PokemonReview.Dto;
using PokemonReview.Interfaces;
using PokemonReview.Models;

namespace PokemonReview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ReviewerController : Controller
    {
        private readonly IReviewerRepository _reviewerRepository;
        private readonly IMapper _mapper;

        public ReviewerController(IReviewerRepository repo, IMapper mapper)
        {
            _reviewerRepository = repo;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Reviewer>))]
        public IActionResult GetReviewers()
        {
            var reviewers = _mapper.Map<List<ReviewerDto>>(_reviewerRepository.GetReviewers());
            
            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            return Ok(reviewers);
        }

        [HttpGet("{reviewerId}")]
        [ProducesResponseType(200, Type = typeof(Reviewer))]
        [ProducesResponseType(400)]
        public IActionResult GetReviewer(int reviewerId)
        {
            if ( !_reviewerRepository.ReviewerExists(reviewerId) )
                return NotFound();

            var reviewer = _mapper.Map<ReviewerDto>(_reviewerRepository.GetReviewer(reviewerId));

            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            return Ok(reviewer);
        }

        [HttpGet("{reviewerId}/reviews")]
        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(400)]
        public IActionResult GetReviewsByReviewer(int reviewerId)
        {
            if ( !_reviewerRepository.ReviewerExists(reviewerId) )
                return NotFound();

            var reviews = _mapper.Map<List<ReviewDto>>(_reviewerRepository.GetReviewsByReviewer(reviewerId));

            if ( !ModelState.IsValid )
                return BadRequest(ModelState);

            return Ok(reviews);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateReviewer([FromBody] ReviewerDto reviewerCreate)
        {
            if ( reviewerCreate == null )
                return BadRequest(ModelState);

            var reviewers = _reviewerRepository.GetReviewers()
                                .Where(c => c.LastName.Trim().ToUpper() == reviewerCreate.LastName.Trim().ToUpper())
                                .FirstOrDefault();
            
            if ( reviewers != null )
            {
                ModelState.AddModelError("", $"Category {reviewerCreate.LastName} already exists");
                return StatusCode(422, ModelState);
            }

            if ( !ModelState.IsValid )
                return BadRequest(ModelState);
            
            var reviwerMap = _mapper.Map<Reviewer>(reviewerCreate);

            if ( !_reviewerRepository.CreateReviewer(reviwerMap) )
            {
                ModelState.AddModelError("", $"Something went wrong saving {reviwerMap.LastName}");
                return StatusCode(500, ModelState);
            }

            return Ok("Successful created");
        }

    }
}