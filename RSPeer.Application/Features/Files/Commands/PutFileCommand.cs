using System.IO;
using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Files.Commands
{
	public class PutFileCommand : IRequest<BinaryFile>
	{
		public string Name { get; set; }
		public byte[] Contents { get; set; }
		public Stream Stream { get; set; }
		public decimal Version { get; set; }
	}
}