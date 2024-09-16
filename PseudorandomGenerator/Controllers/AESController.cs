using Microsoft.AspNetCore.Mvc;
using PseudorandomGenerator.Application;
using PseudorandomGenerator.Application.Detyra1;
using PseudorandomGenerator.Application.Detyra2;
using PseudorandomGenerator.Application.Detyra3;
using PseudorandomGenerator.Application.Detyra4;

namespace PseudorandomGenerator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AESController : ControllerBase
    {
        private SAESService SAESService = new SAESService();
        private CCM CCM = new CCM();
        private DecryptSimplifiedAES DecryptSimplifiedAES = new DecryptSimplifiedAES();
        private CCMEncryption CCMEncryption = new CCMEncryption();
        private GCMEncryption GCMEncryption = new GCMEncryption();
        private XTSEncryption XTSEncryption = new XTSEncryption();
        private MDSCalculator MDSCalculator = new MDSCalculator();
        private Analysis Analysis = new Analysis();

        [HttpGet]
        [Route("aes")]
        public IActionResult GetAES()
        {
            //AesService.Test();

            //This works - Detyra 1
            //SAESService.Encrpyt();
            //SAESService.Decrypt();

            //This works - Detyra 2
            //CCMEncryption.EncryptCCM();
            //Console.WriteLine("Starting decryption");
            //CCMEncryption.DecryptCCM();

            //This works - Detyra 2
            //var result = GCMEncryption.EncryptWithGCMComplete("A2B4");
            //var decResult = GCMEncryption.DecryptWithGCMComplete(result.ciphertext, "A2B4", result.tag);

            //This works - Detyra 2
            //XTSEncryption.EncryptXTS();
            //XTSEncryption.DecryptXTS();

            //This works - Detyra 3
            //Analysis.DifferentialCryptanalysis();
            //Analysis.LinearCryptanalysis();

            //This works - Detyra 4
            //TODO: Generate the matrix on the same field

            //byte[,] mds = new byte[,]
            //{
            //    { 0x57, 0x83, 0x1A, 0x25},
            //    { 0xC1, 0xF3, 0x99, 0x66},
            //    { 0x76, 0xD4, 0xAA, 0xAA},
            //    { 0x12, 0xC4, 0xA7, 0x33}
            //};

            //byte[,] matrix = new byte[,]
            //{
            //    { 0x57, 0x01, 0x1A, 0x25},
            //    { 0xC1, 0xF3, 0x99, 0x66},
            //    { 0x76, 0xD4, 0xAA, 0xAA},
            //    { 0x12, 0xC4, 0xA7, 0x33}
            //};

            //var check = MDSCalculator.CheckMDSMatrix(mds);

            //var matrix_multiply = MDSCalculator.MultiplyMatricesGF(mds, matrix);

            //var new_matrix = MDSCalculator.FindMatrix(matrix);

            //var new_matrix_prod = MDSCalculator.MultiplyMatricesGF(mds, new_matrix);

            //var check_difusion = MDSCalculator.CheckDifusionEffect(matrix_multiply, new_matrix_prod);

            //Console.WriteLine(check_difusion);

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