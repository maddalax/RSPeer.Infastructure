using MediatR;

namespace RSPeer.Application.Features.Groups.Queries
{
    public class HasGroupQuery : IRequest<bool>
    {
        public int UserId { get; set; }
        public int[] GroupIds { get; }

        public HasGroupQuery(int userId, params int[] groupIds)
        {
            UserId = userId;
            GroupIds = groupIds;
        }
    }
}