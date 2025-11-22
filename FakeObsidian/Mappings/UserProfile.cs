using AutoMapper;
using FakeObsidian.Api.Models.User;
using FakeObsidian.Application.DTO;
using FakeObsidian.Domain.Entities;

namespace FakeObsidian.Api.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile() {
            CreateMap<AppUser, UserResponse>();
            CreateMap<AppUser, LimitedUserResponse>();

            CreateMap<UserDto, UserResponse>();
            CreateMap<UserDto, LimitedUserResponse>();

            CreateMap<UpdateUserRequest, AppUser>();
        }
    }
}
