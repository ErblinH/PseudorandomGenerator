using Microsoft.AspNetCore.Mvc;
using PseudorandomGenerator.Application;
using System.Text;

namespace PseudorandomGenerator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GeneratorController : ControllerBase
    {
        private PseudoGenerator PseudoGeneratorService = new PseudoGenerator();

        [HttpGet]
        [Route("hash")]
        public IActionResult Get()
        {
            // Numri i unazave 
            int m = 10;
            string data = "InitialData";
            // Numri i biteve per tu kthyer
            int n = 128;
            // Gjatesia e vleres inicializuese
            int seedlen = 16;
            string result = PseudoGeneratorService.HashEncrypt(m, data, n, seedlen);
            Console.WriteLine("Resulting Hash: " + result);

            return Ok();
        }

        [HttpGet]
        [Route("hmac")]
        public IActionResult GetHMAC()
        {
            byte[] key = Encoding.UTF8.GetBytes("secret");
            byte[] initialValue = Encoding.UTF8.GetBytes("initialValue");
            // Numri i deshiruar i biteve per numrin pseudo random
            int n = 256;
            // Gjatesia ne bit e rezultati nga HMAC
            int outlen = 256;
            byte[] pseudoRandomNumber = PseudoGeneratorService.Generate(key, initialValue, n, outlen);

            Console.WriteLine("Pseudo-random number (hex): " + BitConverter.ToString(pseudoRandomNumber).Replace("-", ""));

            return Ok();
        }

        [HttpGet]
        [Route("hmac/ieee")]
        public IActionResult GetHMAC_IEEE()
        {
            byte[] key = Encoding.UTF8.GetBytes("secret-key");
            byte[] V = Encoding.UTF8.GetBytes("initial-value");
            // Numri i deshiruar i biteve per numrin pseudo random
            int n = 256;
            // Gjatesia ne bit e rezultati nga HMAC
            int outlen = 256;
            byte[] pseudoRandomNumber = PseudoGeneratorService.GeneratePseudoRandomBytes(key, V, n, outlen);

            Console.WriteLine("Pseudo-random number (hex): " + BitConverter.ToString(pseudoRandomNumber).Replace("-", ""));

            return Ok();
        }

        [HttpGet]
        [Route("hmac/tls")]
        public IActionResult GetHMAC_TLS()
        {
            byte[] key = Encoding.UTF8.GetBytes("secret-key");
            byte[] V = Encoding.UTF8.GetBytes("initial-value");
            // Numri total i biteve te deshiruar
            int n = 256;
            // Gjatesia e biteve per daljen e HMAC
            int outlen = 256;
            byte[] randomOutput = PseudoGeneratorService.GenerateTLS(key, V, n, outlen);

            Console.WriteLine(BitConverter.ToString(randomOutput).Replace("-", ""));

            return Ok();
        }
    }
}