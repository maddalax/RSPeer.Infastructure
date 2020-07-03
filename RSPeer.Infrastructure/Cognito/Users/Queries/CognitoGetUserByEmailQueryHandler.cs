using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Configuration;
using RSPeer.Domain.Entities;
using RSPeer.Infrastructure.Cognito.Users.Base;

namespace RSPeer.Infrastructure.Cognito.Users.Queries
{
	public class CognitoGetUserByEmailQueryHandler : BaseCognitoHandler<CognitoGetUserByEmailQuery, User>
	{
		public CognitoGetUserByEmailQueryHandler(IConfiguration configuration) : base(configuration)
		{
		}

		public override async Task<User> Handle(CognitoGetUserByEmailQuery request, CancellationToken cancellationToken)
		{
			using (var provider = new AmazonCognitoIdentityProviderClient(AwsId, AwsKey, RegionEndpoint.USEast1))
			{
				var res = await provider.ListUsersAsync(new ListUsersRequest
				{
					Filter = $"email = \"{request.Email}\"",
					UserPoolId = UserPoolId
				});
				return res.Users.Select(t => ParseToUser(t.Attributes)).FirstOrDefault();
			}
		}
	}
}