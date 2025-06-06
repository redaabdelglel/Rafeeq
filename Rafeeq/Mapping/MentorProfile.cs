// Mapping/MentorProfile.cs
using AutoMapper;
using Rafeeq.DTOs.Availability;
using Rafeeq.DTOs.Users;
using Rafeeq.Models;
using System;

namespace Rafeeq.Mapping
{
    public class MentorProfile : Profile
    {
        public MentorProfile()
        {
            CreateMap<User, MentorDto>()
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src =>
                    src.MentorSkills.Select(ms => ms.Skill.Name).ToList()))
                .ForMember(dest => dest.Availabilities, opt => opt.MapFrom(src =>
                    src.Availabilities.Select(a => new AvailabilityDto
                    {
                        AvailabilityId = a.AvailabilityId , // int? to int
                        //DayOfWeek = a.DayOfWeek , // string? to string
                        StartTime = a.StartTime ?? TimeSpan.Zero, // TimeSpan? to TimeSpan
                        EndTime = a.EndTime ?? TimeSpan.Zero // TimeSpan? to TimeSpan
                    }).ToList()));
        }
    }
}