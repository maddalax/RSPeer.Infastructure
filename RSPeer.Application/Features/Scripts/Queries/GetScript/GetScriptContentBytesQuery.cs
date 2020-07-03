using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Scripts.Queries.GetScript
{
    public class GetScriptContentBytesQuery : IRequest<byte[]>
    {
        	public int ScriptId { get; set; }
            
        	public User User { get; set; }		
            
            public bool CheckAccess { get; set; }
    }
}