using Microsoft.AspNetCore.Mvc;
using PseudorandomGenerator.Application;
using PseudorandomGenerator.Application.Detyra2;
using PseudorandomGenerator.Application.Detyra4;

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
        private MDSCalculator MDSCalculator = new MDSCalculator();

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

            byte[,] matrix2 = new byte[,]
            {
                { 0x57, 0x83, 0x1A },
                { 0xC1, 0xF3, 0x99 },
                { 0x76, 0xD4, 0xAA }
            };

            var oki = MDSCalculator.Determinant3x3(matrix2);
            MDSCalculator.PrintMatrix(matrix2);

            int[,] matrix = new int[,]
            {
                { 1, 2, 3, 4 },
                { 5, 6, 7, 8 },
                { 9, 10, 11, 12 },
                { 13, 14, 15, 16 }
            };

            var ok = MDSCalculator.TruncateMDSMatrix(3, 0, 0, matrix);
            MDSCalculator.PrintMatrix(ok);


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