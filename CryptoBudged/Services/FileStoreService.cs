using System.IO;
using CryptoBudged.Models;
using Newtonsoft.Json;

namespace CryptoBudged.Services
{
    public class FileStoreService
    {
        private const string FileStorePath = "fileStore.json";

        private FileStoreModel _fileStore;

        private FileStoreService() { }

        public bool HasSaves() => File.Exists(FileStorePath);

        public void Save(FileStoreModel fileStoreModel)
        {
            _fileStore = fileStoreModel;

            var json = JsonConvert.SerializeObject(_fileStore);
            File.WriteAllText(FileStorePath, json);
        }

        public FileStoreModel Load()
        {
            if (_fileStore == null && HasSaves())
            {
                var fileContent = File.ReadAllText(FileStorePath);
                _fileStore = JsonConvert.DeserializeObject<FileStoreModel>(fileContent);
            }

            return _fileStore;
        }

        public static FileStoreService Instance { get; } = new FileStoreService();
    }
}
