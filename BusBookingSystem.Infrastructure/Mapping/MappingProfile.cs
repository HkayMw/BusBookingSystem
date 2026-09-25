using AutoMapper;
using BusBookingSystem.Application.DTOs;
using BusBookingSystem.Core.Entities;

namespace BusBookingSystem.Infrastructure.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Trip, TripDto>();
            CreateMap<User, UserDto>();
        }
    }
}