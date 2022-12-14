using WonderAddressBookMVC_.Services.Interfaces;

namespace WonderAddressBookMVC_.Services
{
    public class ImageService : IImageService
    {
        #region Globals
        private readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };
        private readonly string defaultImage = "img/GirlOne_4x.png";
        #endregion
        public string ConvertByteArrayToFile(byte[] fileData, string extension)
        {
            if (fileData is null) return defaultImage;
            try
            {
                string imageBase64Data = Convert.ToBase64String(fileData);
                return string.Format($"data:{extension}; base64,{imageBase64Data}");

            }
            catch (Exception)
            {

                throw;
            }
        }
        #region Convert file to Byte Array
        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                using MemoryStream memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                return byteFile;

            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
    }
}
