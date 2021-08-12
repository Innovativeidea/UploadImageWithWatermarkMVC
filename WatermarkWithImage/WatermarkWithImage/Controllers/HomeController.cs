using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WatermarkWithImage.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [HttpPost]
        public ActionResult SaveImage()
        {
            string uniqueFileName = string.Empty;
            for (int i = 0; i < Request.Files.Count; i++)
            {
                HttpPostedFileBase file = Request.Files[i]; 
                using (var originalBitMapImage = new Bitmap(file.InputStream))
                using (var watermarkBitMapImage = (Bitmap)Image.FromFile(Server.MapPath("~/Content/Images/watermark.png")))
                {
                    var waterimg = Watermark(watermarkBitMapImage, originalBitMapImage);
                    using (var newBitmap = new Bitmap(waterimg))
                    {
                        uniqueFileName = Guid.NewGuid().ToString() + ".png";
                        newBitmap.Save(Server.MapPath("~/Content/Images/" + uniqueFileName), ImageFormat.Png);
                    }
                    waterimg.Dispose();
                }
            }
            return new JsonResult()
            {
                Data = new { ImageUrl = Url.Content(String.Format("~/Content/Images" + "/{0}", uniqueFileName)) },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = Int32.MaxValue
            };
        }
        public static Bitmap Watermark(Bitmap watermark, Bitmap original)
        {
            var units = GraphicsUnit.Pixel;
            float scale = (Math.Max(original.Width, original.Height) * .33f) /
                            Math.Max(watermark.Width, watermark.Height);
            var watermarkSize = new SizeF(watermark.Width * scale, watermark.Height * scale);

            var watermarkBounds = CenterRectangleOnRectangle(
                new RectangleF(PointF.Empty, watermarkSize), original.GetBounds(ref units));
            var workImage = CopyToArgb32(original);
            // Using the SetOpacity() extension method described in the linked question
            // watermark = watermark.SetOpacity(.5f, 1.05f);

            using (var g = Graphics.FromImage(workImage))
            {
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(watermark, watermarkBounds);

                return workImage;
            }
        }

        private static Bitmap CopyToArgb32(Bitmap source)
        {
            var bitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            bitmap.SetResolution(source.HorizontalResolution, source.VerticalResolution);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(source, new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                    new Rectangle(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
                g.Flush();
            }
            return bitmap;
        }

        private static RectangleF CenterRectangleOnRectangle(RectangleF source, RectangleF destination)
        {
            source.Location = new PointF((destination.Width - source.Width) / 2,
                                         (destination.Height - source.Height) / 2);
            return source;
        }
    }
}