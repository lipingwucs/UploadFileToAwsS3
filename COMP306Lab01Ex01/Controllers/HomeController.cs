using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Mvc;
namespace COMP306Lab01Ex01.Controllers
{
    public class HomeController : Controller
    {
        private  string keyName = ""; 
        // Specify your bucket region (an example region is shown).  
        private static readonly string bucketName = ConfigurationManager.AppSettings["BucketName"];
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.CACentral1;
        private static readonly string accesskey = ConfigurationManager.AppSettings["AWSAccessKey"];
        private static readonly string secretkey = ConfigurationManager.AppSettings["AWSSecretKey"];

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult UploadFile()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            var s3Client = new AmazonS3Client(accesskey, secretkey, bucketRegion);

            var fileTransferUtility = new TransferUtility(s3Client);
            try
            {
                if (file.ContentLength > 0)
                {
                    keyName = file.FileName;
                    var filePath = Path.Combine(Server.MapPath("~/Files"), Path.GetFileName(file.FileName));
                    file.SaveAs(filePath);
                    var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = bucketName,
                        FilePath = filePath,
                        StorageClass = S3StorageClass.StandardInfrequentAccess,
                        PartSize = 6291456, // 6 MB.  
                        Key = keyName,
                        CannedACL = S3CannedACL.PublicRead
                    };
                    fileTransferUtilityRequest.Metadata.Add("param1", "Value1");
                    fileTransferUtilityRequest.Metadata.Add("param2", "Value2");
                    fileTransferUtility.Upload(fileTransferUtilityRequest);
                    fileTransferUtility.Dispose();
                }
                ViewBag.Message = "File Uploaded Successfully!!";
            }

            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    ViewBag.Message = "Check the provided AWS Credentials.";
                }
                else
                {
                    ViewBag.Message = "Error occurred: " + amazonS3Exception.Message;
                }
            }
            return RedirectToAction("Index");
        }
    }
}  