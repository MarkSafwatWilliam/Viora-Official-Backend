namespace Viora.Services
{
    public class SaveAudioService
    {
        private readonly string _audioFolderPath;
        public SaveAudioService(IConfiguration configuration) {

            _audioFolderPath = configuration["LocalPath:FolderPath"];
        }


        public async Task<string> SaveAudioAsync(byte[] file, string extension = ".wav")
        {
            if (file == null || file.Length == 0)
                throw new Exception("Invalid file");

            var folderPath = _audioFolderPath;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            await File.WriteAllBytesAsync(filePath, file);

            return filePath;
        }


    }
}
