using System.IO;
using System.Threading.Tasks;
using Amazon.S3.Model;

namespace RSPeer.Infrastructure.File.Base
{
    public interface IFileStorage
    {
        Task<PutObjectResponse> PutStream(Stream stream, string key, bool publicRead = true);
        
        Task<PutObjectResponse> Put(byte[] body, string key);

        Task<PutObjectResponse> PutFile(string path, string key);
        
        Task<GetObjectResponse> Get(string key);
        
        Task<DeleteObjectResponse> Delete(string key);
    }
}