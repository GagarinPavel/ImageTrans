using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ImageTransformer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransformerController : ControllerBase
    {

        [HttpPost]
        public IActionResult Transform([FromQuery] string transform, [FromQuery] string coords, IFormFile uploadedFile)
        {
            if(uploadedFile is null)
            {
                throw new ArgumentNullException(nameof(uploadedFile));
            }
            var coordsArr = coords.Split(',');
            if (coordsArr.Length != 4) return BadRequest();
            int[] intArr = new int[4];
            for (int i = 0; i < intArr.Length; i++)
            {
                if(!Int32.TryParse(coordsArr[i], out intArr[i])) return BadRequest();
            }

            if(intArr[2]<0)
            {
                intArr[0] = (intArr[0] + intArr[2])<0 ? 0: intArr[0] + intArr[2];
                intArr[2] = Math.Abs(intArr[2]);
            }
            if (intArr[3] < 0)
            {
                intArr[1] = (intArr[1] + intArr[3]) < 0 ? 0 : intArr[1] + intArr[3];
                intArr[3] = Math.Abs(intArr[3]);
            }
            if (intArr[2] == 0 | intArr[3] == 0) return NoContent();
            Rectangle rect = new Rectangle(intArr[0], intArr[1], intArr[2], intArr[3]);
            using (var ms = new MemoryStream())
            {
                uploadedFile.CopyTo(ms);
                var fileBytes = ms.ToArray();
                using (Image image = Image.Load(fileBytes))
                {
                    switch (transform)
                    {
                        case "rotate-cw": image.Mutate(img => img.Rotate(RotateMode.Rotate90)); break;
                        case "rotate-ccw": image.Mutate(img => img.Rotate(RotateMode.Rotate270)); break;
                        case "flip-h": image.Mutate(img => img.Flip(FlipMode.Horizontal)); break;
                        case "flip-v": image.Mutate(img => img.Flip(FlipMode.Vertical)); break;
                        default: return BadRequest();
                    }
                    if (intArr[1] + intArr[3] > image.Height | intArr[0] + intArr[2] > image.Width)
                    {
                        return BadRequest();
                    }
                    image.Mutate((img) => img.Crop(rect));
                    using (var newMs = new MemoryStream())
                    {
                        image.SaveAsPng(newMs);
                        return File(newMs.ToArray(), "image/png");
                    }
                   
                }
            }
        }
    }
}
