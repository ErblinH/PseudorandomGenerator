using Microsoft.AspNetCore.Mvc;
using PseudorandomGenerator.Application;
using PseudorandomGenerator.Application.Detyra2;

namespace PseudorandomGenerator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AESController : ControllerBase
    {
        private AesService AesService = new AesService();
        private SAESService SAESService = new SAESService();
        private CCM CCM = new CCM();
        private DecryptSimplifiedAES DecryptSimplifiedAES = new DecryptSimplifiedAES();
        private CCMEncryption CCMEncryption = new CCMEncryption();
        private GCMEncryption GCMEncryption = new GCMEncryption();

        [HttpGet]
        [Route("aes")]
        public IActionResult GetAES()
        {
            //AesService.Test();

            //This works - Detyra 1
            //SAESService.Encrpyt();
            //SAESService.Decrypt();

            //CCM.EncryptWithCCM();

            //DecryptSimplifiedAES.Decrypt();

            //AesService.AES_simplified();


            //This works - Detyra 2
            //CCMEncryption.EncryptCCM();
            //Console.WriteLine("Starting decryption");
            //CCMEncryption.DecryptCCM();

            //This works - Detyra 2
            //var result = GCMEncryption.EncryptWithGCMComplete("A2B4");
            //var decResult = GCMEncryption.DecryptWithGCMComplete(result.ciphertext, "A2B4", result.tag);

            return Ok();
        }

        [HttpGet]
        [Route("saes")]
        public IActionResult GetSAES()
        {

            SAESService.Encrpyt();

            return Ok();
        }
    }
}