using AuthService.Data.Repositories.Interfaces;
using Grpc.Core;

namespace AuthService.SyncDataServices.Grpc
{
    public class GrpcUserService : GrpcUsers.GrpcUsersBase
    {
        private readonly IUserRepository _repository;

        public GrpcUserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public override async Task<GetAllResponse> GetAllUsers(GetAllRequest request, ServerCallContext context)
        {
            var response = new GetAllResponse();
            var users = await _repository.GetAll();

            foreach (var user in users)
            {
                response.Users.Add(new GrpcUserModel() { UserId = user.Id });
            }

            return response;
        }
    }
}