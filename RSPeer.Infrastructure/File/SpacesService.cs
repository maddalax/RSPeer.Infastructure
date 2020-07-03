using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using RSPeer.Infrastructure.File.Base;

namespace RSPeer.Infrastructure.File
{
	public class SpacesService : IFileStorage
	{
		public SpacesService(IConfiguration configuration)
		{
			_s3 = new AmazonS3Client(configuration.GetValue<string>("Spaces:AccessKey"),
				configuration.GetValue<string>("Spaces:Secret"), new AmazonS3Config
				{
					ServiceURL = configuration.GetValue<string>("Spaces:Endpoint")
				});
			_bucket = configuration.GetValue<string>("Spaces:Bucket");
		}

		private readonly AmazonS3Client _s3;
		private readonly string _bucket;

		public async Task<PutObjectResponse> PutStream(Stream stream, string key, bool publicRead = true)
		{
			return await _s3.PutObjectAsync(new PutObjectRequest
			{
				AutoCloseStream = true,
				CannedACL = publicRead ? S3CannedACL.PublicRead : S3CannedACL.Private,
				InputStream = stream,
				Key = key,
				BucketName = _bucket
			});
		}
		
		public async Task<PutObjectResponse> Put(byte[] body, string key)
		{
			return await _s3.PutObjectAsync(new PutObjectRequest
			{
				AutoCloseStream = true,
				InputStream = new MemoryStream(body),
				Key = key,
				BucketName = _bucket
			});
		}

		public async Task<PutObjectResponse> PutFile(string path, string key)
		{
			return await _s3.PutObjectAsync(new PutObjectRequest
			{
				AutoCloseStream = true,
				FilePath = path,
				Key = key,
				BucketName = _bucket
			});
		}

		public async Task<GetObjectResponse> Get(string key)
		{
			return await _s3.GetObjectAsync(_bucket, key);
		}

		public async Task<DeleteObjectResponse> Delete(string key)
		{
			return await _s3.DeleteObjectAsync(_bucket, key);
		}
	}
}