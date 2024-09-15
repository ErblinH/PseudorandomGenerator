using Microsoft.AspNetCore.Mvc;
using PseudorandomGenerator.Application;

namespace PseudorandomGenerator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MDSController : Controller
    {

        private MDSService mdsService = new MDSService(4);

        [HttpGet]
        [Route("mds")]
        public IActionResult GetMDS()
        {
            //AesService.Test();

            var matrix = mdsService.PrintMatrix();

            byte det = mdsService.Determinant(matrix);
            Console.WriteLine($"Determinant of the 4x4 matrix: {det:X2}");

            if (det != 0)
            {
                Console.WriteLine("The matrix is non-singular and may be an MDS matrix.");
            }
            else
            {
                Console.WriteLine("The matrix is singular and not an MDS matrix.");
            }

            return Ok();
        }
    }
}
