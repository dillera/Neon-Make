using Microsoft.AspNetCore.Mvc;
using ADF;

namespace NeonService.Controllers;

[ApiController]
[Route("[controller]")]
public class NeonController : ControllerBase
{

    private readonly ILogger<NeonController> _logger;
    private readonly ADF.Compiler _compiler;

    public NeonController(ILogger<NeonController> logger)
    {
        _logger = logger;
        _compiler = new ADF.Compiler();
    }

    [HttpPost]
    public IActionResult Post([FromBody] NeonConversionData conversionData)
    {
        if (string.IsNullOrEmpty(conversionData.Source) || string.IsNullOrEmpty(conversionData.Destination))
        {
            return BadRequest("Invalid data");
        }
        if (string.IsNullOrEmpty(conversionData.FileFormat))
        {
            conversionData.FileFormat = FileFormat.ATASCII.ToString();
        }
        ADF.FileFormat Newline;
        bool param = false;
        do
        {
            switch (conversionData.FileFormat.Trim().ToUpper())
            {
                case "ATASCII":
                    Newline = ADF.FileFormat.ATASCII;
                    break;

                case "CRLF":
                    Newline = ADF.FileFormat.CRLF;
                    break;

                case "LF":
                    Newline = ADF.FileFormat.LF;
                    break;

                default:
                    Newline = ADF.FileFormat.ATASCII;
                    break;
            }
            bool result = _compiler.ProcessFile(conversionData.Source,Newline);

            if (result)
            {
                _compiler.SaveDocument(conversionData.Destination);
                Console.WriteLine("Document created");
            }
            else
            {
                Console.WriteLine("Errors found!!");
            }

        } while (param);

        return Ok("successfully converted");
    }
}