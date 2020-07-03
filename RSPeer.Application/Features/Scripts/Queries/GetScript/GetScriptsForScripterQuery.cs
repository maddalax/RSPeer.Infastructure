using System.Collections.Generic;
using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Scripts.Queries.GetScript
{
	public class GetScriptsForScripterQuery : IRequest<IEnumerable<Script>>
	{
		public int UserId { get; set; }
	}
}